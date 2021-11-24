﻿namespace mprJoin.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;
    using Autodesk.Revit.UI;
    using Enums;
    using Models;
    using ModPlus_Revit;
    using ModPlusAPI;
    using ModPlusAPI.Mvvm;
    using ModPlusAPI.Services;
    using ModPlusAPI.Windows;
    using Services;
    using Settings;
    using Views;

    public class JoinContext : BaseJoinCutContext
    {
        private readonly UIApplication _uiApplication;
        private readonly ElementConnectorService _elementConnectorService;
        private readonly UserSettingsService _userSettings;

        public JoinContext(UIApplication uiApplication, MainWindow mainWindow)
            : base(uiApplication, mainWindow)
        {
            _uiApplication = uiApplication;
            _elementConnectorService = new ElementConnectorService(uiApplication.ActiveUIDocument);
            _userSettings = new UserSettingsService(PluginSetting.SaveFileName);
            var configurationsList = _userSettings.Get<ObservableCollection<JoinConfigurations>>(nameof(JoinConfigurations));
            PermanentConfiguration = configurationsList.FirstOrDefault(i => !i.IsEditable) ??
                                     new JoinConfigurations
                                     {
                                         Name = Language.GetItem("n1"),
                                         IsEditable = false
                                     };

            Configurations = configurationsList.Any()
                ? configurationsList : new ObservableCollection<JoinConfigurations>
                {
                    PermanentConfiguration
                };
            if (!Configurations.Contains(PermanentConfiguration))
                Configurations.Add(PermanentConfiguration);
            JoinOption = _userSettings.Get<JoinOption>(nameof(JoinOption));
            AllowedCategories = PluginSetting.AllowedCategoriesToJoin;
        }
        
        /// <summary>
        /// Опции для работы сервиса.
        /// </summary>
        public JoinOption JoinOption
        {
            get => Enum.TryParse(
                UserConfigFile.GetValue(ModPlusConnector.Instance.Name, nameof(JoinOption)), out JoinOption b) ? b : JoinOption.Join;
            set => UserConfigFile.SetValue(
                ModPlusConnector.Instance.Name, nameof(JoinOption), value.ToString(), true);
        }

        /// <summary>
        /// Команда выполнения.
        /// </summary>
        public override ICommand Execute => new RelayCommand<ScopeType>(scope =>
        {
            try
            {
                if (scope == ScopeType.SelectedElement)
                    MainWindow.Hide();
                
                var pairs = CurrentConfiguration.Pairs.ToList();
                if (!pairs.Any())
                {
                    MessageBox.Show(Language.GetItem("e2"), MessageBoxIcon.Alert);
                    return;
                }

                SetElements(pairs, scope);
                _elementConnectorService.JoinElements(_uiApplication.ActiveUIDocument.Document, pairs, JoinOption);
            }
            catch (Exception e)
            {
                e.ShowInExceptionBox();
            }

            if (scope == ScopeType.SelectedElement)
                ModPlus.ShowModal(MainWindow);
        });

        /// <summary>
        /// Сохранение настроек
        /// </summary>
        public override void SaveSettings()
        {
            _userSettings.Set(Configurations, nameof(JoinConfigurations));
            _userSettings.Set(JoinOption, nameof(JoinOption));
        }
    }
}