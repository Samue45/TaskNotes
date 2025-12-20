using TaskNotes.MVVM.Views;
using TaskNotes.MVVM.ViewModels;

namespace TaskNotes
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // 1. Obtenemos el ViewModel desde el contenedor de servicios.
            // Esto resolverá automáticamente la dependencia de ISpeechToText.
            var commonViewModel = Handler.MauiContext.Services.GetService<TaskGestorViewModel>();

            // 2. Lo pasamos al constructor de la HomePageView
            var homePage = new HomePageView(commonViewModel);

            // 3. Devolvemos la ventana con la navegación
            return new Window(new NavigationPage(homePage));
        }
    }
}