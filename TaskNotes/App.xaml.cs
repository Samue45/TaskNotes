using Microsoft.Extensions.DependencyInjection;
using TaskNotes.MVVM.Views;

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
            return new Window(new NavigationPage(new HomePageView()));
        }
    }
}