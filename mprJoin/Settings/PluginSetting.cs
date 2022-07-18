namespace mprJoin.Settings;

using System.Collections.Generic;
using Autodesk.Revit.DB;

/// <summary>
/// Класс с настройками для плагина
/// </summary>
public static class PluginSetting
{
    /// <summary>
    /// Разрешенные категории для вкладки Примыкание
    /// </summary>
    public static readonly List<BuiltInCategory> AllowedCategoriesToContiguity = new ()
    {
        BuiltInCategory.OST_Walls,
        BuiltInCategory.OST_StructuralFraming
    };

    /// <summary>
    /// Разрешенные категории для соединения элементов
    /// </summary>
    public static readonly List<BuiltInCategory> AllowedCategoriesToJoin = new ()
    {
        BuiltInCategory.OST_Walls,
        BuiltInCategory.OST_Ceilings,
        BuiltInCategory.OST_Columns,
        BuiltInCategory.OST_GenericModel,
        BuiltInCategory.OST_Floors,
        BuiltInCategory.OST_StructuralColumns,
        BuiltInCategory.OST_StructuralFraming,
        BuiltInCategory.OST_StructuralFoundation,
        BuiltInCategory.OST_Roofs,
        BuiltInCategory.OST_StructConnections
    };
        
    /// <summary>
    /// Разрешенные категории для Вырезания элементов.
    /// </summary>
    public static readonly List<BuiltInCategory> AllowedCategoriesToCut = new ()
    {
        BuiltInCategory.OST_Columns,
        BuiltInCategory.OST_GenericModel,
        BuiltInCategory.OST_StructuralColumns,
        BuiltInCategory.OST_StructuralFraming,
        BuiltInCategory.OST_ElectricalEquipment,
        BuiltInCategory.OST_MechanicalEquipment,
        BuiltInCategory.OST_SpecialityEquipment,
        BuiltInCategory.OST_PlumbingFixtures,
        BuiltInCategory.OST_DataDevices,
        BuiltInCategory.OST_Mass,
        BuiltInCategory.OST_StructConnections
    };

    /// <summary>
    /// Имя файла для сохранения настроек
    /// </summary>
    public static string SaveFileName = nameof(mprJoin);
}