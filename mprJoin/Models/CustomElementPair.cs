namespace mprJoin.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Xml.Serialization;
    using Autodesk.Revit.DB;
    using Enums;
    using Extensions;
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
        [XmlIgnore]
        public List<Element> WhatToJoinElements { get; set; }
        
        /// <summary>
        /// Элементы которые будут присоединяться (т.е. у них будут образовываться вырезы)
        /// </summary>
        [XmlIgnore]
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
        public LogicCondition LogicCondition { get; set; }

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
                LogicCondition = LogicCondition,
                FiltersForMainCategory = FiltersForMainCategory,
                FilterModelsForSubCategories = FilterModelsForSubCategories,
                ShowFilters = ShowFilters
            };
            return saveMode;
        }

        public void ApplyFilters()
        {
            switch (LogicCondition)
            {
                case LogicCondition.And:
                    WhatToJoinElements = WhatToJoinElements.Where(el =>
                        FiltersForMainCategory.Where(filter => !string.IsNullOrEmpty(filter.ParameterName))
                            .All(filter => IsMatchByFilter(el, filter))).ToList();

                    WhereToJoinElements = WhereToJoinElements.Where(el =>
                        FilterModelsForSubCategories.Where(filter => !string.IsNullOrEmpty(filter.ParameterName))
                            .All(filter => IsMatchByFilter(el, filter))).ToList();
                    break;
                case LogicCondition.Or:
                    WhatToJoinElements = WhatToJoinElements.Where(el =>
                        FiltersForMainCategory.Where(filter => !string.IsNullOrEmpty(filter.ParameterName))
                            .Any(filter => IsMatchByFilter(el, filter))).ToList();

                    WhereToJoinElements = WhereToJoinElements.Where(el =>
                        FilterModelsForSubCategories.Where(filter => !string.IsNullOrEmpty(filter.ParameterName))
                            .Any(filter => IsMatchByFilter(el, filter))).ToList();

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Соответствие элемента фильтру.
        /// </summary>
        /// <param name="element">Элемент.</param>
        /// <param name="filter">Фильтр.</param>
        private bool IsMatchByFilter(Element element, FilterModel filter)
        {
            var param = element.GetParameterFromInstanceOrType(filter.ParameterName);
            if (param == null)
                return false;

            if (string.IsNullOrEmpty(filter.ParameterValue))
                return true;
            
            var paramValue = param.GetParameterValue();
            switch (filter.Condition)
            {
                case Condition.Begin:
                    return paramValue.StartsWith(filter.ParameterValue, StringComparison.InvariantCultureIgnoreCase);
                case Condition.NotBegin:
                    return !paramValue.StartsWith(filter.ParameterValue, StringComparison.InvariantCultureIgnoreCase);
                case Condition.Contains:
                    return paramValue.IndexOf(filter.ParameterValue, StringComparison.InvariantCultureIgnoreCase) > 0;
                case Condition.NotContains:
                    return paramValue.IndexOf(filter.ParameterValue, StringComparison.InvariantCultureIgnoreCase) == 0;
                case Condition.Equals:
                    return paramValue.Equals(filter.ParameterValue, StringComparison.InvariantCultureIgnoreCase);
                case Condition.NotEquals:
                    return !paramValue.Equals(filter.ParameterValue, StringComparison.InvariantCultureIgnoreCase);
            }

            return true;
        }
    }
}