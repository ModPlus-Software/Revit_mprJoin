namespace mprJoin.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Enums;

    /// <inheritdoc/>
    public class CollectorService : ICollectorService
    {
        /// <inheritdoc/>
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
    }
}