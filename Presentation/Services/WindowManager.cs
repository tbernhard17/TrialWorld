using TrialWorld.Presentation.Interfaces;
using System;
using System.Windows;
using TrialWorld.Presentation.Views;

namespace TrialWorld.Presentation.Services
{
    public class WindowManager : IWindowManager
    {
        public bool ShowDialog<TViewModel>() where TViewModel : class
        {
            // TODO: Implement dialog showing logic
            return false;
        }
        
        public bool ShowDialog<TViewModel>(object parameters) where TViewModel : class
        {
            // TODO: Implement dialog showing logic with parameters
            return false;
        }
        
        public void ShowWindow<TViewModel>() where TViewModel : class
        {
            // TODO: Implement window showing logic
        }
    }
}
