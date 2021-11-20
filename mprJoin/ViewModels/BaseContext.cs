namespace mprJoin.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Enums;
    using Models;
    using ModPlusAPI.Mvvm;

    /// <summary>
    /// Базовый класс контекста для страниц
    /// </summary>
    public abstract class BaseContext : ObservableObject
    {
        private static UIApplication _uiApplication;
        
        protected BaseContext(UIApplication uiApplication)
        {
            _uiApplication = uiApplication;
        }
        
        /// <summary>
        /// Опции для работы сервиса
        /// </summary>
        public ContiguityOption Option { get; set; }
        
        /// <summary>
        /// Получить список моделей категорий.
        /// <param name="allowedCategories">Список доступных категорий.</param>
        /// </summary>
        /// <param name="allowedCategories">Allowable categories</param>
        protected static IEnumerable<SelectedCategory> GetSelectedCategories(List<BuiltInCategory> allowedCategories)
        {
            foreach (Category category in _uiApplication.ActiveUIDocument.Document.Settings.Categories)
            {
                var builtInCategory = (BuiltInCategory)category.Id.IntegerValue;
                if (allowedCategories.Any(i => i == builtInCategory))
                {
                    yield return new SelectedCategory
                    {
                        Name = category.Name,
                        IsSelected = true
                    };
                }
            }
        }

        public abstract void SaveSettings();
    }
}