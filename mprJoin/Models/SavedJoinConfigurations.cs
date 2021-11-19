namespace mprJoin.Models
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Abstractions;
    using ModPlusAPI.Mvvm;

    /// <summary>
    /// Объект конфигурации, которая будет сохранена в файл.
    /// </summary>
    public class SavedJoinConfigurations : ObservableObject, ICloneable<SavedJoinConfigurations>
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
        
        /// <inheritdoc/>
        public SavedJoinConfigurations Clone()
        {
            return new SavedJoinConfigurations
            {
                Name = _name,
                Pairs = Pairs
            };
        }
    }
}
