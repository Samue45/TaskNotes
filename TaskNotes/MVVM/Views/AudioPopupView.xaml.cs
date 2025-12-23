using CommunityToolkit.Maui.Views;
using TaskNotes.MVVM.ViewModels;

namespace TaskNotes.MVVM.Views;

public partial class AudioPopupView : Popup
{
    public AudioPopupView(TaskGestorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        this.BackgroundColor = Colors.Transparent;

    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        var vm = this.BindingContext as TaskGestorViewModel;

        if (vm != null)
        {
            // 1. Detenemos y esperamos el proceso de audio
            await vm.FinalizeAudioAsync();

            // 2. Ejecutamos el guardado automático si el título no está vacío
            if (vm.AddTaskCommand.CanExecute(null) && !string.IsNullOrWhiteSpace(vm.NewTaskTitle))
            {
                vm.AddTaskCommand.Execute(null);
            }
        }

        // 3. Cerramos el Popup (esto ahora siempre funcionará al primer clic)
        await CloseAsync();
    }
}