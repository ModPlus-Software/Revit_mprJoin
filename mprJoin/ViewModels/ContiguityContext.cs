namespace mprJoin.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Enums;
    using Models;
    using ModPlusAPI.Mvvm;
    using ModPlusAPI.Services;
    using ModPlusAPI.Windows;
    using Services;
    using Settings;

    public class ContiguityContext
    {
        private readonly UIApplication _uiApplication;
        private readonly CollectorService _collectorService;
        private List<SelectedCategory> _selectedCategories;
        private readonly ElementConnectorService _elementConnectorService;

        public ContiguityContext(UIApplication uiApplication)
        {
            _collectorService = new CollectorService();
            _uiApplication = uiApplication;
            _elementConnectorService = new ElementConnectorService(uiApplication.ActiveUIDocument);
        }

        /// <summary>
        /// Список моделей категорий для вывода пользователю
        /// </summary>
        public List<SelectedCategory> SelectedCategories => _selectedCategories ??= GetSelectedCategories();

        /// <summary>
        /// Обрабатывать ли начало элемента
        /// </summary>
        public bool FirstElementPoint { get; set; }

        /// <summary>
        /// Обрабатывать ли конец элемента
        /// </summary>
        public bool SecondElementPoint { get; set; }

        /// <summary>
        /// Опции для работы сервиса
        /// </summary>
        public ContiguityOption Option { get; set; }

        /// <summary>
        /// Команда выполнения
        /// </summary>
        public ICommand Execute => new RelayCommand<ScopeType>(scope =>
        {
            try
            {
                var selectedCategories = SelectedCategories.Where(i => i.IsSelected).Select(i => i.Name).ToList();
                var elements = _collectorService.GetFilteredElementCollector(_uiApplication.ActiveUIDocument, scope)
                    .WhereElementIsNotElementType()

                    // Оставляет возможные категории, без этого при следующей проверке получаю ошибку, полагаю,
                    // что у какого то элемента не получается посмотреть категорию
                    .WherePasses(new ElementMulticategoryFilter(PluginSetting.AllowedCategoriesToContiguity))

                    // Оставляет только выбранные
                    .Where(element => selectedCategories.Contains(element.Category.Name))
                    .ToList();
                if (!elements.Any())
                {
                    MessageBox.Show(ModPlusAPI.Language.GetItem("e1"), MessageBoxIcon.Alert);
                }
                else
                {
                    _elementConnectorService.DoContiguityAction(elements, Option,
                        (FirstElementPoint, SecondElementPoint));
                }
            }
            catch (Exception e)
            {
                e.ShowInExceptionBox();
            }
        });

        /// <summary>
        /// Получить список моделей категорий
        /// </summary>
        /// <returns></returns>
        private List<SelectedCategory> GetSelectedCategories()
        {
            var resultList = new List<SelectedCategory>();
            foreach (Category category in _uiApplication.ActiveUIDocument.Document.Settings.Categories)
            {
                var builtInCategory = (BuiltInCategory)category.Id.IntegerValue;
                if (PluginSetting.AllowedCategoriesToContiguity.Any(i => i == builtInCategory))
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