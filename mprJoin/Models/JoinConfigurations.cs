namespace mprJoin.Models
{
    using System.Collections.ObjectModel;
    using ModPlusAPI.Mvvm;

    /// <summary>
    /// Объект конфигурации, которая будет сохранена в файл.
    /// </summary>
    public class JoinConfigurations : ObservableObject
    {
        private string _name;

        /// <summary>
        /// Имя конфигурации.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<CustomElementPair> Pairs { get; set; } = new ();

        /// <summary>
        /// Является ли объект изменяемым.
        /// </summary>
        public bool IsEditable { get; set; } = true;
    }
}
