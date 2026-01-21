using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ComputerController.ViewModels;
using ComputerController.Views;
using ComputerController.Services;

namespace ComputerController;

public sealed partial class MainWindow : Window
{
    private readonly SettingsService _settingsService;
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        this.InitializeComponent();
        _viewModel = viewModel;
        _settingsService = App.Services.GetService(typeof(SettingsService)) as SettingsService 
            ?? throw new InvalidOperationException("SettingsService not found");
        
        // Set window size
        this.AppWindow.Resize(new Windows.Graphics.SizeInt32(1000, 700));

        // Apply saved theme immediately
        ApplyTheme();

        // Subscribe to theme changes
        if (App.Services.GetService(typeof(SettingsViewModel)) is SettingsViewModel settingsViewModel)
        {
            settingsViewModel.PropertyChanged += (s, e) => 
            {
                if (e.PropertyName == nameof(SettingsViewModel.IsDarkMode))
                {
                    DispatcherQueue.TryEnqueue(ApplyTheme);
                }
            };
        }
        
        // Navigate to Computer Control by default
        ContentFrame.Navigate(typeof(ComputerControlView));
    }

    private void ApplyTheme()
    {
        if (Content is FrameworkElement rootElement)
        {
            var isDark = _settingsService.GetIsDarkMode();
            rootElement.RequestedTheme = isDark ? ElementTheme.Dark : ElementTheme.Light;
        }
    }
}
