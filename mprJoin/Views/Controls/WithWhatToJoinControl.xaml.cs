﻿namespace mprJoin.Views.Controls
{
    using System.Windows;
    using Models;

    /// <summary>
    /// Логика взаимодействия для WithWhatToJoinControl.xaml
    /// </summary>
    public partial class WithWhatToJoinControl
    {
        public static readonly DependencyProperty StorageProperty = DependencyProperty.Register(
            "Storage", typeof(CustomElementPair), typeof(WithWhatToJoinControl), new PropertyMetadata(default(CustomElementPair)));

        public WithWhatToJoinControl()
        {
            InitializeComponent();
            
            ModPlusAPI.Windows.Helpers.WindowHelpers.ChangeStyleForResourceDictionary(Resources);
            
            ChangeTextWidth();
        }

        public CustomElementPair Storage
        {
            get => (CustomElementPair)GetValue(StorageProperty);
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
