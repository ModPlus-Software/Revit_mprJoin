namespace mprJoin.ViewModels
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

    public class CutContext : BaseJoinCutContext
    {
        private readonly string _cutConfigurationFieldName = "CutConfiguration";
        private readonly UIApplication _uiApplication;
        private readonly UserSettingsService _userSettings;
        private readonly ElementConnectorService _elementConnectorService;

        public CutContext(UIApplication uiApplication, MainWindow mainWindow) 
            : base(uiApplication, mainWindow)
        {
            _uiApplication = uiApplication;
            _userSettings = new UserSettingsService(PluginSetting.SaveFileName);
            _elementConnectorService = new ElementConnectorService(uiApplication.ActiveUIDocument);
            var configurationsList = _userSettings.Get<ObservableCollection<JoinConfigurations>>(_cutConfigurationFieldName);
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
            CutOptions = _userSettings.Get<CutOptions>(nameof(CutOptions));
            AllowedCategories = PluginSetting.AllowedCategoriesToCut;
        }
        
        /// <summary>
        /// Опции для работы сервиса.
        /// </summary>
        public CutOptions CutOptions
        {
            get => Enum.TryParse(
                UserConfigFile.GetValue(ModPlusConnector.Instance.Name, nameof(CutOptions)), out CutOptions b) ? b : CutOptions.Cut;
            set => UserConfigFile.SetValue(
                ModPlusConnector.Instance.Name, nameof(CutOptions), value.ToString(), true);
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
                _elementConnectorService.CutElements(_uiApplication.ActiveUIDocument.Document, pairs, CutOptions);
            }
            catch (Exception e)
            {
                e.ShowInExceptionBox();
            }

            if (scope == ScopeType.SelectedElement)
                ModPlus.ShowModal(MainWindow);
        });
        
        public override void SaveSettings()
        {
            _userSettings.Set(Configurations, _cutConfigurationFieldName);
            _userSettings.Set(CutOptions, nameof(CutOptions));
        }
    }
}