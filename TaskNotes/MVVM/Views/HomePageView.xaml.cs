using TaskNotes.MVVM.ViewModels;

namespace TaskNotes.MVVM.Views;

public partial class HomePageView : ContentPage
{
    // Mantenemos la instancia del ViewModel aquí
    private TaskGestorViewModel _viewModel;

    public HomePageView()
    {
        InitializeComponent();

        // 1. Inicializamos el ViewModel
        _viewModel = new TaskGestorViewModel();

        // 2. Lo asignamos al contexto de esta vista
        BindingContext = _viewModel;
    }

    private async void OnAddTaskClicked(object sender, EventArgs e)
    {
        // 3. PASAMOS la misma instancia del ViewModel a la vista de agregar
        await Navigation.PushAsync(new AddTaskView(_viewModel));
    }
}

