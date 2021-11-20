namespace mprJoin.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
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

    public class ContiguityContext : BaseContext
    {
        private readonly UIApplication _uiApplication;
        private readonly CollectorService _collectorService;
        private ObservableCollection<SelectedCategory> _selectedCategories;
        private readonly ElementConnectorService _elementConnectorService;
        private readonly UserSettingsService _userSettings;

        public ContiguityContext(UIApplication uiApplication)
            : base(uiApplication)
        {
            _collectorService = new CollectorService();
            _uiApplication = uiApplication;
            _elementConnectorService = new ElementConnectorService(uiApplication.ActiveUIDocument);
            _userSettings = new UserSettingsService(PluginSetting.SaveFileName);
            var list = _userSettings.Get<ObservableCollection<SelectedCategory>>(nameof(SelectedCategories));
            _selectedCategories = list.Any() ? list : null;
        }

        /// <summary>
        /// Список моделей категорий для вывода пользователю
        /// </summary>
        public ObservableCollection<SelectedCategory> SelectedCategories =>
            _selectedCategories ??= new ObservableCollection<SelectedCategory>(GetSelectedCategories(PluginSetting.AllowedCategoriesToContiguity));

        /// <summary>
        /// Обрабатывать ли начало элемента
        /// </summary>
        public bool FirstElementPoint { get; set; }

        /// <summary>
        /// Обрабатывать ли конец элемента
        /// </summary>
        public bool SecondElementPoint { get; set; }

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
        /// Сохранение настроек
        /// </summary>
        public override void SaveSettings()
        {
            _userSettings.Set(SelectedCategories, nameof(SelectedCategories));
        }
    }
}