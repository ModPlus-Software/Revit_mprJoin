namespace mprJoin;

using System.Collections.Generic;
using System.Windows.Input;
using ModPlusAPI.Mvvm;
using ModPlusAPI.Services;
using Settings;
using ViewModels;
using Views;

/// <summary>
/// Main context
/// </summary>
public class MainContext : ObservableObject
{
    private readonly List<BaseContext> _contexts;
    private readonly UserSettingsService _userSettingsService;

    public MainContext(MainWindow mainWindow)
    {
        _userSettingsService = new UserSettingsService(PluginSetting.SaveFileName);
        ContiguityContext = new ContiguityContext(mainWindow, _userSettingsService);
        JoinContext = new JoinContext(mainWindow, _userSettingsService);
        CutContext = new CutContext(mainWindow, _userSettingsService);
        _contexts = new List<BaseContext>
        {
            ContiguityContext,
            JoinContext,
            CutContext
        };
            
        SelectedTab = _userSettingsService.Get<int>(nameof(SelectedTab));
    }

    public int SelectedTab
    {
        get; set;
    }
        
    public ContiguityContext ContiguityContext { get; }
        
    public JoinContext JoinContext { get; }
        
    public CutContext CutContext { get; }

    public ICommand SaveAllConfiguration =>
        new RelayCommandWithoutParameter(() =>
        {
            _contexts.ForEach(i => i.SaveSettings());
            _userSettingsService.Set(SelectedTab, nameof(SelectedTab));
        });
}