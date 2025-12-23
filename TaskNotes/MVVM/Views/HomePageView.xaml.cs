using CommunityToolkit.Maui.Extensions;
using TaskNotes.MVVM.Models;
using TaskNotes.MVVM.ViewModels;

namespace TaskNotes.MVVM.Views;

public partial class HomePageView : ContentPage
{
    public HomePageView(TaskGestorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // --- NUEVO: Lógica del botón de Filtro ---
    private async void OnFilterClicked(object sender, EventArgs e)
    {
        // 1. Mostrar ActionSheet nativo (mejor UX que botones sueltos)
        string action = await DisplayActionSheet(
            "Filtrar tareas por prioridad:",
            "Cancelar",
            null,
            "Todas", "Alta", "Media", "Baja");

        // 2. Si el usuario cancela, no hacemos nada
        if (action == "Cancelar" || action == null)
            return;

        // 3. Comunicar al ViewModel
        var viewModel = (TaskGestorViewModel)BindingContext;

        // Asegúrate de crear este Command en tu ViewModel
        if (viewModel.FilterTasksCommand.CanExecute(action))
        {
            viewModel.FilterTasksCommand.Execute(action);
        }
    }
    // ------------------------------------------

    private async void OnEditTaskTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is TaskItem taskSeleccionada)
        {
            var viewModel = (TaskGestorViewModel)BindingContext;
            viewModel.PrepareForEdit(taskSeleccionada);
            await Navigation.PushAsync(new AddTaskView(viewModel));
        }
    }

    private async void OnAddTaskClicked(object sender, EventArgs e)
    {
        var viewModel = (TaskGestorViewModel)BindingContext;
        viewModel.PrepareForNew();
        await Navigation.PushAsync(new AddTaskView(viewModel));
    }

    private void OnShowAudioPopupClicked(object sender, EventArgs e)
    {
        var popup = new AudioPopupView((TaskGestorViewModel)this.BindingContext);
        this.ShowPopup(popup);
    }
}