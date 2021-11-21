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
        public void DoContiguityAction(List<Element> elements, ContiguityOption option, Tuple<bool, bool> beginAndAndOptions)
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

                        var start = beginAndAndOptions.Item1;
                        var end = beginAndAndOptions.Item2;
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
        /// <param name="doc">Документы.</param>
        /// <param name="pairs">Пары элементов.</param>
        /// <param name="option">Опции соединения.</param>
        public void JoinElements(Document doc, List<CustomElementPair> pairs, ContiguityOption option)
        {
            var resultService = new ResultService();
            var trName = ModPlusAPI.Language.GetItem("t2");
            using (var tr = new Transaction(doc, trName))
            {
                tr.Start();

                foreach (var pair in pairs)
                {
                    foreach (var elementWhoWillJoin in pair.WhatToJoinElements)
                    {
                        foreach (var intersectedElement in _collectorService
                            .GetIntersectedElementByBoundingBoxFilter(doc, elementWhoWillJoin, pair.WhereToJoinElements)
                            .Where(i => !i.Id.Equals(elementWhoWillJoin.Id)))
                        {
                            // Проверка на примыкание стен. Если стена примыкает к другой стене, то данная проверка
                            // (!JoinGeometryUtils.AreElementsJoined(document, elementWhoWillJoin, intersectedElement))
                            // не ловит этот момент и 2 стены пытаются соединиться, поэтому получается ошибка
                            if (intersectedElement is Wall wallWhereWillJoin &&
                                elementWhoWillJoin is Wall { Location: LocationCurve whoWillJoinCurve })
                            {
                                if (whoWillJoinCurve
                                    .get_ElementsAtJoin(0)
                                    .ToList()
                                    .Any(i => i.Id.Equals(wallWhereWillJoin.Id)))
                                    continue;
                                if (whoWillJoinCurve
                                    .get_ElementsAtJoin(1)
                                    .ToList()
                                    .Any(i => i.Id.Equals(wallWhereWillJoin.Id)))
                                    continue;
                            }

                            try
                            {
                                switch (option)
                                {
                                    case ContiguityOption.Join:
                                        if (!JoinGeometryUtils.AreElementsJoined(doc, elementWhoWillJoin,
                                            intersectedElement))
                                        {
                                            JoinGeometryUtils.JoinGeometry(doc, elementWhoWillJoin, intersectedElement);

                                            // Проверка на правильный приоритет вырезания элементов
                                            if (JoinGeometryUtils.IsCuttingElementInJoin(
                                                doc, intersectedElement, elementWhoWillJoin))
                                            {
                                                JoinGeometryUtils.SwitchJoinOrder(
                                                    doc, elementWhoWillJoin, intersectedElement);
                                            }
                                        }
                                        else
                                        {
                                            // Проверка на правильный приоритет вырезания элементов
                                            if (JoinGeometryUtils.IsCuttingElementInJoin(
                                                doc, intersectedElement, elementWhoWillJoin))
                                            {
                                                JoinGeometryUtils.SwitchJoinOrder(
                                                    doc, elementWhoWillJoin, intersectedElement);
                                            }
                                        }

                                        break;
                                    case ContiguityOption.DisJoin:
                                        if (JoinGeometryUtils.AreElementsJoined(doc, elementWhoWillJoin, intersectedElement))
                                            JoinGeometryUtils.UnjoinGeometry(doc, elementWhoWillJoin, intersectedElement);
                                        break;
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