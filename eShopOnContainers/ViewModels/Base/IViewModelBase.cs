﻿using eShopOnContainers.Services;
using eShopOnContainers.Services.Settings;

namespace eShopOnContainers.ViewModels.Base;

public interface IViewModelBase : IQueryAttributable
{
    public INavigationService NavigationService { get; }

    public bool IsBusy { get; }

    public bool IsInitialized { get; set; }

    Task InitializeAsync();
}