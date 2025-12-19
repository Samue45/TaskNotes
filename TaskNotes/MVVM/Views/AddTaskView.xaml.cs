using TaskNotes.MVVM.Models;
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

        // Llenar el Picker con los valores del Enum (TaskPriority)
        // Esto se hace aquí para no ensuciar el XAML con helpers si quieres mantenerlo simple
        PriorityPicker.ItemsSource = Enum.GetValues(typeof(TaskPriority));
    }
}