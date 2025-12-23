using CommunityToolkit.Maui.Views;
using TaskNotes.MVVM.ViewModels;

namespace TaskNotes.MVVM.Views;

public partial class AudioPopupView : Popup
{
    // Evita que el botón se ejecute más de una vez
    private bool _isClosing;

    public AudioPopupView(TaskGestorViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;

        BackgroundColor = Colors.Transparent;
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        // Protección contra doble clic
        if (_isClosing)
            return;

        _isClosing = true;

        if (BindingContext is not TaskGestorViewModel vm)
        {
            await CloseAsync();
            return;
        }

        // 1️º Finalizamos correctamente el proceso de audio
        // (detiene el micro y espera el resultado final)
        await vm.FinalizeAudioAsync();

        // 2️º Guardado automático SOLO si:
        // - Hay texto
        // - No está ya guardando
        // - No está escuchando
        if (!string.IsNullOrWhiteSpace(vm.NewTaskTitle) && vm.CanSave)
        {
            vm.AddTaskCommand.Execute(null);
        }

        // 3️º Cerramos el popup (una sola vez, seguro)
        await CloseAsync();
    }
}
