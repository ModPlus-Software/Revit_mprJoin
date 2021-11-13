namespace mprJoin.Settings
{
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
        public static List<BuiltInCategory> AllowedCategoriesToContiguity = new ()
        {
            BuiltInCategory.OST_Walls,
            BuiltInCategory.OST_StructuralFraming
        };
    }
}