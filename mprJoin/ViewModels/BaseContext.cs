namespace mprJoin.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Models;

    /// <summary>
    /// Базовый класс контекста для страниц
    /// </summary>
    public class BaseContext
    {
        private static UIApplication _uiApplication;
        
        public BaseContext(UIApplication uiApplication)
        {
            _uiApplication = uiApplication;
        }
        
        /// <summary>
        /// Получить список моделей категорий
        /// </summary>
        /// <returns></returns>
        protected static List<SelectedCategory> GetSelectedCategories(List<BuiltInCategory> allowedCategories)
        {
            var resultList = new List<SelectedCategory>();
            foreach (Category category in _uiApplication.ActiveUIDocument.Document.Settings.Categories)
            {
                var builtInCategory = (BuiltInCategory)category.Id.IntegerValue;
                if (allowedCategories.Any(i => i == builtInCategory))
                {
                    resultList.Add(new SelectedCategory
                    {
                        Name = category.Name,
                        IsSelected = true
                    });
                }
            }

            return resultList;
        }
    }
}