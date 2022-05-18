namespace mprJoin.Services;

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
    public void JoinElements(Document doc, List<CustomElementPair> pairs, JoinOption option)
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
                                 .GetIntersectedElementByBoundingBoxFilter(doc, elementWhoWillJoin, pair.WhereToJoinElements))
                    {
                        // Проверка на примыкание стен. Если стена примыкает к другой стене, то данная проверка
                        // (!JoinGeometryUtils.AreElementsJoined(document, elementWhoWillJoin, intersectedElement))
                        // не ловит этот момент и 2 стены пытаются соединиться, поэтому получается ошибка
                        if (intersectedElement is Wall wallWhereWillJoin &&
                            elementWhoWillJoin is Wall { Location: LocationCurve whoWillJoinCurve })
                        {
                            if (whoWillJoinCurve
                                .get_ElementsAtJoin(0)
                                .OfType<Element>()
                                .Any(i => i.Id.Equals(wallWhereWillJoin.Id)))
                                continue;
                            if (whoWillJoinCurve
                                .get_ElementsAtJoin(1)
                                .OfType<Element>()
                                .Any(i => i.Id.Equals(wallWhereWillJoin.Id)))
                                continue;
                        }

                        try
                        {
                            switch (option)
                            {
                                case JoinOption.Join:
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
                                case JoinOption.DisJoin:
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

    /// <summary>
    /// Вырезать элементы.
    /// </summary>
    /// <param name="doc">Документы.</param>
    /// <param name="pairs">Пары элементов.</param>
    /// <param name="cutOptions">Опции вырезания.</param>
    public void CutElements(Document doc, List<CustomElementPair> pairs, CutOptions cutOptions)
    {
        var resultService = new ResultService();
        var trName = ModPlusAPI.Language.GetItem("t3");
        using (var tr = new Transaction(doc, trName))
        {
            tr.Start();

            foreach (var pair in pairs)
            {
                foreach (var elementWhoWillJoin in pair.WhatToJoinElements)
                {
                    foreach (var intersectedElement in _collectorService
                                 .GetIntersectedElementByBoundingBoxFilter(doc, elementWhoWillJoin, pair.WhereToJoinElements)
                                 .Where(i => CheckParallelWalls(pair.OnlyParallelWalls, elementWhoWillJoin, i)))
                    {
                        try
                        {
                            switch (cutOptions)
                            {
                                case CutOptions.Cut:
                                    if (!InstanceVoidCutUtils.IsVoidInstanceCuttingElement(elementWhoWillJoin))
                                        continue;
                                    if (InstanceVoidCutUtils.GetElementsBeingCut(elementWhoWillJoin).Contains(intersectedElement.Id))
                                        continue;
                                    InstanceVoidCutUtils.AddInstanceVoidCut(doc, intersectedElement, elementWhoWillJoin);
                                    break;
                                    
                                case CutOptions.CancelCut:
                                    foreach (var id in InstanceVoidCutUtils.GetElementsBeingCut(elementWhoWillJoin))
                                    {
                                        var el = _doc.GetElement(id);
                                        if (el is not FamilyInstance)
                                            InstanceVoidCutUtils.RemoveInstanceVoidCut(doc, el, elementWhoWillJoin);
                                    }
                                        
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

    private bool CheckParallelWalls(bool checkParallelWalls, Element whoWillJoin, Element withWhoWillJoin)
    {
        if (!checkParallelWalls)
            return true;

        if (whoWillJoin is not Wall whoWillJoinWall)
            return true;

        if (withWhoWillJoin is not Wall withWhoWillJoinWall)
            return true;

        var whoCurve = ((LocationCurve)whoWillJoinWall.Location).Curve;
        var withWhoCurve = ((LocationCurve)withWhoWillJoinWall.Location).Curve;

        if (whoCurve.)
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