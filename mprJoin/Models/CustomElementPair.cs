namespace mprJoin.Models;

using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Autodesk.Revit.DB;
using ModPlus_Revit.Utils;
using ModPlusAPI.Mvvm;

/// <summary>
/// Модель данных с информацией о используемых категориях для соединения и правилах
/// </summary>
public class CustomElementPair : ObservableObject
{
    private bool _isVisibleOnlyParallelWallsSetting;
    private bool _onlyParallelWalls;

    public CustomElementPair()
    {
    }

    public CustomElementPair(List<BuiltInCategory> categories)
        : this()
    {
        WithWhatToJoin = new ElementApplyFilter
        {
            SourceCategoriesOverride = categories
        };
        WhatJoinFilters = new ElementApplyFilter
        {
            SourceCategoriesOverride = categories
        };
        WithWhatToJoin.PropertyChanged += CheckFiltersCategory;
        WhatJoinFilters.PropertyChanged += CheckFiltersCategory;
        IsVisibleOnlyParallelWalls = CheckCategory();
    }

    /// <summary>
    /// Фильтр для элементов которые будут присоединяться (т.е. у них будут образовываться вырезы)
    /// </summary>
    public ElementApplyFilter WithWhatToJoin { get; set; }

    /// <summary>
    /// Фильтр для элементов куда будут присоединяться (т.е. у них будут образовываться вырезы)
    /// </summary>
    public ElementApplyFilter WhatJoinFilters { get; set; }

    /// <summary>
    /// Только параллельные стены
    /// </summary>
    public bool OnlyParallelWalls
    {
        get => _onlyParallelWalls;
        set
        {
            _onlyParallelWalls = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Видимость опции "Только параллельные стены"
    /// </summary>
    public bool IsVisibleOnlyParallelWalls
    {
        get => _isVisibleOnlyParallelWallsSetting;
        set
        {
            _isVisibleOnlyParallelWallsSetting = value;
            OnPropertyChanged();
        }
    }

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

    public void ApplyFilters()
    {
        WhatToJoinElements = WhatToJoinElements.Where(i => WhatJoinFilters.IsMatch(i)).ToList();
        WhereToJoinElements = WhereToJoinElements.Where(i => WithWhatToJoin.IsMatch(i)).ToList();
    }

    public void SetSettingsAfterGetInSaveFile(List<BuiltInCategory> categories)
    {
        WithWhatToJoin.SourceCategoriesOverride = categories;
        WhatJoinFilters.SourceCategoriesOverride = categories;
        WithWhatToJoin.PropertyChanged += CheckFiltersCategory;
        WhatJoinFilters.PropertyChanged += CheckFiltersCategory;
        IsVisibleOnlyParallelWalls = CheckCategory();
    }

    private void CheckFiltersCategory(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        IsVisibleOnlyParallelWalls = CheckCategory();
    }

    /// <summary>
    /// Проверяет имеется ли левом и правом фильтре категория стен
    /// </summary>
    private bool CheckCategory()
    {
        var result = WithWhatToJoin.Categories.Any() &&
                     WhatJoinFilters.Categories.Any() &&
                     WithWhatToJoin.Categories.Select(i => i.BuiltInCategory).All(j => j == BuiltInCategory.OST_Walls) &&
                     WhatJoinFilters.Categories.Select(i => i.BuiltInCategory).All(j => j == BuiltInCategory.OST_Walls);

        if (result == false)
            OnlyParallelWalls = false;

        return result;
    }
}