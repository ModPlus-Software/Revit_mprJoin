using ModPlusAPI.Services;
using mprJoin.Settings;

namespace mprJoin;

using System.Collections.Generic;
using System.Windows.Input;
using Autodesk.Revit.UI;
using ModPlusAPI.Mvvm;
using ViewModels;
using Views;

/// <summary>
/// Main context
/// </summary>
public class MainContext : ObservableObject
{
    private readonly List<BaseContext> _contexts;
    private readonly UserSettingsService _userSettingsService;

    public MainContext(UIApplication uiApplication, MainWindow mainWindow)
    {
        _userSettingsService = new UserSettingsService(PluginSetting.SaveFileName);
        ContiguityContext = new ContiguityContext(uiApplication, mainWindow, _userSettingsService);
        JoinContext = new JoinContext(uiApplication, mainWindow, _userSettingsService);
        CutContext = new CutContext(uiApplication, mainWindow, _userSettingsService);
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