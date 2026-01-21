using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using ComputerController.Services;
using ComputerController.ViewModels;
using ComputerController.Views;

namespace ComputerController;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static Window MainWindow { get; private set; } = null!;

    public App()
    {
        this.InitializeComponent();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register services
        services.AddSingleton<SettingsService>();
        services.AddSingleton<LLMService>();
        services.AddSingleton<PythonExecutor>();

        // Register view models
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<ComputerControlViewModel>();
        services.AddSingleton<SettingsViewModel>();

        // Register views
        services.AddTransient<MainWindow>();
        services.AddTransient<ComputerControlView>();
        services.AddTransient<SettingsView>();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Setup dependency injection after WinUI is initialized
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();
        
        MainWindow = Services.GetRequiredService<MainWindow>();
        MainWindow.Activate();
    }
}
