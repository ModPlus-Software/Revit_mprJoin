namespace mprJoin.Views.Pages
{
    using System.Windows;
    using System.Windows.Controls;
    using mprJoin.ViewModels;

    public partial class ContiguityPage : Page
    {
        public ContiguityPage()
        {
            InitializeComponent();
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