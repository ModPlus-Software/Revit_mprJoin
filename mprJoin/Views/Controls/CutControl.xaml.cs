namespace mprJoin.Views.Controls;

public partial class CutControl 
{
    public CutControl()
    {
        InitializeComponent();

        ModPlusAPI.Windows.Helpers.WindowHelpers.ChangeStyleForResourceDictionary(Resources);
        ModPlusAPI.Language.SetLanguageProviderForResourceDictionary(Resources);
        ModPlusAPI.Language.SetLanguageProviderForResourceDictionary(Resources, "LangCommon");
    }
}