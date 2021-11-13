namespace mprJoin.Models
{
    using ModPlusAPI.Mvvm;

    /// <summary>
    /// Модель данных с информацией о используемых категориях для соединения и правилах
    /// </summary>
    public class CustomElementPair : ObservableObject
    {
        private string _whatToJoinCategory;
        private string _withWhatToJoin;

        /// <summary>
        /// Категория элементов которые будут вырезаться
        /// </summary>
        public string WhatToJoinCategory
        {
            get => _whatToJoinCategory;
            set
            {
                _whatToJoinCategory = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Категория откуда будут вырезаться элементы
        /// </summary>
        public string WithWhatToJoin
        {
            get => _withWhatToJoin;
            set
            {
                _withWhatToJoin = value;
                OnPropertyChanged();
            }
        }
    }
}