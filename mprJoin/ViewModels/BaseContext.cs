using System.Collections.ObjectModel;
using mprJoin.Enums;

namespace mprJoin.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Models;
    using ModPlusAPI.Mvvm;

    /// <summary>
    /// Базовый класс контекста для страниц
    /// </summary>
    public abstract class BaseContext : ObservableObject
    {
        private static UIApplication _uiApplication;
        
        public BaseContext(UIApplication uiApplication)
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
        protected static ObservableCollection<SelectedCategory> GetSelectedCategories(List<BuiltInCategory> allowedCategories)
        {
            var resultList = new ObservableCollection<SelectedCategory>();
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

        public abstract void SaveSettings();
    }
}