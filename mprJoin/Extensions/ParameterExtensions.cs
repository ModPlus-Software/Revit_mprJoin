namespace mprJoin.Extensions
{
    using Autodesk.Revit.DB;

    /// <summary>
    /// Расширения для параметра элемента Revit
    /// </summary>
    public static class ParameterExtensions
    {
        /// <summary>
        /// Получает параметр из экземпляра или типа элемента
        /// </summary>
        /// <param name="elem">Element</param>
        /// <param name="parameterName">Имя параметра</param>
        public static Parameter GetParameterFromInstanceOrType(this Element elem, string parameterName)
        {
            if (!elem.IsValidObject)
                return null;

            var param = elem.LookupParameter(parameterName);
            if (param != null)
                return param;

            var typeId = elem.GetTypeId();
            if (typeId == null)
                return null;

            var type = elem.Document?.GetElement(typeId);

            param = type?.LookupParameter(parameterName);
            return param;
        }

        /// <summary>
        /// Возвращает значение параметра
        /// </summary>
        /// <param name="param">Параметр</param>
        /// <returns>Значение параметра</returns>
        public static string GetParameterValue(this Parameter param)
        {
            if (param == null || !param.HasValue)
                return string.Empty;

            if (param.StorageType == StorageType.String)
                return param.AsString() ?? string.Empty;
            return param.AsValueString() ?? string.Empty;
        }
    }
}