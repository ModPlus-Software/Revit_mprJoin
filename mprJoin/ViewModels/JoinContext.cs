namespace mprJoin.ViewModels
{
    using System;
    using System.Collections.Generic;
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

    public class JoinContext : BaseContext
    {
        private readonly UIApplication _uiApplication;
        private List<string> _stringSelectedCategories;
        private readonly CollectorService _collectorService;
        private readonly ElementConnectorService _elementConnectorService;
        private readonly UserSettingsService _userSettings;
        private readonly ObservableCollection<CustomElementPair> _pairs;

        public JoinContext(UIApplication uiApplication)
            : base(uiApplication)
        {
            _uiApplication = uiApplication;
            _collectorService = new CollectorService();
            _elementConnectorService = new ElementConnectorService(uiApplication.ActiveUIDocument);
            _userSettings = new UserSettingsService(PluginSetting.SaveFileName);
            _pairs = new ObservableCollection<CustomElementPair>();
            _userSettings.Get<List<CustomElementPairForSave>>(nameof(Pairs)).ForEach(el => _pairs.Add(el.ToModel()));
        }

        /// <summary>
        /// Список с парами элементов для соединения
        /// </summary>
        public ObservableCollection<CustomElementPair> Pairs => _pairs;

        /// <summary>
        /// Список моделей категорий для вывода пользователю в текстовом представлении
        /// </summary>
        public List<string> StringSelectedCategories => _stringSelectedCategories ??=
            GetSelectedCategories(PluginSetting.AllowedCategoriesToJoin).Select(i => i.Name).ToList();

        /// <summary>
        /// Добавить пару.
        /// </summary>
        public ICommand AddPair => new RelayCommandWithoutParameter(() =>
        {
            try
            {
                _pairs.Add(new CustomElementPair(GetSelectedCategories(PluginSetting.AllowedCategoriesToJoin)));
            }
            catch (Exception exception)
            {
                exception.ShowInExceptionBox();
            }
        });
        
        /// <summary>
        /// Удалить пару.
        /// </summary>
        public ICommand RemovePair => new RelayCommand<CustomElementPair>(pair => _pairs.Remove(pair));

        /// <summary>
        /// Добавить фильтр к паре.
        /// </summary>
        public ICommand AddFilter => new RelayCommand<CustomElementPair>(pair => pair.Filters.Add(new FilterModel()));
        
        /// <summary>
        /// Удалить фильтр у пары
        /// </summary>
        public ICommand RemoveFilter => new RelayCommand<FilterModel>(filter =>
        {
            foreach (var pair in Pairs)
            {
                if (pair.Filters.Contains(filter))
                {
                    pair.Filters.Remove(filter);
                    return;
                }
            }
        });

        /// <summary>
        /// Сохранение настроек
        /// </summary>
        public ICommand SaveSettings => new RelayCommandWithoutParameter(() =>
        {
            var saveList = Pairs.Select(i => i.ToSaveMode()).ToList();
            _userSettings.Set(saveList, nameof(Pairs));
        });

        /// <summary>
        /// Отображение фильтров для пары.
        /// </summary>
        public ICommand ChangeVisibilityForFilters =>
            new RelayCommand<CustomElementPair>(pair => pair.ShowFilters = !pair.ShowFilters);

        /// <summary>
        /// Команда выполнения
        /// </summary>
        public ICommand Execute => new RelayCommand<ScopeType>(scope =>
        {
            try
            {
                var pairs = Pairs.ToList();
                if (!pairs.Any())
                {
                    MessageBox.Show(ModPlusAPI.Language.GetItem("e2"), MessageBoxIcon.Alert);
                    return;
                }

                foreach (var pair in pairs)
                {
                    if (string.IsNullOrEmpty(pair.WhatToJoinCategory))
                    {
                        continue;
                    }

                    pair.WhatToJoinElements = _collectorService
                        .GetFilteredElementCollector(_uiApplication.ActiveUIDocument, scope)
                        .WhereElementIsNotElementType()
                        
                        // Оставляет возможные категории, без этого при следующей проверке получаю ошибку, полагаю,
                        // что у какого то элемента не получается посмотреть категорию
                        .WherePasses(new ElementMulticategoryFilter(PluginSetting.AllowedCategoriesToJoin))
                        .Where(el =>
                            el.Category.Name.Equals(
                                pair.WhatToJoinCategory,
                                StringComparison.InvariantCultureIgnoreCase))
                        .ToList();

                    pair.WhereToJoinElements = _collectorService
                        .GetFilteredElementCollector(_uiApplication.ActiveUIDocument, scope)
                        .WhereElementIsNotElementType()
                        
                        // Оставляет возможные категории, без этого при следующей проверке получаю ошибку, полагаю,
                        // что у какого то элемента не получается посмотреть категорию
                        .WherePasses(new ElementMulticategoryFilter(PluginSetting.AllowedCategoriesToJoin))
                        .Where(el => pair.WithWhatToJoin.SelectedCategories.Where(cat => cat.IsSelected).Select(cat => cat.Name).Any(cat =>
                            cat.Equals(el.Category.Name, StringComparison.InvariantCultureIgnoreCase)))
                        .ToList();
                }

                pairs.ForEach(_collectorService.FiltratePair);
                _elementConnectorService.JoinElements(_uiApplication.ActiveUIDocument.Document, pairs);
            }
            catch (Exception e)
            {
                e.ShowInExceptionBox();
            }
        });
    }
}