namespace mprJoin.Views
{
    using System.Windows.Controls;
    using Pages;
    using ViewModels;

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Контекст страницы
        /// </summary>
        public ContiguityPageContext ContiguityPageContext { get; set; }
        
        /// <summary>
        /// Контекст страницы присоединения элементов
        /// </summary>
        public JoinPageContext JoinPageContext { get; set; }
        
        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            var frameContent = MainFrame.Content;
            if (frameContent is not Page page)
                return;

            page.DataContext = frameContent switch
            {
                ContiguityPage _ => ContiguityPageContext,
                JoinPage _ => JoinPageContext,
                _ => page.DataContext
            };
        }
    }
}
