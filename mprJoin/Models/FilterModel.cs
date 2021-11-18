namespace mprJoin.Models
{
    using Enums;
    using ModPlusAPI.Mvvm;

    /// <summary>
    /// Модель фильтра.
    /// </summary>
    public class FilterModel : ObservableObject
    {
        private string _parameterName;
        private string _parameterValue;

        /// <summary>
        /// Имя параметра.
        /// </summary>
        public string ParameterName
        {
            get => _parameterName;
            set
            {
                _parameterName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Значение параметра.
        /// </summary>
        public string ParameterValue
        {
            get => _parameterValue;
            set
            {
                _parameterValue = value;
                OnPropertyChanged();
            }
        }
        
        public Conditions Conditions { get; set; }
    }
}