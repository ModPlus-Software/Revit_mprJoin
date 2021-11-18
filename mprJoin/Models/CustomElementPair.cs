namespace mprJoin.Models
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
        private bool _showFilters;

        public CustomElementPair(List<SelectedCategory> selectedCategories)
        {
            WithWhatToJoin = new SelectedCategoriesStorage(selectedCategories);
            FiltersForMainCategory.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasFilters));
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
        public SelectedCategoriesStorage WithWhatToJoin { get; }
        
        /// <summary>
        /// Элементы которые будут будут иметь высший приоритет при соединении
        /// </summary>
        public List<Element> WhatToJoinElements { get; set; }
        
        /// <summary>
        /// Элементы которые будут присоединяться (т.е. у них будут образовываться вырезы)
        /// </summary>
        public List<Element> WhereToJoinElements { get; set; }

        /// <summary>
        /// Список фильтров для основной категории, к которой будут присоединяться другие элементы.
        /// </summary>
        public ObservableCollection<FilterModel> FiltersForMainCategory { get; set; } = new ();

        /// <summary>
        /// Список фильтров для категорий которые будут присоединяться
        /// </summary>
        public ObservableCollection<FilterModel> FilterModelsForSubCategories { get; set; } = new ();

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

        public bool HasFilters => FiltersForMainCategory.Any() || FilterModelsForSubCategories.Any();

        /// <summary>
        /// Получить модель для сохранения
        /// </summary>
        public CustomElementPairForSave ToSaveMode()
        {
            var saveMode = new CustomElementPairForSave
            {
                WhatToJoinCategory = WhatToJoinCategory,
                WithWhatToJoin = WithWhatToJoin.SelectedCategories,
                LogicConditions = LogicConditions,
                FiltersForMainCategory = FiltersForMainCategory,
                FilterModelsForSubCategories = FilterModelsForSubCategories,
                ShowFilters = ShowFilters
            };
            return saveMode;
        }
    }
}