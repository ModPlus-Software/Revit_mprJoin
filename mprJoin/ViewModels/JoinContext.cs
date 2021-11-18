namespace mprJoin.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
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
        /// <summary>
        /// Имя перманентной - постоянной конфигурации, которая сохраняет настройки всегда и ее нельзя удалить
        /// </summary>
        private readonly string _standardConfigName = "Standard"; // todo ModPlusAPI.Language.GetItem("n1");
        private readonly UIApplication _uiApplication;
        private List<string> _stringSelectedCategories;
        private readonly CollectorService _collectorService;
        private readonly ElementConnectorService _elementConnectorService;
        private readonly UserSettingsService _userSettings;
        private readonly ObservableCollection<CustomElementPair> _pairs;
        private readonly SavedJoinConfigurations _permanentConfigurations;
        private SavedJoinConfigurations _currentConfiguration;

        public JoinContext(UIApplication uiApplication)
            : base(uiApplication)
        {
            _uiApplication = uiApplication;
            _collectorService = new CollectorService();
            _elementConnectorService = new ElementConnectorService(uiApplication.ActiveUIDocument);
            _userSettings = new UserSettingsService(PluginSetting.SaveFileName);
            _pairs = new ObservableCollection<CustomElementPair>();
            var configurationsList = _userSettings.Get<ObservableCollection<SavedJoinConfigurations>>(nameof(SavedJoinConfigurations));
            _permanentConfigurations = configurationsList.FirstOrDefault(i => i.Name.Equals(_standardConfigName)) ??
                new SavedJoinConfigurations
                {
                    Name = _standardConfigName,
                    IsEditable = false
                };

            Configurations = configurationsList.Any()
                ? configurationsList : new ObservableCollection<SavedJoinConfigurations>
                {
                    _permanentConfigurations
                };
            if (!Configurations.Contains(_permanentConfigurations))
                Configurations.Add(_permanentConfigurations);
            _currentConfiguration = _permanentConfigurations;
            _currentConfiguration.Pairs.Select(i => i.ToModel()).ToList().ForEach(i => _pairs.Add(i));
        }

        /// <summary>
        /// Текущая выбранная конфигурация.
        /// </summary>
        public SavedJoinConfigurations CurrentConfiguration
        {
            get => _currentConfiguration;
            set
            {
                _currentConfiguration = value;
                Pairs.Clear();
                value.Pairs.ForEach(i => Pairs.Add(i.ToModel()));
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Конфигурации в проекте.
        /// </summary>
        public ObservableCollection<SavedJoinConfigurations> Configurations { get; set; }

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
        public ICommand RemovePair => new RelayCommand<CustomElementPair>(pair =>
        {
            try
            {
                _pairs.Remove(pair);
            }
            catch (Exception exception)
            {
                exception.ShowInExceptionBox();
            }
        });

        /// <summary>
        /// Удалить конфигурацию.
        /// </summary>
        public ICommand DeleteConfiguration =>
            new RelayCommand<SavedJoinConfigurations>(config =>
            {
                if (config.Name.Equals(_standardConfigName))
                    return;
                Configurations.Remove(config);
                CurrentConfiguration = _permanentConfigurations;
            });

        /// <summary>
        /// Добавить конфигурацию.
        /// </summary>
        public ICommand AddConfiguration => new RelayCommandWithoutParameter(() =>
        {
            var newConfiguration = _currentConfiguration.Clone();
            newConfiguration.Name = DateTime.Now.ToString(CultureInfo.CurrentCulture);
            Configurations.Add(newConfiguration);
            CurrentConfiguration = newConfiguration;
        });

        /// <summary>
        /// Добавить фильтр к паре.
        /// </summary>
        public ICommand AddFilterMainFilter => new RelayCommand<CustomElementPair>(pair =>
        {
            try
            {
                pair.FiltersForMainCategory.Add(new FilterModel());
            }
            catch (Exception exception)
            {
                exception.ShowInExceptionBox();
            }
        });
        
        /// <summary>
        /// Добавить фильтр к паре.
        /// </summary>
        public ICommand AddFilterSubFilter => new RelayCommand<CustomElementPair>(pair =>
        {
            try
            {
                pair.FilterModelsForSubCategories.Add(new FilterModel());
            }
            catch (Exception exception)
            {
                exception.ShowInExceptionBox();
            }
        });
        
        /// <summary>
        /// Удалить фильтр у пары.
        /// </summary>
        public ICommand RemoveFilter => new RelayCommand<FilterModel>(filter =>
        {
            foreach (var pair in Pairs)
            {
                if (pair.FiltersForMainCategory.Contains(filter))
                {
                    pair.FiltersForMainCategory.Remove(filter);
                    return;
                }
                
                if (pair.FilterModelsForSubCategories.Contains(filter))
                {
                    pair.FilterModelsForSubCategories.Remove(filter);
                    return;
                }
            }
        });

        /// <summary>
        /// Сохранение настроек
        /// </summary>
        public override ICommand SaveSettings => new RelayCommandWithoutParameter(() =>
        {
            _permanentConfigurations.Pairs =
                Pairs.ToList().Select(i => i.ToSaveMode()).ToList();
            
            _userSettings.Set(Configurations, nameof(SavedJoinConfigurations));
        });
        
        /// <summary>
        /// Сохранение текущей конфигурации.
        /// </summary>
        public ICommand SaveCurrentConfiguration => new RelayCommand<SavedJoinConfigurations>(config =>
        {
            config.Pairs = Pairs.ToList().Select(i => i.ToSaveMode()).ToList();
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
                _elementConnectorService.JoinElements(_uiApplication.ActiveUIDocument.Document, pairs, Option);
            }
            catch (Exception e)
            {
                e.ShowInExceptionBox();
            }
        });
    }
}