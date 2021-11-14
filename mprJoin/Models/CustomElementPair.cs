﻿namespace mprJoin.Models
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Autodesk.Revit.DB;
    using Enums;
    using ModPlusAPI.Mvvm;

    /// <summary>
    /// Модель данных с информацией о используемых категориях для соединения и правилах
    /// </summary>
    public class CustomElementPair : ObservableObject
    {
        private string _whatToJoinCategory;
        private List<SelectedCategory> _withWhatToJoin;
        private bool _showFilters;

        public CustomElementPair()
        {
            Filters.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasFilters));
        }

        /// <summary>
        /// Категория элементов которые будут будут иметь высший приоритет при соединении
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
        /// Категория которые будут присоединяться (т.е. у них будут образовываться вырезы)
        /// </summary>
        public List<SelectedCategory> WithWhatToJoin
        {
            get => _withWhatToJoin;
            set
            {
                _withWhatToJoin = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Элементы которые будут будут иметь высший приоритет при соединении
        /// </summary>
        public List<Element> WhatToJoinElements { get; set; }
        
        /// <summary>
        /// Элементы которые будут присоединяться (т.е. у них будут образовываться вырезы)
        /// </summary>
        public List<Element> WhereToJoinElements { get; set; }

        /// <summary>
        /// Список фильтров
        /// </summary>
        public ObservableCollection<FilterModel> Filters { get; set; } = new ();
        
        /// <summary>
        /// Логический оператор.
        /// </summary>
        public LogicConditions LogicConditions { get; set; }

        /// <summary>
        /// Показывать фильтры.
        /// </summary>
        public bool ShowFilters
        {
            get => _showFilters;
            set
            {
                _showFilters = value;
                OnPropertyChanged();
            }
        }

        public bool HasFilters => Filters.Any();

        /// <summary>
        /// Получить модель для сохранения
        /// </summary>
        public CustomElementPairForSave ToSaveMode()
        {
            var saveMode = new CustomElementPairForSave
            {
                WhatToJoinCategory = WhatToJoinCategory,
                WithWhatToJoin = WithWhatToJoin,
                LogicConditions = LogicConditions,
                Filters = Filters,
                ShowFilters = ShowFilters
            };
            return saveMode;
        }
    }
}