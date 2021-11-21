namespace mprJoin
{
    using System.Collections.Generic;
    using System.Windows.Input;
    using Autodesk.Revit.UI;
    using ModPlusAPI.Mvvm;
    using ViewModels;
    using Views;

    /// <summary>
    /// Main context
    /// </summary>
    public class MainContext : ObservableObject
    {
        private readonly List<BaseContext> _contexts;

        public MainContext(UIApplication uiApplication, MainWindow mainWindow)
        {
            ContiguityContext = new ContiguityContext(uiApplication, mainWindow);
            JoinContext = new JoinContext(uiApplication, mainWindow);
            _contexts = new List<BaseContext>
            {
                ContiguityContext,
                JoinContext
            };
        }
        
        public ContiguityContext ContiguityContext { get; }
        
        public JoinContext JoinContext { get; }

        public ICommand SaveAllConfiguration =>
            new RelayCommandWithoutParameter(() => _contexts.ForEach(i => i.SaveSettings()));
    }
}
