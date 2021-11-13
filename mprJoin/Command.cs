namespace mprJoin
{
    using System;
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using ModPlus_Revit;
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
            try
            {
                var win = new MainWindow
                {
                    DataContext = new MainContext(commandData.Application),
                };

                ModPlus.ShowModal(win);
                
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
