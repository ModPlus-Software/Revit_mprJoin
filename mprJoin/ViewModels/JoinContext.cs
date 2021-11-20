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
        private readonly UIApplication _uiApplication;
        private List<string> _stringSelectedCategories;
        private readonly CollectorService _collectorService;
        private readonly ElementConnectorService _elementConnectorService;
        private readonly UserSettingsService _userSettings;
        private readonly JoinConfigurations _permanentConfigurations;
        private JoinConfigurations _currentConfiguration;

        public JoinContext(UIApplication uiApplication)
            : base(uiApplication)
        {
            _uiApplication = uiApplication;
            _collectorService = new CollectorService();
            _elementConnectorService = new ElementConnectorService(uiApplication.ActiveUIDocument);
            _userSettings = new UserSettingsService(PluginSetting.SaveFileName);
            var configurationsList = _userSettings.Get<ObservableCollection<JoinConfigurations>>(nameof(JoinConfigurations));
            _permanentConfigurations = configurationsList.FirstOrDefault(i => !i.IsEditable) ??
                new JoinConfigurations
                {
                    Name = ModPlusAPI.Language.GetItem("n1"),
                    IsEditable = false
                };

            Configurations = configurationsList.Any()
                ? configurationsList : new ObservableCollection<JoinConfigurations>
                {
                    _permanentConfigurations
                };
            if (!Configurations.Contains(_permanentConfigurations))
                Configurations.Add(_permanentConfigurations);
            _currentConfiguration = _permanentConfigurations;
        }

        /// <summary>
        /// Текущая выбранная конфигурация.
        /// </summary>
        public JoinConfigurations CurrentConfiguration
        {
            get => _currentConfiguration;
            set
            {
                _currentConfiguration = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Конфигурации в проекте.
        /// </summary>
        public ObservableCollection<JoinConfigurations> Configurations { get; set; }

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
                CurrentConfiguration.Pairs.Add(
                    new CustomElementPair(GetSelectedCategories(PluginSetting.AllowedCategoriesToJoin)));
            }
            catch (Exception exception)
            {
                exception.ShowInExceptionBox();
            }
        });

        /// <summary>
        /// Удалить пару.
        /// </summary>
        public ICommand RemovePair => 
            new RelayCommand<CustomElementPair>(pair => CurrentConfiguration.Pairs.Remove(pair));

        /// <summary>
        /// Удалить конфигурацию.
        /// </summary>
        public ICommand DeleteConfiguration => new RelayCommandWithoutParameter(
            () =>
            {
                Configurations.Remove(CurrentConfiguration);
                CurrentConfiguration = _permanentConfigurations;
            }, _ => CurrentConfiguration != null && CurrentConfiguration.IsEditable);

        /// <summary>
        /// Добавить конфигурацию.
        /// </summary>
        public ICommand AddConfiguration => new RelayCommandWithoutParameter(() =>
        {
            var newConfiguration = new JoinConfigurations
            {
                Name = DateTime.Now.ToString(CultureInfo.CurrentCulture)
            };
            Configurations.Add(newConfiguration);
            CurrentConfiguration = newConfiguration;
        });

        /// <summary>
        /// Переместить пару вниз.
        /// </summary>
        public ICommand ReplaceDown => new RelayCommand<CustomElementPair>(pair =>
        {
            try
            {
                var index = CurrentConfiguration.Pairs.IndexOf(pair);
                var maxIndex = CurrentConfiguration.Pairs.Count;
                if (index + 1 < maxIndex)
                    CurrentConfiguration.Pairs.Move(index, index + 1);
            }
            catch (Exception e)
            {
                e.ShowInExceptionBox();
            }
        });

        /// <summary>
        /// Переместить пару вниз.
        /// </summary>
        public ICommand ReplaceUp => new RelayCommand<CustomElementPair>(pair =>
        {
            try
            {
                var index = CurrentConfiguration.Pairs.IndexOf(pair);
                if (index > 0)
                    CurrentConfiguration.Pairs.Move(index, index - 1);
            }
            catch (Exception e)
            {
                e.ShowInExceptionBox();
            }
        });

        /// <summary>
        /// Команда выполнения
        /// </summary>
        public ICommand Execute => new RelayCommand<ScopeType>(scope =>
        {
            try
            {
                var pairs = CurrentConfiguration.Pairs.ToList();
                if (!pairs.Any())
                {
                    MessageBox.Show(ModPlusAPI.Language.GetItem("e2"), MessageBoxIcon.Alert);
                    return;
                }

                foreach (var pair in pairs)
                {
                    if (string.IsNullOrEmpty(pair.WhatToJoinCategory))
                    {
                        pair.WhatToJoinElements = new List<Element>();
                        pair.WhereToJoinElements = new List<Element>();
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
                        .Where(el => pair.WithWhatToJoin.Where(cat => cat.IsSelected).Select(cat => cat.Name).Any(cat =>
                            cat.Equals(el.Category.Name, StringComparison.InvariantCultureIgnoreCase)))
                        .ToList();
                }

                pairs.ForEach(p => p.ApplyFilters());
                _elementConnectorService.JoinElements(_uiApplication.ActiveUIDocument.Document, pairs, Option);
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
            _userSettings.Set(Configurations, nameof(JoinConfigurations));
        }
    }
}