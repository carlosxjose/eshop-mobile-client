﻿using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using eShopOnContainers.Services;
using eShopOnContainers.Services.AppEnvironment;
using eShopOnContainers.Services.Location;
using eShopOnContainers.Services.Settings;
using eShopOnContainers.ViewModels.Base;

namespace eShopOnContainers.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly ILocationService _locationService;
    private readonly IAppEnvironmentService _appEnvironmentService;

    private bool _useAzureServices;
    private bool _allowGpsLocation;
    private bool _useFakeLocation;
    private string _identityEndpoint;
    private string _gatewayShoppingEndpoint;
    private string _gatewayMarketingEndpoint;
    private double _latitude;
    private double _longitude;
    private string _gpsWarningMessage;

    public static string TitleUseAzureServices => "Use Microservices/Containers from eShopOnContainers";

    public string DescriptionUseAzureServices => !UseAzureServices
                ? "Currently using mock services that are simulated objects that mimic the behavior of real services using a controlled approach. Toggle on to configure the use of microserivces/containers."
                : "When enabling the use of microservices/containers, the app will attempt to use real services deployed as Docker/Kubernetes containers at the specified base endpoint, which will must be reachable through the network.";

    public bool UseAzureServices
    {
        get => _useAzureServices;
        set
        {
            SetProperty(ref _useAzureServices, value);
            UpdateUseAzureServices();
        }
    }

    public string TitleUseFakeLocation => !UseFakeLocation
                ? "Use Real Location"
                : "Use Fake Location";

    public string DescriptionUseFakeLocation => !UseFakeLocation
                ? "When enabling location, the app will attempt to use the location from the device."
                : "Fake Location data is added for marketing campaign testing.";

    public bool UseFakeLocation
    {
        get => _useFakeLocation;
        set
        {
            SetProperty(ref _useFakeLocation, value);
            UpdateFakeLocation();
        }
    }

    public string TitleAllowGpsLocation => !AllowGpsLocation
                ? "GPS Location Disabled"
                : "GPS Location Enabled";

    public string DescriptionAllowGpsLocation => !AllowGpsLocation
                ? "When disabling location, you won't receive location campaigns based upon your location."
                : "When enabling location, you'll receive location campaigns based upon your location.";

    public string GpsWarningMessage
    {
        get => _gpsWarningMessage;
        set => SetProperty(ref _gpsWarningMessage, value);
    }

    public string IdentityEndpoint
    {
        get => _identityEndpoint;
        set
        {
            SetProperty(ref _identityEndpoint, value);
            if (!string.IsNullOrEmpty(value))
            {
                UpdateIdentityEndpoint();
            }
        }
    }

    public string GatewayShoppingEndpoint
    {
        get => _gatewayShoppingEndpoint;
        set
        {
            SetProperty(ref _gatewayShoppingEndpoint, value);
            if (!string.IsNullOrEmpty(value))
            {
                UpdateGatewayShoppingEndpoint();
            }
        }
    }

    public string GatewayMarketingEndpoint
    {
        get => _gatewayMarketingEndpoint;
        set
        {
            SetProperty(ref _gatewayMarketingEndpoint, value);
            if (!string.IsNullOrEmpty(value))
            {
                UpdateGatewayMarketingEndpoint();
            }
        }
    }

    public double Latitude
    {
        get => _latitude;
        set
        {
            SetProperty(ref _latitude, value);
            UpdateLatitude();
        }
    }

    public double Longitude
    {
        get => _longitude;
        set
        {
            SetProperty(ref _longitude, value);
            UpdateLongitude();
        }
    }

    public bool AllowGpsLocation
    {
        get => _allowGpsLocation;
        set => SetProperty(ref _allowGpsLocation, value);
    }

    public bool UserIsLogged => !string.IsNullOrEmpty(_settingsService.AuthAccessToken);

    public ICommand ToggleMockServicesCommand { get; }

    public ICommand ToggleFakeLocationCommand { get; }

    public ICommand ToggleSendLocationCommand { get; }

    public ICommand ToggleAllowGpsLocationCommand { get; }

    public SettingsViewModel(
        ILocationService locationService, IAppEnvironmentService appEnvironmentService,
        INavigationService navigationService, ISettingsService settingsService)
        : base(navigationService)
    {
        _settingsService = settingsService;
        _locationService = locationService;
        _appEnvironmentService = appEnvironmentService;

        _useAzureServices = !_settingsService.UseMocks;
        _identityEndpoint = _settingsService.IdentityEndpointBase;
        _gatewayShoppingEndpoint = _settingsService.GatewayShoppingEndpointBase;
        _gatewayMarketingEndpoint = _settingsService.GatewayMarketingEndpointBase;
        _latitude = double.Parse(_settingsService.Latitude, CultureInfo.CurrentCulture);
        _longitude = double.Parse(_settingsService.Longitude, CultureInfo.CurrentCulture);
        _useFakeLocation = _settingsService.UseFakeLocation;
        _allowGpsLocation = _settingsService.AllowGpsLocation;
        _gpsWarningMessage = string.Empty;

        ToggleMockServicesCommand = new RelayCommand(ToggleMockServices);

        ToggleFakeLocationCommand = new RelayCommand(ToggleFakeLocation);

        ToggleSendLocationCommand = new AsyncRelayCommand(ToggleSendLocationAsync);

        ToggleAllowGpsLocationCommand = new RelayCommand(ToggleAllowGpsLocation);

        UseAzureServices = !_settingsService.UseMocks;
    }

    protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(AllowGpsLocation))
        {
            await UpdateAllowGpsLocation();
        }
    }

    private void ToggleMockServices()
    {
        _appEnvironmentService.UpdateDependencies(!UseAzureServices);

        OnPropertyChanged(nameof(TitleUseAzureServices));
        OnPropertyChanged(nameof(DescriptionUseAzureServices));

        //TODO: We should re-evaluate this workflow
        if (UseAzureServices)
        {
            _settingsService.AuthAccessToken = string.Empty;
            _settingsService.AuthIdToken = string.Empty;
        }
    }

    private void ToggleFakeLocation()
    {
        _appEnvironmentService.UpdateDependencies(!UseAzureServices);
        OnPropertyChanged(nameof(TitleUseFakeLocation));
        OnPropertyChanged(nameof(DescriptionUseFakeLocation));
    }

    private async Task ToggleSendLocationAsync()
    {
        if (!_settingsService.UseMocks)
        {
            var locationRequest = new Models.Location.Location
            {
                Latitude = _latitude,
                Longitude = _longitude
            };

            var authToken = _settingsService.AuthAccessToken;

            await _locationService.UpdateUserLocation(locationRequest, authToken);
        }
    }

    private void ToggleAllowGpsLocation()
    {
        OnPropertyChanged(nameof(TitleAllowGpsLocation));
        OnPropertyChanged(nameof(DescriptionAllowGpsLocation));
    }

    private void UpdateUseAzureServices()
    {
        // Save use mocks services to local storage
        _settingsService.UseMocks = !UseAzureServices;
    }

    private void UpdateIdentityEndpoint()
    {
        // Update remote endpoint (save to local storage)
        GlobalSetting.Instance.BaseIdentityEndpoint = _settingsService.IdentityEndpointBase = _identityEndpoint;
    }

    private void UpdateGatewayShoppingEndpoint()
    {
        GlobalSetting.Instance.BaseGatewayShoppingEndpoint = _settingsService.GatewayShoppingEndpointBase = _gatewayShoppingEndpoint;
    }

    private void UpdateGatewayMarketingEndpoint()
    {
        GlobalSetting.Instance.BaseGatewayMarketingEndpoint = _settingsService.GatewayMarketingEndpointBase = _gatewayMarketingEndpoint;
    }

    private void UpdateFakeLocation()
    {
        _settingsService.UseFakeLocation = _useFakeLocation;
    }

    private void UpdateLatitude()
    {
        // Update fake latitude (save to local storage)
        _settingsService.Latitude = _latitude.ToString();
    }

    private void UpdateLongitude()
    {
        // Update fake longitude (save to local storage)
        _settingsService.Longitude = _longitude.ToString();
    }

    private async Task UpdateAllowGpsLocation()
    {
        if (_allowGpsLocation)
        {
            bool hasWhenInUseLocationPermissions;
            bool hasBackgroundLocationPermissions;

            if (await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>() != PermissionStatus.Granted)
            {
                hasWhenInUseLocationPermissions = await Permissions.RequestAsync<Permissions.LocationWhenInUse>() == PermissionStatus.Granted;
            }
            else
            {
                hasWhenInUseLocationPermissions = true;
            }

            if (await Permissions.CheckStatusAsync<Permissions.LocationAlways>() != PermissionStatus.Granted)
            {
                hasBackgroundLocationPermissions = await Permissions.RequestAsync<Permissions.LocationAlways>() == PermissionStatus.Granted;
            }
            else
            {
                hasBackgroundLocationPermissions = true;
            }


            if (!hasWhenInUseLocationPermissions || !hasBackgroundLocationPermissions)
            {
                _allowGpsLocation = false;
                GpsWarningMessage = "Enable the GPS sensor on your device";
            }
            else
            {
                _settingsService.AllowGpsLocation = _allowGpsLocation;
                GpsWarningMessage = string.Empty;
            }
        }
        else
        {
            _settingsService.AllowGpsLocation = _allowGpsLocation;
        }
    }
}
