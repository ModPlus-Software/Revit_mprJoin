namespace mprJoin.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Input;
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

        public CustomElementPair()
        {
            WhatJoinFilters = new ObservableCollection<Filter>();
            WhereJoinFilters = new ObservableCollection<Filter>();
            WhatJoinFilters.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasFilters));
            WithWhatToJoin = new ObservableCollection<SelectedCategory>();
            WithWhatToJoin.CollectionChanged += WithWhatToJoinOnCollectionChanged;
        }

        public CustomElementPair(IEnumerable<SelectedCategory> categories)
            : this()
        {
            foreach (var selectedCategory in categories)
                WithWhatToJoin.Add(selectedCategory);
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
        public ObservableCollection<SelectedCategory> WithWhatToJoin { get; set; }

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
        public ObservableCollection<Filter> WhatJoinFilters { get; set; }

        /// <summary>
        /// Список фильтров для категорий которые будут присоединяться
        /// </summary>
        public ObservableCollection<Filter> WhereJoinFilters { get; set; }

        /// <summary>
        /// Логический оператор для элементов основной категории
        /// </summary>
        public LogicCondition WhatJoinLogicCondition { get; set; }

        /// <summary>
        /// Логический оператор для элементов, которые будут присоединяться
        /// </summary>
        public LogicCondition WhereJoinLogicCondition { get; set; }

        /// <summary>
        /// Строковое имя всех выбранных категорий, для View.
        /// </summary>
        public string DisplayName => string.Join(", ", WithWhatToJoin.Where(c => c.IsSelected).Select(c => c.Name));

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

        public bool HasFilters => WhatJoinFilters.Any() || WhereJoinFilters.Any();

        /// <summary>
        /// Добавить фильтр к паре.
        /// </summary>
        public ICommand AddWhatJoinFilter => 
            new RelayCommandWithoutParameter(() => WhatJoinFilters.Add(new Filter()));

        /// <summary>
        /// Добавить фильтр к паре.
        /// </summary>
        public ICommand AddWhereJointFilter => 
            new RelayCommandWithoutParameter(() => WhereJoinFilters.Add(new Filter()));

        /// <summary>
        /// Удалить фильтр в левом списке
        /// </summary>
        public ICommand RemoveWhatJoinFilter =>
            new RelayCommand<Filter>(filter => WhatJoinFilters.Remove(filter));
        
        /// <summary>
        /// Удалить фильтр в правом списке
        /// </summary>
        public ICommand RemoveWhereJoinFilter =>
            new RelayCommand<Filter>(filter => WhereJoinFilters.Remove(filter));

        /// <summary>
        /// Отображение фильтров для пары.
        /// </summary>
        public ICommand ChangeVisibilityForFilters => new RelayCommandWithoutParameter(() => ShowFilters = !ShowFilters);

        public void ApplyFilters()
        {
            WhatToJoinElements = WhatJoinLogicCondition switch
            {
                LogicCondition.And => WhatToJoinElements
                    .Where(el => WhatJoinFilters
                        .Where(filter => !string.IsNullOrEmpty(filter.ParameterName))
                        .All(filter => IsMatchByFilter(el, filter)))
                    .ToList(),
                LogicCondition.Or => WhatToJoinElements
                    .Where(el => WhatJoinFilters
                        .Where(filter => !string.IsNullOrEmpty(filter.ParameterName))
                        .Any(filter => IsMatchByFilter(el, filter)))
                    .ToList(),
                _ => throw new ArgumentOutOfRangeException()
            };

            WhereToJoinElements = WhereJoinLogicCondition switch
            {
                LogicCondition.And => WhereToJoinElements
                    .Where(el => WhereJoinFilters
                        .Where(filter => !string.IsNullOrEmpty(filter.ParameterName))
                        .All(filter => IsMatchByFilter(el, filter)))
                    .ToList(),
                LogicCondition.Or => WhereToJoinElements
                    .Where(el => WhereJoinFilters
                        .Where(filter => !string.IsNullOrEmpty(filter.ParameterName))
                        .Any(filter => IsMatchByFilter(el, filter)))
                    .ToList(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <summary>
        /// Соответствие элемента фильтру.
        /// </summary>
        /// <param name="element">Элемент.</param>
        /// <param name="filter">Фильтр.</param>
        private bool IsMatchByFilter(Element element, Filter filter)
        {
            if (string.IsNullOrEmpty(filter.ParameterName))
                return true;
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
                    return paramValue.IndexOf(filter.ParameterValue, StringComparison.InvariantCultureIgnoreCase) >= 0;
                case Condition.NotContains:
                    return paramValue.IndexOf(filter.ParameterValue, StringComparison.InvariantCultureIgnoreCase) == -1;
                case Condition.Equals:
                    return paramValue.Equals(filter.ParameterValue, StringComparison.InvariantCultureIgnoreCase);
                case Condition.NotEquals:
                    return !paramValue.Equals(filter.ParameterValue, StringComparison.InvariantCultureIgnoreCase);
            }

            return true;
        }

        private void WithWhatToJoinOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var selectedCategory in e.NewItems.OfType<SelectedCategory>())
                {
                    selectedCategory.PropertyChanged += SelectedCategoryOnPropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var selectedCategory in e.OldItems.OfType<SelectedCategory>())
                {
                    selectedCategory.PropertyChanged -= SelectedCategoryOnPropertyChanged;
                }
            }
        }

        private void SelectedCategoryOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedCategory.IsSelected))
                OnPropertyChanged(nameof(DisplayName));
        }
    }
}