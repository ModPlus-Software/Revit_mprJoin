namespace mprJoin.Views.Controls
{
    public partial class ContiguityControl 
    {
        public ContiguityControl()
        {
            InitializeComponent();

            ModPlusAPI.Windows.Helpers.WindowHelpers.ChangeStyleForResourceDictionary(Resources);
            ModPlusAPI.Language.SetLanguageProviderForResourceDictionary(Resources);
            ModPlusAPI.Language.SetLanguageProviderForResourceDictionary(Resources, "LangCommon");
        }
    }
}