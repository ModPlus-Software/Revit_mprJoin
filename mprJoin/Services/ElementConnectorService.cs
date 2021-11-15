namespace mprJoin.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.DB.Structure;
    using Autodesk.Revit.UI;
    using Enums;
    using Extensions;
    using Models;
    using ModPlusAPI.Enums;
    using ModPlusAPI.Services;

    public class ElementConnectorService
    {
        private readonly Document _doc;
        private readonly CollectorService _collectorService;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="uiDocument">Документ.</param>
        public ElementConnectorService(UIDocument uiDocument)
        {
            _doc = uiDocument.Document;
            _collectorService = new CollectorService();
        }

        /// <summary>
        /// Выполнить примыкание.
        /// </summary>
        /// <param name="elements">Набор элементов.</param>
        /// <param name="option">Опции примыканий.</param>
        /// <param name="beginAndAndOptions">Сет из настроек начала и конца элемента.</param>
        public void DoContiguityAction(List<Element> elements, ContiguityOption option, (bool, bool) beginAndAndOptions)
        {
            var trName = ModPlusAPI.Language.GetItem("t1");
            var resultService = new ResultService();
            using (var tr = new Transaction(_doc, trName))
            {
                tr.Start();
                foreach (var element in elements)
                {
                    try
                    {
                        if (element.Location is not LocationCurve)
                            continue;
                        
                        var (start, end) = beginAndAndOptions;
                        if (start)
                            ChangeEndJoinState(element, 0, option);
                        if (end)
                            ChangeEndJoinState(element, 1, option);
                    }
                    catch (Exception exception)
                    {
                        resultService.Add(exception.Message, element.Id.ToString(), ResultItemType.Error);
                    }
                }

                tr.Commit();
            }

            resultService.ShowByType();
        }

        /// <summary>
        /// Соединить элементы.
        /// </summary>
        /// <param name="document">Документы</param>
        /// <param name="pairs">Пары элементов</param>
        public void JoinElements(Document document, List<CustomElementPair> pairs)
        {
            var resultService = new ResultService();
            var trName = "транзакция"; // todo ModPlusAPI.Language.GetItem("t2");
            using (var tr = new Transaction(document, trName))
            {
                tr.Start();

                foreach (var pair in pairs)
                {
                    foreach (var elementWhoWillJoin in pair.WhatToJoinElements)
                    {
                        foreach (var intersectedElement in _collectorService.GetIntersectedElementByBoundingBoxFilter(
                            document, elementWhoWillJoin, pair.WhereToJoinElements).Where(i => !i.Id.Equals(elementWhoWillJoin.Id)))
                        {
                            // Проверка на примыкание стен. Если стена примыкает к другой стене, то данная проверка
                            // (!JoinGeometryUtils.AreElementsJoined(document, elementWhoWillJoin, intersectedElement))
                            // не ловит этот момент и 2 стены пытаются соединиться, поэтому получается ошибка
                            if (intersectedElement is Wall wallWhereWillJoin &&
                                elementWhoWillJoin is Wall wallWhoWillJoin)
                            {
                                if (((LocationCurve)wallWhoWillJoin.Location).get_ElementsAtJoin(0).ToList()
                                    .Any(i => i.Id.Equals(wallWhereWillJoin.Id)))
                                    continue;
                                if (((LocationCurve)wallWhoWillJoin.Location).get_ElementsAtJoin(1).ToList()
                                    .Any(i => i.Id.Equals(wallWhereWillJoin.Id)))
                                    continue;
                            }

                            try
                            {
                                if (!JoinGeometryUtils.AreElementsJoined(document, elementWhoWillJoin,
                                    intersectedElement))
                                {
                                    JoinGeometryUtils.JoinGeometry(document, elementWhoWillJoin, intersectedElement);

                                    // Проверка на правильный приоритет вырезания элементов
                                    if (JoinGeometryUtils.IsCuttingElementInJoin(document, intersectedElement,
                                        elementWhoWillJoin))
                                    {
                                        JoinGeometryUtils.SwitchJoinOrder(document, elementWhoWillJoin,
                                            intersectedElement);
                                    }
                                }
                                else
                                {
                                    // Проверка на правильный приоритет вырезания элементов
                                    if (JoinGeometryUtils.IsCuttingElementInJoin(document, intersectedElement,
                                        elementWhoWillJoin))
                                    {
                                        JoinGeometryUtils.SwitchJoinOrder(document, elementWhoWillJoin,
                                            intersectedElement);
                                    }
                                }
                            }
                            catch (Exception exception)
                            {
                                resultService.Add(
                                    exception.Message,
                                    elementWhoWillJoin.Id.ToString(), ResultItemType.Error);
                            }
                        }
                    }
                }
                
                tr.Commit();
            }
            
            resultService.ShowByType();
        }
        
        private void ChangeEndJoinState(Element element, int end, ContiguityOption joinType)
        {
            if (element is Wall wall)
            {
                switch (joinType)
                {
                    case ContiguityOption.Join:
                    case ContiguityOption.Invert when !WallUtils.IsWallJoinAllowedAtEnd(wall, end):
                        WallUtils.AllowWallJoinAtEnd(wall, end);
                        break;
                    case ContiguityOption.DisJoin:
                    case ContiguityOption.Invert when WallUtils.IsWallJoinAllowedAtEnd(wall, end):
                        WallUtils.DisallowWallJoinAtEnd(wall, end);
                        break;
                }
            }
            else if (element is FamilyInstance familyInstance)
            {
                switch (joinType)
                {
                    case ContiguityOption.Join:
                    case ContiguityOption.Invert when !StructuralFramingUtils.IsJoinAllowedAtEnd(familyInstance, end):
                        StructuralFramingUtils.AllowJoinAtEnd(familyInstance, end);
                        break;
                    case ContiguityOption.DisJoin:
                    case ContiguityOption.Invert when StructuralFramingUtils.IsJoinAllowedAtEnd(familyInstance, end):
                        StructuralFramingUtils.DisallowJoinAtEnd(familyInstance, end);
                        break;
                }
            }
        }
    }
}