using TrialWorld.Presentation.Interfaces;
using TrialWorld.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace TrialWorld.Presentation.Services;

public class NavigationService : INavigationService
{
    public NavigationService()
    {
        Console.WriteLine("NavigationService initialized - Placeholder");
    }
    
    public void Navigate(string viewName)
    {
        // TODO: Implement navigation
    }
    
    public void NavigateToViewModel<TViewModel>() where TViewModel : class
    {
        // TODO: Implement view model navigation
    }
    
    public bool CanGoBack { get; private set; }
    
    public void GoBack()
    {
        // TODO: Implement back navigation
    }
} 