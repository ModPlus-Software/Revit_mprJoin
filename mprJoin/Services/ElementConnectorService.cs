namespace mprJoin.Services
{
    using System;
    using System.Collections.Generic;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.DB.Structure;
    using Autodesk.Revit.UI;
    using Enums;
    using Helpers;
    using Result = CSharpFunctionalExtensions.Result;

    public class ElementConnectorService
    {
        private readonly Document _doc;
        
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="uiDocument">Документ</param>
        public ElementConnectorService(UIDocument uiDocument)
        {
            _doc = uiDocument.Document;
        }

        /// <inheritdoc/>
        public Result DoContiguityAction(List<Element> elements, ContiguityOption option, (bool, bool) beginAndAndOptions)
        {
            var trName = ModPlusAPI.Language.GetItem(Constants.LangItem, "t1");
            using (var tr = new Transaction(_doc, trName))
            {
                tr.Start();
                try
                {
                    foreach (var element in elements)
                    {
                        if (element.Location is not LocationCurve)
                            continue;
                        
                        // Чет не знаю, как можно этот одинаковый код схлопнуть, выносить в стратегии не хочется, ибо случая всего 2
                        // а идеи что то нет, как избавиться от дублирования
                        var (start, end) = beginAndAndOptions;
                        switch (element)
                        {
                            case Wall wall:
                                switch (option)
                                {
                                    case ContiguityOption.Join:
                                        if (start)
                                            WallUtils.AllowWallJoinAtEnd(wall, 0);
                                        if (end)
                                            WallUtils.AllowWallJoinAtEnd(wall, 1);
                                        break;
                                    case ContiguityOption.DisJoin:
                                        if (start)
                                            WallUtils.DisallowWallJoinAtEnd(wall, 0);
                                        if (end)
                                            WallUtils.DisallowWallJoinAtEnd(wall, 1);
                                        break;
                                    case ContiguityOption.Invert:
                                        if (start)
                                            InvertJoin(wall, 0);
                                        if (end)
                                            InvertJoin(wall, 1);
                                        break;
                                }
                                
                                break;
                            
                            case FamilyInstance familyInstance:
                                switch (option)
                                {
                                    case ContiguityOption.Join:
                                        if (start)
                                            StructuralFramingUtils.AllowJoinAtEnd(familyInstance, 0);
                                        if (end)
                                            StructuralFramingUtils.AllowJoinAtEnd(familyInstance, 1);
                                        break;
                                    case ContiguityOption.DisJoin:
                                        if (start)
                                            StructuralFramingUtils.DisallowJoinAtEnd(familyInstance, 0);
                                        if (end)
                                            StructuralFramingUtils.DisallowJoinAtEnd(familyInstance, 1);
                                        break;
                                    case ContiguityOption.Invert:
                                        if (start)
                                            InvertJoin(familyInstance, 0);
                                        if (end)
                                            InvertJoin(familyInstance, 1);
                                        break;
                                }

                                break;
                        }
                    }

                    tr.Commit();
                }
                catch (Exception e)
                {
                    return Result.Failure(e.ToString());
                }
            }

            return Result.Success();
        }

        private void InvertJoin(Element element, int end)
        {
            if (element is Wall wall)
            {
                if (WallUtils.IsWallJoinAllowedAtEnd(wall, end))
                    WallUtils.DisallowWallJoinAtEnd(wall, end);
                else
                    WallUtils.AllowWallJoinAtEnd(wall, end);
            }
            else if (element is FamilyInstance familyInstance 
                     && (BuiltInCategory)familyInstance.Category.Id.IntegerValue == BuiltInCategory.OST_StructuralFraming)
            {
                if (StructuralFramingUtils.IsJoinAllowedAtEnd(familyInstance, end))
                    StructuralFramingUtils.DisallowJoinAtEnd(familyInstance, end);
                else
                    StructuralFramingUtils.AllowJoinAtEnd(familyInstance, end);
            }
        }
    }
}