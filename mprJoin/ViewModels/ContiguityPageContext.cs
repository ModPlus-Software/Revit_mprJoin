namespace mprJoin.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Abstractions;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using CSharpFunctionalExtensions;
    using Enums;
    using Models;
    using ModPlusAPI.Mvvm;
    using ModPlusAPI.Services;
    using ModPlusAPI.Windows;
    using Settings;
    using Constants = Helpers.Constants;

    public class ContiguityPageContext
    {
        private readonly UIApplication _uiApplication;
        private readonly PluginSetting _pluginSetting;
        private readonly RevitTask _revitTask;
        private readonly ICollectorService _collectorService;
        private readonly Lazy<List<SelectedCategory>> _selectedCategories;
        private readonly IElementConnectorService _elementConnectorService;

        public ContiguityPageContext(
            UIApplication uiApplication, 
            PluginSetting pluginSetting,
            RevitTask revitTask,
            ICollectorService collectorService,
            IElementConnectorService elementConnectorService)
        {
            _revitTask = revitTask;
            _collectorService = collectorService;
            _pluginSetting = pluginSetting;
            _uiApplication = uiApplication;
            _elementConnectorService = elementConnectorService;
            _selectedCategories = new Lazy<List<SelectedCategory>>(GetSelectedCategories);
        }

        /// <summary>
        /// Список моделей категорий для вывода пользователю
        /// </summary>
        public List<SelectedCategory> SelectedCategories => _selectedCategories.Value;
        
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
        public ICommand Execute => new RelayCommand<ScopeType>(Action);

        private async void Action(ScopeType scope)
        {
            var resultService = new ResultService();
            try
            {
                var selectedCategories = SelectedCategories.Where(i => i.IsSelected).Select(i => i.Name).ToList();
                var elements = _collectorService.GetFilteredElementCollector(_uiApplication.ActiveUIDocument, scope)
                    .WhereElementIsNotElementType()

                    // Оставляет возможные категории, без этого при следующей проверке получаю ошибку, полагаю, что у какого то элемента не получается посмотреть категорию
                    .WherePasses(new ElementMulticategoryFilter(_pluginSetting.AllowedCategoriesToContiguity))

                    // Оставляет только выбранные
                    .Where(element => selectedCategories.Contains(element.Category.Name))
                    .ToList();
                if (!elements.Any())
                {
                    resultService.ShowWithoutGrouping(ModPlusAPI.Language.GetItem(Constants.LangItem, "e1"));
                }
                else
                {
                    // Могу оставить и этот пусть, но ревит так кажется понятнее
                    /*RevitExternalEventHandler.Instance.Run(
                        () =>
                    {
                        _elementConnectorService.DoContiguityAction(elements, Option, (FirstElementPoint, SecondElementPoint))
                            .OnFailure(err => resultService.ShowWithoutGrouping(err));
                    }, false, _uiApplication.ActiveUIDocument.Document);*/
                    await _revitTask.Run(_ =>
                    {
                        _elementConnectorService
                            .DoContiguityAction(elements, Option, (FirstElementPoint, SecondElementPoint))
                            .OnFailure(err => resultService.ShowWithoutGrouping(err));
                    });
                }
            }
            catch (Exception e)
            {
                e.ShowInExceptionBox();
            }
        }

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
                if (_pluginSetting.AllowedCategoriesToContiguity.Any(i => i == builtInCategory))
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