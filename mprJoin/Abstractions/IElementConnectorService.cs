namespace mprJoin.Abstractions
{
    using System.Collections.Generic;
    using Autodesk.Revit.DB;
    using CSharpFunctionalExtensions;
    using Enums;

    /// <summary>
    /// Сервис выбора элементов
    /// </summary>
    public interface IElementConnectorService
    {
        /// <summary>
        /// Присоединить или отсоединить элементы в зависимости от опций.
        /// </summary>
        /// <param name="elements">Список элементов.</param>
        /// <param name="option">Опции.</param>
        /// <param name="beginAndAndOptions">Опции начала и конца</param>
        public Result DoContiguityAction(List<Element> elements, ContiguityOption option, (bool, bool) beginAndAndOptions);
    }
}