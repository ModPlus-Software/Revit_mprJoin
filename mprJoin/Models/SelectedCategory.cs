namespace mprJoin.Models
{
    using ModPlusAPI.Mvvm;

    public class SelectedCategory : ObservableObject
    {
        private bool _isSelected;

        /// <summary>
        /// Является ли категория выбранной
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Имя категории
        /// </summary>
        public string Name { get; set; }
    }
}