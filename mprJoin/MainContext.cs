namespace mprJoin
{
    using Autodesk.Revit.UI;
    using ModPlusAPI;
    using ModPlusAPI.Mvvm;
    using ViewModels;

    /// <summary>
    /// Main context
    /// </summary>
    public class MainContext : ObservableObject
    {
        public MainContext(UIApplication uiApplication)
        {
            ContiguityContext = new ContiguityContext(uiApplication);
            JoinContext = new JoinContext();
        }
        
        public ContiguityContext ContiguityContext { get; }
        
        public JoinContext JoinContext { get; }
    }
}
