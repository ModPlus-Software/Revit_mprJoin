namespace mprJoin
{
    using System.Collections.Generic;
    using System.Windows.Input;
    using Autodesk.Revit.UI;
    using ModPlusAPI.Mvvm;
    using ViewModels;

    /// <summary>
    /// Main context
    /// </summary>
    public class MainContext : ObservableObject
    {
        private readonly List<BaseContext> _contexts;

        public MainContext(UIApplication uiApplication)
        {
            ContiguityContext = new ContiguityContext(uiApplication);
            JoinContext = new JoinContext(uiApplication);
            _contexts = new List<BaseContext>
            {
                ContiguityContext,
                JoinContext
            };
        }
        
        public ContiguityContext ContiguityContext { get; }
        
        public JoinContext JoinContext { get; }

        public ICommand SaveAllConfiguration => new RelayCommandWithoutParameter(() =>
        {
            _contexts.ForEach(i => i.SaveSettings.Execute(null));
        });
    }
}
