namespace mprJoin.ViewModels;

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

    public CutContext(MainWindow mainWindow, UserSettingsService userSettingsService)
        : base(mainWindow, userSettingsService)
    {
        _uiApplication = ModPlus.UiApplication;
        _elementConnectorService = new ElementConnectorService(_uiApplication.ActiveUIDocument);
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
        CutOptions = UserSettingsService.Get<CutOptions>(nameof(CutOptions));
        AllowedCategories = PluginSetting.AllowedCategoriesToCut;
        configurationsList.ToList().ForEach(i => i.Pairs.ToList().ForEach(p => p.SetSettingsAfterGetInSaveFile(AllowedCategories)));
    }

    /// <summary>
    /// Опции для работы сервиса.
    /// </summary>
    public CutOptions CutOptions { get; set; }

    /// <summary>
    /// Команда выполнения.
    /// </summary>
    public override ICommand Execute => new RelayCommand<ScopeType>(scope =>
    {
        Command.RevitEvent.Run(() =>
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
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // ignore
            }
            catch (Exception e)
            {
                e.ShowInExceptionBox();
            }
            finally
            {
                if (scope == ScopeType.SelectedElement)
                    ModPlus.ShowModeless(MainWindow);
            }
        });
    });

    public override void SaveSettings()
    {
        UserSettingsService.Set(CutOptions, nameof(CutOptions));
        UserSettingsService.Set(Configurations, _cutConfigurationFieldName);
    }
}