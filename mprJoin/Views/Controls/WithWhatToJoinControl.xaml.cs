namespace mprJoin.Views.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using Models;

    /// <summary>
    /// Логика взаимодействия для WithWhatToJoinControl.xaml
    /// </summary>
    public partial class WithWhatToJoinControl
    {
        public static readonly DependencyProperty PairProperty = DependencyProperty.Register(
            "Pair", typeof(CustomElementPair), typeof(WithWhatToJoinControl), new PropertyMetadata(default(CustomElementPair)));

        public WithWhatToJoinControl()
        {
            InitializeComponent();
            
            ModPlusAPI.Windows.Helpers.WindowHelpers.ChangeStyleForResourceDictionary(Resources);
            
            ChangeTextWidth();
        }

        public CustomElementPair Pair
        {
            get => (CustomElementPair)GetValue(PairProperty);
            set => SetValue(PairProperty, value);
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

        private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox.SelectedItem != null)
                ComboBox.SelectedItem = null;
        }
    }
}
