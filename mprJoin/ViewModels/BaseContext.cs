namespace mprJoin.ViewModels;
using ModPlusAPI.Mvvm;
using ModPlusAPI.Services;
using Views;

/// <summary>
/// Базовый класс контекста для страниц
/// </summary>
public abstract class BaseContext : ObservableObject
{    
    protected BaseContext(MainWindow mainWindow, UserSettingsService userSettingsService)
    {
        MainWindow = mainWindow;
        UserSettingsService = userSettingsService;
    }

    /// <summary>
    /// <see cref="Views.MainWindow"/> instance
    /// </summary>
    public MainWindow MainWindow { get; }

    /// <summary>
    /// User settings service
    /// </summary>
    public UserSettingsService UserSettingsService { get; }

    public abstract void SaveSettings();
}