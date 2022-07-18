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

public class JoinContext : BaseJoinCutContext
{
    private readonly UIApplication _uiApplication;
    private readonly ElementConnectorService _elementConnectorService;

    public JoinContext(MainWindow mainWindow, UserSettingsService userSettingsService)
        : base(mainWindow, userSettingsService)
    {
        _uiApplication = ModPlus.UiApplication;
        _elementConnectorService = new ElementConnectorService(_uiApplication.ActiveUIDocument);
        var configurationsList = userSettingsService.Get<ObservableCollection<JoinConfigurations>>(nameof(JoinConfigurations));
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
        JoinOption = UserSettingsService.Get<JoinOption>(nameof(JoinOption));
        AllowedCategories = PluginSetting.AllowedCategoriesToJoin;
        configurationsList.ToList().ForEach(i => i.Pairs.ToList().ForEach(p => p.SetSettingsAfterGetInSaveFile(AllowedCategories)));
    }

    /// <summary>
    /// Опции для работы сервиса.
    /// </summary>
    public JoinOption JoinOption { get; set; }

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
                _elementConnectorService.JoinElements(_uiApplication.ActiveUIDocument.Document, pairs, JoinOption);
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

    /// <summary>
    /// Сохранение настроек
    /// </summary>
    public override void SaveSettings()
    {
        UserSettingsService.Set(Configurations, nameof(JoinConfigurations));
        UserSettingsService.Set(JoinOption, nameof(JoinOption));
    }
}