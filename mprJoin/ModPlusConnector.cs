namespace mprJoin
{
    using System;
    using System.Collections.Generic;
    using ModPlusAPI.Abstractions;
    using ModPlusAPI.Enums;

    /// <inheritdoc />
    public class ModPlusConnector : IModPlusPlugin
    {
        private static ModPlusConnector _instance;

        public static ModPlusConnector Instance => _instance ??= new ModPlusConnector();
        
        /// <inheritdoc />
        public SupportedProduct SupportedProduct => SupportedProduct.Revit;

        /// <inheritdoc />
        public string Name => nameof(mprJoin);

        /// <inheritdoc />
        public string LName => "Соединение элементов"; // todo lName

        /// <inheritdoc />
        public string Description => "Позволяет управлять примыканием стен и балок, а так же соединением элементов"; // todo Description
        
        /// <inheritdoc />
        public string Price => "0";

        /// <inheritdoc />
        public string FullDescription => ""; // todo full description if need - not need

#if R2017
        /// <inheritdoc/>
        public string AvailProductExternalVersion => "2017";
#elif R2018
        /// <inheritdoc/>
        public string AvailProductExternalVersion => "2018";
#elif R2019
        /// <inheritdoc/>
        public string AvailProductExternalVersion => "2019";
#elif R2020
        /// <inheritdoc/>
        public string AvailProductExternalVersion => "2020";
#elif R2021
        /// <inheritdoc/>
        public string AvailProductExternalVersion => "2021";
#elif R2022
        /// <inheritdoc/>
        public string AvailProductExternalVersion => "2022";
#endif

        /// <inheritdoc />
        public string FullClassName => $"{nameof(mprJoin)}.{nameof(Command)}";

        /// <inheritdoc />
        public string AppFullClassName => string.Empty;

        /// <inheritdoc />
        public Guid AddInId => Guid.Empty;

        /// <inheritdoc />
        public bool CanAddToRibbon => true;

        /// <inheritdoc />
        public string ToolTipHelpImage => string.Empty;

        /// <inheritdoc />
        public List<string> SubPluginsNames => new List<string>
        {
                string.Empty
        };

        /// <inheritdoc />
        public List<string> SubPluginsLNames => new List<string>
        {
                string.Empty
        };

        /// <inheritdoc />
        public List<string> SubDescriptions => new List<string>
        {
                string.Empty
        };

        /// <inheritdoc />
        public List<string> SubFullDescriptions => new List<string>
        {
                string.Empty
        };

        /// <inheritdoc />
        public List<string> SubHelpImages => new List<string>
        {
                string.Empty
        };

        /// <inheritdoc />
        public List<string> SubClassNames => new List<string>
        {
                string.Empty
        };
    }
}