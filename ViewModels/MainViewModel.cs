using CommunityToolkit.Mvvm.ComponentModel;

namespace ComputerController.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object? _currentPage;

    public ComputerControlViewModel ComputerControlViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }

    public MainViewModel(ComputerControlViewModel computerControlViewModel, SettingsViewModel settingsViewModel)
    {
        ComputerControlViewModel = computerControlViewModel;
        SettingsViewModel = settingsViewModel;
        
        // Start with Computer Control view
        _currentPage = ComputerControlViewModel;
    }

    public void NavigateToComputerControl()
    {
        CurrentPage = ComputerControlViewModel;
        ComputerControlViewModel.UpdateConnectionStatus();
    }

    public void NavigateToSettings()
    {
        CurrentPage = SettingsViewModel;
    }

    public void OnProviderChanged()
    {
        // Update both view models when provider changes
        ComputerControlViewModel.UpdateProvider(SettingsViewModel.SelectedProvider);
    }
}
