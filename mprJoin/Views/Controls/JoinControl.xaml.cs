namespace mprJoin.Views.Controls
{
    public partial class JoinControl 
    {
        public JoinControl()
        {
            InitializeComponent();

            ModPlusAPI.Windows.Helpers.WindowHelpers.ChangeStyleForResourceDictionary(Resources);
            ModPlusAPI.Language.SetLanguageProviderForResourceDictionary(Resources);
        }
    }
}