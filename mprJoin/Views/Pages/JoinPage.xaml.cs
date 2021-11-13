using System.Windows.Controls;

namespace mprJoin.Views.Pages
{
    public partial class JoinPage 
    {
        public JoinPage()
        {
            InitializeComponent();

            ModPlusAPI.Windows.Helpers.WindowHelpers.ChangeStyleForResourceDictionary(Resources);
            ModPlusAPI.Language.SetLanguageProviderForResourceDictionary(Resources);
            ModPlusAPI.Language.SetLanguageProviderForResourceDictionary(Resources, "LangCommon");
        }
    }
}