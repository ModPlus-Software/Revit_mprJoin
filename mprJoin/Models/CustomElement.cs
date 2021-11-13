namespace mprJoin.Models
{
    using Autodesk.Revit.DB;
    using ModPlusAPI.Mvvm;

    public class CustomElement : ObservableObject
    {
        private bool _isSelected;
        public Element RevitElement { get; set; }

        public string Category => RevitElement.Category.Name;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }
}