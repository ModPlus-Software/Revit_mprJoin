namespace mprJoin.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Autodesk.Revit.DB;
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

public class ContiguityContext : BaseContext
{
    private readonly UIApplication _uiApplication;
    private readonly CollectorService _collectorService;
    private ObservableCollection<SelectedCategory> _selectedCategories;
    private readonly ElementConnectorService _elementConnectorService;

    public ContiguityContext(UIApplication uiApplication, MainWindow mainWindow, UserSettingsService userSettingsService)
        : base(uiApplication, mainWindow, userSettingsService)
    {
        _collectorService = new CollectorService();
        _uiApplication = uiApplication;
        _elementConnectorService = new ElementConnectorService(uiApplication.ActiveUIDocument);
        var list = UserSettingsService.Get<ObservableCollection<SelectedCategory>>(nameof(SelectedCategories));
        _selectedCategories = list.Any() ? list : null;

        ContiguityOption = UserSettingsService.Get<ContiguityOption>(nameof(ContiguityOption));
        FirstEnd = UserSettingsService.Get<bool>(nameof(FirstEnd));
        SecondEnd = UserSettingsService.Get<bool>(nameof(SecondEnd));
    }

    /// <summary>
    /// Опции для работы сервиса
    /// </summary>
    public ContiguityOption ContiguityOption { get; set; }

    /// <summary>
    /// Список моделей категорий для вывода пользователю
    /// </summary>
    public ObservableCollection<SelectedCategory> SelectedCategories =>
        _selectedCategories ??= new ObservableCollection<SelectedCategory>(GetSelectedCategories(PluginSetting.AllowedCategoriesToContiguity));

    /// <summary>
    /// Обрабатывать ли начало элемента
    /// </summary>
    public bool FirstEnd { get; set; }

    /// <summary>
    /// Обрабатывать ли конец элемента
    /// </summary>
    public bool SecondEnd { get; set; }

    /// <summary>
    /// Команда выполнения
    /// </summary>
    public ICommand Execute => new RelayCommand<ScopeType>(scope =>
    {
        Command.RevitEvent.Run(() =>
        {
            try
            {
                if (scope == ScopeType.SelectedElement)
                    MainWindow.Hide();

                var selectedCategories = SelectedCategories.Where(i => i.IsSelected).Select(i => i.Name).ToList();
                var elements = _collectorService
                    .GetFilteredElementCollector(_uiApplication.ActiveUIDocument, scope)
                    .WhereElementIsNotElementType()

                    // Оставляет возможные категории, без этого при следующей проверке получаю ошибку, полагаю,
                    // что у какого то элемента не получается посмотреть категорию
                    .WherePasses(new ElementMulticategoryFilter(PluginSetting.AllowedCategoriesToContiguity))

                    // Оставляет только выбранные
                    .Where(element => selectedCategories.Contains(element.Category.Name))
                    .ToList();
                if (!elements.Any())
                {
                    MessageBox.Show(Language.GetItem("e1"), MessageBoxIcon.Alert);
                }
                else
                {
                    _elementConnectorService.DoContiguityAction(
                        elements,
                        ContiguityOption,
                        new Tuple<bool, bool>(FirstEnd, SecondEnd));
                }
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
        UserSettingsService.Set(SelectedCategories, nameof(SelectedCategories));
        UserSettingsService.Set(ContiguityOption, nameof(ContiguityOption));
        UserSettingsService.Set(FirstEnd, nameof(FirstEnd));
        UserSettingsService.Set(SecondEnd, nameof(SecondEnd));
    }
}