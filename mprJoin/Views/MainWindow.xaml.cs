namespace mprJoin.Views;

/// <summary>
/// Логика взаимодействия для MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();

        Title = ModPlusAPI.Language.GetPluginLocalName(new ModPlusConnector());
        ModPlusAPI.Windows.Helpers.WindowHelpers.ChangeStyleForResourceDictionary(Resources);
        ModPlusAPI.Language.SetLanguageProviderForResourceDictionary(Resources);
        ModPlusAPI.Language.SetLanguageProviderForResourceDictionary(Resources, "LangCommon");
    }
}