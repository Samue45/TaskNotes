using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TaskNotes.MVVM.Models; // Asegúrate de que este using coincida con tu modelo TaskItem

namespace TaskNotes.MVVM.ViewModels;

public class TaskGestorViewModel : INotifyPropertyChanged
{
    // 1. COLECCIÓN DE TAREAS
    // Usamos ObservableCollection para que la lista en pantalla se actualice sola al agregar/borrar.
    public ObservableCollection<TaskItem> Tasks { get; set; } = new ObservableCollection<TaskItem>();

    // 2. PROPIEDADES PARA EL FORMULARIO (ADD TASK)
    // Necesitamos campos privados y propiedades públicas para notificar cambios.

    private string _newTaskTitle;
    public string NewTaskTitle
    {
        get => _newTaskTitle;
        set
        {
            _newTaskTitle = value;
            OnPropertyChanged(); // Notifica a la UI
        }
    }

    private string _newTaskDescription;
    public string NewTaskDescription
    {
        get => _newTaskDescription;
        set
        {
            _newTaskDescription = value;
            OnPropertyChanged();
        }
    }

    private DateTime _newTaskDate;
    public DateTime NewTaskDate
    {
        get => _newTaskDate;
        set
        {
            _newTaskDate = value;
            OnPropertyChanged();
        }
    }

    private TaskPriority _newTaskPriority;
    public TaskPriority NewTaskPriority
    {
        get => _newTaskPriority;
        set
        {
            _newTaskPriority = value;
            OnPropertyChanged();
        }
    }

    // 3. COMANDOS (ACCIONES DE LOS BOTONES)
    public ICommand AddTaskCommand { get; private set; }
    public ICommand DeleteTaskCommand { get; private set; }
    public ICommand ToggleCompleteCommand { get; private set; }


    // 4. CONSTRUCTOR
    public TaskGestorViewModel()
    {
        // Inicializar valores por defecto
        NewTaskDate = DateTime.Now;
        NewTaskPriority = TaskPriority.Baja;

        // Definir la lógica de los comandos
        AddTaskCommand = new Command(async () => await AddTaskAsync());
        DeleteTaskCommand = new Command<TaskItem>(DeleteTask);
        ToggleCompleteCommand = new Command<TaskItem>(ToggleComplete);

        // Cargar datos de prueba (Para que veas el diseño bonito al ejecutar)
        LoadMockData();
    }


    // 5. LÓGICA DE NEGOCIO

    private async Task AddTaskAsync()
    {
        // Validación básica
        if (string.IsNullOrWhiteSpace(NewTaskTitle))
            return;

        // Crear la nueva tarea basada en los inputs
        var task = new TaskItem
        {
            Title = NewTaskTitle,
            Description = NewTaskDescription,
            DueDate = NewTaskDate,
            Priority = NewTaskPriority,
            IsCompleted = false
        };

        // Añadir a la lista
        Tasks.Add(task);

        // Limpiar el formulario
        NewTaskTitle = string.Empty;
        NewTaskDescription = string.Empty;
        NewTaskDate = DateTime.Now;
        NewTaskPriority = TaskPriority.Baja;

        // Navegación: Volver atrás (Cierra la vista AddTask)
        await Application.Current.MainPage.Navigation.PopAsync();
    }

    private void DeleteTask(TaskItem task)
    {
        if (Tasks.Contains(task))
        {
            Tasks.Remove(task);
        }
    }

    private void ToggleComplete(TaskItem task)
    {
        // Como TaskItem hereda de ObservableObject o implementa INotifyPropertyChanged,
        // la UI se actualiza sola, pero aquí puedes añadir lógica extra (ej. guardar en BBDD)
    }

    private void LoadMockData()
    {
        Tasks.Add(new TaskItem
        {
            Title = "Diseñar Interfaz",
            Description = "Definir paleta de colores en Figma",
            Priority = TaskPriority.Alta,
            DueDate = DateTime.Now.AddDays(1)
        });

        Tasks.Add(new TaskItem
        {
            Title = "Revisar correos",
            Description = "Contestar al cliente sobre el presupuesto",
            Priority = TaskPriority.Media,
            IsCompleted = true
        });
    }

    // 6. IMPLEMENTACIÓN DE INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;

    // Helper para no escribir el nombre de la propiedad manualmente cada vez
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}