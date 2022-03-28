namespace mprJoin;

using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ModPlus_Revit;
using ModPlusAPI.Windows;
using Views;

/// <summary>
/// Command
/// </summary>
[Regeneration(RegenerationOption.Manual)]
[Transaction(TransactionMode.Manual)]
public class Command : IExternalCommand
{
    /// <summary>
    /// Revit event
    /// </summary>
    public static RevitEvent RevitEvent { get; private set; }

    /// <inheritdoc />
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
#if !DEBUG
                ModPlusAPI.Statistic.SendCommandStarting(ModPlusConnector.Instance);
#endif
            RevitEvent = new RevitEvent();
            var win = new MainWindow();
            var context = new MainContext(commandData.Application, win);
            win.DataContext = context;

            ModPlus.ShowModeless(win);

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