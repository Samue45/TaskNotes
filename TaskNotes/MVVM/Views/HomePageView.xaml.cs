using CommunityToolkit.Maui.Extensions;
using TaskNotes.MVVM.Models;
using TaskNotes.MVVM.ViewModels;

namespace TaskNotes.MVVM.Views;

public partial class HomePageView : ContentPage
{
    public HomePageView(TaskGestorViewModel viewModel)
    {
        InitializeComponent();

        // Asignamos el ViewModel a la vista
        BindingContext = viewModel;
    }

    // ============================
    // BOTÓN FILTRAR
    // ============================
    [Obsolete]
    private async void OnFilterClicked(object sender, EventArgs e)
    {
        if (BindingContext is not TaskGestorViewModel viewModel)
            return;

        // Mostrar opciones de filtrado
        string action = await DisplayActionSheet(
            "Filtrar tareas por prioridad:",
            "Cancelar",
            null,
            "Todas", "Alta", "Media", "Baja");

        // Si se cancela o no se selecciona nada, salimos
        if (string.IsNullOrWhiteSpace(action) || action == "Cancelar")
            return;

        // Ejecutamos directamente el comando (CanExecute no aporta aquí)
        viewModel.FilterTasksCommand.Execute(action);
    }

    // ============================
    // EDITAR TAREA
    // ============================
    private async void OnEditTaskTapped(object sender, TappedEventArgs e)
    {
        if (BindingContext is not TaskGestorViewModel viewModel)
            return;

        if (e.Parameter is not TaskItem taskSeleccionada)
            return;

        viewModel.PrepareForEdit(taskSeleccionada);

        await Navigation.PushAsync(new AddTaskView(viewModel));
    }

    // ============================
    // AÑADIR TAREA
    // ============================
    private async void OnAddTaskClicked(object sender, EventArgs e)
    {
        if (BindingContext is not TaskGestorViewModel viewModel)
            return;

        viewModel.PrepareForNew();

        await Navigation.PushAsync(new AddTaskView(viewModel));
    }

    // ============================
    // POPUP DE VOZ
    // ============================
    private void OnShowAudioPopupClicked(object sender, EventArgs e)
    {
        if (BindingContext is not TaskGestorViewModel viewModel)
            return;

        var popup = new AudioPopupView(viewModel);
        this.ShowPopup(popup);
    }
}
