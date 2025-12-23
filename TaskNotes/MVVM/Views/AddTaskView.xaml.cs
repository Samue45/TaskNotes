using TaskNotes.MVVM.ViewModels;

namespace TaskNotes.MVVM.Views;

public partial class AddTaskView : ContentPage
{
    // Constructor que RECIBE el ViewModel existente
    public AddTaskView(TaskGestorViewModel viewModel)
    {
        InitializeComponent();

        // Asignamos el contexto compartido
        BindingContext = viewModel;
    }
}