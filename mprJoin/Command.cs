namespace mprJoin
{
    using System;
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Helpers;
    using ModPlusAPI.Windows;
    using Services;
    using Settings;
    using ViewModels;
    using Views;

    /// <summary>
    /// Command
    /// </summary>
    [Regeneration(RegenerationOption.Manual)]
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        /// <inheritdoc />
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            RevitExternalEventHandler.Init();
            var pluginSettings = new PluginSetting();
            var revitTask = new RevitTask();
            var collectorService = new CollectorService();
            var elementConnectorService = new ElementConnectorService(commandData.Application.ActiveUIDocument);
            try
            {
                var win = new MainWindow
                {
                    DataContext = new MainContext(),
                    JoinPageContext = new JoinPageContext(),
                    ContiguityPageContext = new ContiguityPageContext(
                        commandData.Application, pluginSettings, revitTask, collectorService, elementConnectorService)
                };

                ModPlus_Revit.ModPlus.ShowModeless(win);

                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception exception)
            {
                exception.ShowInExceptionBox();
                return Result.Failed;
            }
        }
    }
}
