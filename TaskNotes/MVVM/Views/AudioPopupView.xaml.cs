using CommunityToolkit.Maui.Views;
using TaskNotes.MVVM.ViewModels;

namespace TaskNotes.MVVM.Views;

public partial class AudioPopupView : Popup 
{
    public AudioPopupView(TaskGestorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        // Al cerrar, si el usuario estaba grabando, detenemos la grabación por seguridad
        var vm = BindingContext as TaskGestorViewModel;
        if (vm != null && vm.IsListening)
        {
            // Ejecutamos el comando de detener si existe y se puede ejecutar
            if (vm.StopListeningCommand.CanExecute(null))
                vm.StopListeningCommand.Execute(null);
        }

        await CloseAsync();
    }
}