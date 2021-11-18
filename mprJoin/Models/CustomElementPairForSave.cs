namespace mprJoin.Models
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Enums;

    public class CustomElementPairForSave
    {
        /// <summary>
        /// Категория элементов которые будут будут иметь высший приоритет при соединении
        /// </summary>
        public string WhatToJoinCategory { get; set; }

        /// <summary>
        /// Категория которые будут присоединяться (т.е. у них будут образовываться вырезы)
        /// </summary>
        public List<SelectedCategory> WithWhatToJoin { get; set; }

        /// <summary>
        /// Список фильтров
        /// </summary>
        public ObservableCollection<FilterModel> FiltersForMainCategory { get; set; }
        
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
        public bool ShowFilters { get; set; }

        public CustomElementPair ToModel()
        {
            var model = new CustomElementPair(WithWhatToJoin)
            {
                WhatToJoinCategory = WhatToJoinCategory,
                LogicConditions = LogicConditions,
                FiltersForMainCategory = FiltersForMainCategory,
                FilterModelsForSubCategories = FilterModelsForSubCategories,
                ShowFilters = ShowFilters
            };

            return model;
        }
    }
}