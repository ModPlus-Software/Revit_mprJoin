namespace mprJoin.Abstractions
{
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Enums;

    /// <summary>
    /// Сервис получения элементов
    /// </summary>
    public interface ICollectorService
    {
        /// <summary>
        /// Получить настроенный элемент коллектор
        /// </summary>
        /// <param name="doc">Документ</param>
        /// <param name="scopeType">Опции выбора</param>
        public FilteredElementCollector GetFilteredElementCollector(UIDocument doc, ScopeType scopeType);
    }
}