namespace mprJoin.Views.Pages
{
    using System.Windows;

    public partial class ContiguityControl 
    {
        public ContiguityControl()
        {
            InitializeComponent();

            ModPlusAPI.Windows.Helpers.WindowHelpers.ChangeStyleForResourceDictionary(Resources);
            ModPlusAPI.Language.SetLanguageProviderForResourceDictionary(Resources);
            ModPlusAPI.Language.SetLanguageProviderForResourceDictionary(Resources, "LangCommon");
        }

        private void BtCreate_OnClick(object sender, RoutedEventArgs e)
        {
            PopupCreate.IsOpen = !PopupCreate.IsOpen;
        }

        private void BtClosePopupCreate_OnClick(object sender, RoutedEventArgs e)
        {
            PopupCreate.IsOpen = false;
        }
    }
}