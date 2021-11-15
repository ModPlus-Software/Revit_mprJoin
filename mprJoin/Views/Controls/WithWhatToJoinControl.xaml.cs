namespace mprJoin.Views.Controls
{
    using System.Windows;
    using Models;

    /// <summary>
    /// Логика взаимодействия для WithWhatToJoinControl.xaml
    /// </summary>
    public partial class WithWhatToJoinControl
    {
        public static readonly DependencyProperty StorageProperty = DependencyProperty.Register(
            "Storage", typeof(SelectedCategoriesStorage), typeof(WithWhatToJoinControl), new PropertyMetadata(default(SelectedCategoriesStorage)));

        public WithWhatToJoinControl()
        {
            InitializeComponent();
            ChangeTextWidth();
        }

        public SelectedCategoriesStorage Storage
        {
            get => (SelectedCategoriesStorage)GetValue(StorageProperty);
            set => SetValue(StorageProperty, value);
        }

        private void ComboBox_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChangeTextWidth();
        }

        private void ChangeTextWidth()
        {
            if (!double.IsNaN(ComboBox.ActualWidth) && ComboBox.ActualWidth > 0.0)
                TextBlock.Width = ComboBox.ActualWidth - 40;
        }
    }
}
