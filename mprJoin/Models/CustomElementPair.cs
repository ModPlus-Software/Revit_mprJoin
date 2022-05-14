namespace mprJoin.Models;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Autodesk.Revit.DB;
using Extensions;
using ModPlus_Revit.Utils;
using ModPlusAPI.Mvvm;

/// <summary>
/// Модель данных с информацией о используемых категориях для соединения и правилах
/// </summary>
public class CustomElementPair : ObservableObject
{
    public CustomElementPair()
    {
    }

    public CustomElementPair(List<BuiltInCategory> categories)
        : this()
    {
        WithWhatToJoin = new ElementApplyFilter();
        WithWhatToJoin.SourceCategoriesOverride = categories;
        WhatJoinFilters = new ElementApplyFilter();
        WhatJoinFilters.SourceCategoriesOverride = categories;
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
}