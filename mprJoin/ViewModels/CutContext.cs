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
        private readonly ElementConnectorService _elementConnectorService;

        public CutContext(UIApplication uiApplication, MainWindow mainWindow, UserSettingsService userSettingsService) 
            : base(uiApplication, mainWindow, userSettingsService)
        {
            _uiApplication = uiApplication;
            _elementConnectorService = new ElementConnectorService(uiApplication.ActiveUIDocument);
            var configurationsList = UserSettingsService.Get<ObservableCollection<JoinConfigurations>>(_cutConfigurationFieldName);
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
            
            AllowedCategories = PluginSetting.AllowedCategoriesToCut;
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
                _elementConnectorService.CutElements(_uiApplication.ActiveUIDocument.Document, pairs);
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // ignore
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
            UserSettingsService.Set(Configurations, _cutConfigurationFieldName);
        }
    }
}