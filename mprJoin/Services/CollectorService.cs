namespace mprJoin.Services;

using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Enums;
using ModPlus_Revit.Utils;

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
                var selectedElementIds = doc.Selection
                    .PickObjects(ObjectType.Element)
                    .Select(e => doc.Document.GetElement(e).Id)
                    .ToList();
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
    /// <param name="checkElement">Проверяемый элемент.</param>
    /// <param name="elementsWhereFindingIntersection">Элементы среди которых проводится поиск.</param>
    public IEnumerable<Element> GetIntersectedElementByBoundingBoxFilter(
        Document document, Element checkElement, List<Element> elementsWhereFindingIntersection)
    {
        var box = checkElement.get_BoundingBox(null);
        var line = Line.CreateBound(box.Min, box.Max);
        var extraLength = 100.MmToFt(); // Увеличение BoundingBox для поиска пересечений.
        var boundingBoxFilter = new BoundingBoxIntersectsFilter(new Outline(box.Min - (line.Direction * extraLength), box.Max + (line.Direction * extraLength)));
        if (!elementsWhereFindingIntersection.Any())
            return new List<Element>();
        return new FilteredElementCollector(document, elementsWhereFindingIntersection.Select(i => i.Id).ToList())
            .WhereElementIsNotElementType()
            .WherePasses(boundingBoxFilter)
            .Where(i => !i.Id.Equals(checkElement.Id))
            .ToList();
    }
}