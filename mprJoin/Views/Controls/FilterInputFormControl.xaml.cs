namespace mprJoin.Views.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Models;

    public partial class FilterInputFormControl : UserControl
    {
        public static readonly DependencyProperty RemoveCommandProperty = DependencyProperty.Register(
            "RemoveCommand", typeof(ICommand), typeof(FilterInputFormControl), new PropertyMetadata(default(ICommand)));
        
        public static readonly DependencyProperty FilterModelProperty = DependencyProperty.Register(
            "FilterModel", typeof(FilterModel), typeof(FilterInputFormControl), new PropertyMetadata(default(FilterModel)));
        
        public FilterInputFormControl()
        {
            InitializeComponent();
            
            ModPlusAPI.Windows.Helpers.WindowHelpers.ChangeStyleForResourceDictionary(Resources);
            ModPlusAPI.Language.SetLanguageProviderForResourceDictionary(Resources);
            ModPlusAPI.Language.SetLanguageProviderForResourceDictionary(Resources, "LangCommon");
        }

        /// <summary>
        /// Команда удаления строки.
        /// </summary>
        public ICommand RemoveCommand
        {
            get => (ICommand)GetValue(RemoveCommandProperty);
            set => SetValue(RemoveCommandProperty, value);
        }
        
        public FilterModel FilterModel
        {
            get => (FilterModel)GetValue(FilterModelProperty);
            set => SetValue(FilterModelProperty, value);
        }
    }
}