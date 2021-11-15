namespace mprJoin.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Enums;
    using Extensions;
    using Models;

    /// <summary>
    /// Сервис выбора элементов
    /// </summary>
    public class CollectorService
    {
        /// <summary>
        /// Получить FilterElementCollector в зависимости от опций.
        /// </summary>
        /// <param name="doc">Документ из которого будут получены элементы.</param>
        /// <param name="scopeType">Опции выбора элементов</param>
        public FilteredElementCollector GetFilteredElementCollector(UIDocument doc, ScopeType scopeType)
        {
            switch (scopeType)
            {
                case ScopeType.InProject:
                    return new FilteredElementCollector(doc.Document);

                case ScopeType.InActiveView:
                    return new FilteredElementCollector(doc.Document, doc.Document.ActiveView.Id);
                
                case ScopeType.SelectedElement:
                    var selectedElementIds = doc.Selection.GetElementIds();
                    return selectedElementIds.Any()
                        ? new FilteredElementCollector(doc.Document, selectedElementIds)
                        : new FilteredElementCollector(doc.Document, new List<ElementId> { ElementId.InvalidElementId });
            }

            return new FilteredElementCollector(doc.Document);
        }

        /// <summary>
        /// Найти пересекаемые элементы по BoundingBox Filter.
        /// </summary>
        /// <param name="document">Документ.</param>
        /// <param name="checkElement">Проверяемый элемент</param>
        /// <param name="elementsWhereFindingIntersection">Элементы среди которых проводится поиск</param>
        public IEnumerable<Element> GetIntersectedElementByBoundingBoxFilter(Document document, Element checkElement,
            IEnumerable<Element> elementsWhereFindingIntersection)
        {
            var box = checkElement.get_BoundingBox(null);
            var boundingBoxFilter = new BoundingBoxIntersectsFilter(new Outline(box.Min, box.Max));
            return new FilteredElementCollector(document, elementsWhereFindingIntersection.Select(i => i.Id).ToList())
                .WhereElementIsNotElementType().WherePasses(boundingBoxFilter).ToList();
        }

        /// <summary>
        /// Фильтрация элементов в паре.
        /// </summary>
        /// <param name="pair">Пара</param>
        public void FiltratePair(CustomElementPair pair)
        {
            switch (pair.LogicConditions)
            {
                case LogicConditions.And:
                    pair.WhatToJoinElements = pair.WhatToJoinElements.Where(el =>
                        pair.Filters.Where(filter => !string.IsNullOrEmpty(filter.ParameterName))
                            .All(filter => IsMatchBuFilter(el, filter))).ToList();
                    
                    pair.WhereToJoinElements = pair.WhereToJoinElements.Where(el =>
                        pair.Filters.Where(filter => !string.IsNullOrEmpty(filter.ParameterName))
                            .All(filter => IsMatchBuFilter(el, filter))).ToList();
                    break;
                case LogicConditions.Or:
                    pair.WhatToJoinElements = pair.WhatToJoinElements.Where(el =>
                        pair.Filters.Where(filter => !string.IsNullOrEmpty(filter.ParameterName))
                            .Any(filter => IsMatchBuFilter(el, filter))).ToList();
                    
                    pair.WhereToJoinElements = pair.WhereToJoinElements.Where(el =>
                        pair.Filters.Where(filter => !string.IsNullOrEmpty(filter.ParameterName))
                            .Any(filter => IsMatchBuFilter(el, filter))).ToList();

                    break;
            }
        }

        /// <summary>
        /// Соответствие элемента фильтру.
        /// </summary>
        /// <param name="element">Элемент.</param>
        /// <param name="filter">Фильтр.</param>
        /// <returns></returns>
        private bool IsMatchBuFilter(Element element, FilterModel filter)
        {
            var param = element.GetParameterFromInstanceOrType(filter.ParameterName);
            if (param == null)
                return false;

            var paramValue = param.GetParameterValue();
            switch (filter.Conditions)
            {
                case Conditions.Begin:
                    return paramValue.StartsWith(filter.ParameterValue, StringComparison.InvariantCulture);
                case Conditions.NotBegin:
                    return !paramValue.StartsWith(filter.ParameterValue, StringComparison.InvariantCulture);
                case Conditions.Contains:
                    return paramValue.Contains(filter.ParameterValue);
                case Conditions.NotContains:
                    return !paramValue.Contains(filter.ParameterValue);
                case Conditions.Equals:
                    return paramValue.Equals(filter.ParameterValue, StringComparison.InvariantCulture);
                case Conditions.NotEquals:
                    return !paramValue.Equals(filter.ParameterValue, StringComparison.InvariantCulture);
            }

            return true;
        }
    }
}