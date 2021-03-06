namespace mprJoin.ViewModels;

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
using ModPlus_Revit;
using ModPlusAPI.Mvvm;
using ModPlusAPI.Services;
using ModPlusAPI.Windows;
using Services;
using Views;

public abstract class BaseJoinCutContext : BaseContext
{
    private readonly CollectorService _collectorService;
    private readonly UIApplication _uiApplication;

    protected BaseJoinCutContext(MainWindow mainWindow, UserSettingsService userSettingsService) 
        : base(mainWindow, userSettingsService)
    {
        _collectorService = new CollectorService();
        _uiApplication = ModPlus.UiApplication;
    }

    public List<BuiltInCategory> AllowedCategories { get; protected set; }

    public JoinConfigurations PermanentConfiguration { get; set; }

    /// <summary>
    /// Текущая выбранная конфигурация.
    /// </summary>
    public JoinConfigurations CurrentConfiguration
    {
        get => PermanentConfiguration;
        set
        {
            PermanentConfiguration = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Конфигурации в проекте.
    /// </summary>
    public ObservableCollection<JoinConfigurations> Configurations { get; set; }

    /// <summary>
    /// Добавить пару.
    /// </summary>
    public ICommand AddPair => new RelayCommandWithoutParameter(() =>
    {
        try
        {
            CurrentConfiguration.Pairs.Add(new CustomElementPair(AllowedCategories));
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
            CurrentConfiguration = PermanentConfiguration;
        }, _ => CurrentConfiguration is { IsEditable: true });

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
    public abstract ICommand Execute { get; }

    /// <summary>
    /// Сохранение настроек
    /// </summary>
    public abstract override void SaveSettings();

    public void SetElements(List<CustomElementPair> pairs, ScopeType scope)
    {
        var collector = _collectorService
            .GetFilteredElementCollector(_uiApplication.ActiveUIDocument, scope)
            .WhereElementIsNotElementType();

        foreach (var pair in pairs)
        {
            pair.WhatToJoinElements = collector

                // Оставляет возможные категории, без этого при следующей проверке получаю ошибку, полагаю,
                // что у какого то элемента не получается посмотреть категорию
                .WherePasses(new ElementMulticategoryFilter(AllowedCategories))
                .ToList();

            pair.WhereToJoinElements = collector

                // Оставляет возможные категории, без этого при следующей проверке получаю ошибку, полагаю,
                // что у какого то элемента не получается посмотреть категорию
                .WherePasses(new ElementMulticategoryFilter(AllowedCategories))
                .ToList();
        }

        pairs.ForEach(p => p.ApplyFilters());
    }
}