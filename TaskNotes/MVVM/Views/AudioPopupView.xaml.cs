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
        var vm = BindingContext as TaskGestorViewModel;

        if (vm != null)
        {
            // 1. Detener la grabación si está escuchando
            if (vm.IsListening)
            {
                // Es mejor usar el método del comando para asegurar consistencia
                if (vm.StopListeningCommand.CanExecute(null))
                    vm.StopListeningCommand.Execute(null);
            }

            // 2. AÑADIR LA TAREA (NUEVO)
            // Solo intentamos añadir si hay algo escrito en el título
            if (!string.IsNullOrWhiteSpace(vm.NewTaskTitle))
            {
                if (vm.AddTaskCommand.CanExecute(null))
                {
                    // Ejecutamos el comando que ya programaste en el ViewModel
                    // Esto añade la tarea a la lista y limpia el campo NewTaskTitle
                    vm.AddTaskCommand.Execute(null);
                }
            }
        }

        // 3. Cerrar el popup
        await CloseAsync();
    }
}