namespace mprJoin.Models
{
    using System.Collections.ObjectModel;
    using System.Windows.Input;
    using Enums;
    using ModPlusAPI.Mvvm;

    /// <summary>
    /// Модель фильтра.
    /// </summary>
    public class Filter : ObservableObject
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
        
        public Condition Condition { get; set; }
    }
}