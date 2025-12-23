using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Media;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TaskNotes.MVVM.Models;

namespace TaskNotes.MVVM.ViewModels;

/// <summary>
/// ViewModel principal de la aplicación.
/// Responsable de:
/// - Gestionar el ciclo de vida de las tareas (CRUD)
/// - Aplicar filtros y ordenaciones
/// - Coordinar la entrada por voz
/// - Exponer el estado necesario para la UI
/// </summary>
public partial class TaskGestorViewModel : INotifyPropertyChanged
{
    // =====================================================
    // CONSTANTES
    // =====================================================
    // Evitan el uso de strings mágicos y centralizan valores
    private const string FilterAll = "Todas";
    private const string FilterCancel = "Cancelar";

    // =====================================================
    // FUENTE DE LA VERDAD (ESTADO INTERNO)
    // =====================================================
    // Lista MAESTRA de tareas.
    // Nunca se bindea directamente a la UI.
    // Cualquier filtro u orden parte SIEMPRE de esta lista.
    private readonly List<TaskItem> _allTasks = new();

    // Filtro actualmente seleccionado por el usuario
    private string _currentFilter = FilterAll;

    // =====================================================
    // SERVICIOS EXTERNOS
    // =====================================================
    // Servicio de reconocimiento de voz (inyectado)
    private readonly ISpeechToText _speechToText;

    // Token para cancelar escucha de voz y evitar crash al cerrar la app
    private CancellationTokenSource? _speechCts;

    // =====================================================
    // COLECCIONES EXPUESTAS A LA UI
    // =====================================================
    // Colección observable que consume la UI.
    // Su contenido se recalcula desde _allTasks.
    public ObservableCollection<TaskItem> Tasks { get; } = new();

    // Lista de prioridades disponibles para Pickers / filtros
    public ObservableCollection<TaskPriority> Priorities { get; } =
        new(Enum.GetValues(typeof(TaskPriority)).Cast<TaskPriority>());

    // =====================================================
    // ESTADO VISUAL DEL FILTRADO
    // =====================================================
    private bool _isFilterActive;

    /// <summary>
    /// Indica si hay un filtro aplicado actualmente.
    /// Controla visibilidad de mensajes en la UI.
    /// </summary>
    public bool IsFilterActive
    {
        get => _isFilterActive;
        set { _isFilterActive = value; OnPropertyChanged(); }
    }

    private string _filterMessage = string.Empty;

    /// <summary>
    /// Texto informativo que indica qué filtro está activo.
    /// </summary>
    public string FilterMessage
    {
        get => _filterMessage;
        set { _filterMessage = value; OnPropertyChanged(); }
    }

    // =====================================================
    // CAMPOS DEL FORMULARIO (CREAR / EDITAR)
    // =====================================================
    private string _newTaskTitle = string.Empty;

    /// <summary>
    /// Título de la tarea (campo obligatorio).
    /// </summary>
    public string NewTaskTitle
    {
        get => _newTaskTitle;
        set
        {
            _newTaskTitle = value;
            OnPropertyChanged();

            // Estos estados dependen del título
            OnPropertyChanged(nameof(IsFormValid));
            OnPropertyChanged(nameof(CanSave));
        }
    }

    private string _newTaskDescription = string.Empty;
    public string NewTaskDescription
    {
        get => _newTaskDescription;
        set { _newTaskDescription = value; OnPropertyChanged(); }
    }

    private DateTime _newTaskDate;
    public DateTime NewTaskDate
    {
        get => _newTaskDate;
        set { _newTaskDate = value; OnPropertyChanged(); }
    }

    private TaskPriority _newTaskPriority;
    public TaskPriority NewTaskPriority
    {
        get => _newTaskPriority;
        set { _newTaskPriority = value; OnPropertyChanged(); }
    }

    // =====================================================
    // ESTADO DE EDICIÓN
    // =====================================================
    // Si no es null → estamos editando una tarea existente
    private TaskItem? _taskBeingEdited;

    /// <summary>
    /// Indica si el formulario está en modo edición.
    /// </summary>
    public bool IsEditing => _taskBeingEdited != null;

    /// <summary>
    /// Texto dinámico del botón Guardar / Actualizar.
    /// </summary>
    public string SaveButtonText =>
        IsEditing ? "Actualizar Tarea" : "Guardar Tarea";

    // =====================================================
    // ESTADOS DE PROCESO (UX)
    // =====================================================
    private bool _isListening;

    /// <summary>
    /// Indica si el micrófono está activo.
    /// Deshabilita acciones mientras se escucha.
    /// </summary>
    public bool IsListening
    {
        get => _isListening;
        set
        {
            _isListening = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSave));
        }
    }

    private bool _isSaving;

    /// <summary>
    /// Indica si la tarea se está guardando.
    /// Evita dobles envíos.
    /// </summary>
    public bool IsSaving
    {
        get => _isSaving;
        set
        {
            _isSaving = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSave));
        }
    }

    /// <summary>
    /// Validación mínima del formulario.
    /// </summary>
    public bool IsFormValid => !string.IsNullOrWhiteSpace(NewTaskTitle);

    /// <summary>
    /// Determina si el usuario puede guardar la tarea.
    /// </summary>
    public bool CanSave => IsFormValid && !IsSaving && !IsListening;

    // =====================================================
    // COMANDOS (INTERACCIÓN UI → VM)
    // =====================================================
    public ICommand AddTaskCommand { get; }
    public ICommand DeleteTaskCommand { get; }
    public ICommand ToggleCompleteCommand { get; }
    public ICommand StartListeningCommand { get; }
    public ICommand StopListeningCommand { get; }
    public ICommand FilterTasksCommand { get; }
    public ICommand SortByPriorityCommand { get; }

    // =====================================================
    // CONSTRUCTOR
    // =====================================================
    public TaskGestorViewModel(ISpeechToText speechToText)
    {
        _speechToText = speechToText;

        // Valores por defecto del formulario
        NewTaskDate = DateTime.Now;
        NewTaskPriority = TaskPriority.Baja;

        // Inicialización de comandos
        AddTaskCommand = new Command(async () => await AddTaskAsync());
        DeleteTaskCommand = new Command<TaskItem>(DeleteTask);
        ToggleCompleteCommand = new Command<TaskItem>(ToggleComplete);
        StartListeningCommand = new Command(async () => await StartListeningAsync());
        StopListeningCommand = new Command(async () => await StopListeningAsync());
        FilterTasksCommand = new Command<string>(ApplyFilterSelection);
        SortByPriorityCommand = new Command(SortByPriority);

        // Datos temporales para desarrollo / pruebas
        LoadMockData();
    }

    // =====================================================
    // FILTRADO Y ORDENACIÓN
    // =====================================================
    private void ApplyFilterSelection(string filter)
    {
        if (string.IsNullOrEmpty(filter) || filter == FilterCancel)
            return;

        _currentFilter = filter;
        RefreshTaskList();
    }

    private void RefreshTaskList()
    {
        IEnumerable<TaskItem> filteredData = _allTasks;

        if (_currentFilter != FilterAll &&
            Enum.TryParse<TaskPriority>(_currentFilter, out var priority))
        {
            filteredData = filteredData.Where(t => t.Priority == priority);
            IsFilterActive = true;
            FilterMessage = $"Filtro activo: {priority}";
        }
        else
        {
            IsFilterActive = false;
            FilterMessage = string.Empty;
            _currentFilter = FilterAll;
        }

        Tasks.Clear();
        foreach (var task in filteredData)
            Tasks.Add(task);
    }

    private void SortByPriority()
    {
        var sorted = Tasks.OrderBy(t => t.Priority).ToList();

        Tasks.Clear();
        foreach (var task in sorted)
            Tasks.Add(task);

        Toast.Make("Lista ordenada por prioridad ⚡").Show();
    }

    // =====================================================
    // CRUD DE TAREAS
    // =====================================================
    private async Task AddTaskAsync()
    {
        if (!CanSave) return;

        IsSaving = true;

        try
        {
            if (IsEditing && _taskBeingEdited != null)
            {
                // Actualización
                _taskBeingEdited.Title = NewTaskTitle;
                _taskBeingEdited.Description = NewTaskDescription;
                _taskBeingEdited.DueDate = NewTaskDate;
                _taskBeingEdited.Priority = NewTaskPriority;

                RefreshTaskList();
                await Toast.Make("Tarea actualizada 📝").Show();
            }
            else
            {
                // Creación
                var newTask = new TaskItem
                {
                    Title = NewTaskTitle,
                    Description = NewTaskDescription,
                    DueDate = NewTaskDate,
                    Priority = NewTaskPriority
                };

                _allTasks.Add(newTask);
                RefreshTaskList();
                await Toast.Make("Nueva tarea creada ✅").Show();
            }

            PrepareForNew();

            // Cierra la pantalla si venimos de una navegación
            if (Application.Current?.MainPage?.Navigation.NavigationStack.Count > 1)
                await Application.Current.MainPage.Navigation.PopAsync();
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void DeleteTask(TaskItem task)
    {
        if (task == null) return;

        _allTasks.Remove(task);
        RefreshTaskList();
    }

    // =====================================================
    // UTILIDADES DE EDICIÓN
    // =====================================================
    public void PrepareForEdit(TaskItem task)
    {
        _taskBeingEdited = task;

        NewTaskTitle = task.Title;
        NewTaskDescription = task.Description;
        NewTaskDate = task.DueDate;
        NewTaskPriority = task.Priority;

        RefreshEditState();
    }

    public void PrepareForNew()
    {
        _taskBeingEdited = null;

        NewTaskTitle = string.Empty;
        NewTaskDescription = string.Empty;
        NewTaskDate = DateTime.Now;
        NewTaskPriority = TaskPriority.Baja;

        RefreshEditState();
    }

    private void RefreshEditState()
    {
        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(SaveButtonText));
    }

    private void ToggleComplete(TaskItem task)
    {
        if (task != null)
            task.IsCompleted = !task.IsCompleted;
    }

    // =====================================================
    // DATOS DE PRUEBA
    // =====================================================
    private void LoadMockData()
    {
        _allTasks.Add(new TaskItem { Title = "Aprender MAUI", Priority = TaskPriority.Alta });
        _allTasks.Add(new TaskItem { Title = "Comprar café", Priority = TaskPriority.Baja });
        _allTasks.Add(new TaskItem { Title = "Revisar PRs", Priority = TaskPriority.Media });

        RefreshTaskList();
    }

    // =====================================================
    // SPEECH TO TEXT
    // =====================================================
    private async Task StartListeningAsync()
    {
        // Cancela cualquier escucha previa
        _speechCts?.Cancel();
        _speechCts = new CancellationTokenSource();

        try
        {
            var granted = await _speechToText.RequestPermissions(_speechCts.Token);
            if (!granted)
            {
                await Toast.Make("Sin permiso de micrófono").Show();
                return;
            }

            // Evita múltiples suscripciones al mismo evento
            _speechToText.RecognitionResultUpdated -= OnRecognitionTextUpdated;
            _speechToText.RecognitionResultCompleted -= OnRecognitionTextCompleted;

            _speechToText.RecognitionResultUpdated += OnRecognitionTextUpdated;
            _speechToText.RecognitionResultCompleted += OnRecognitionTextCompleted;

            IsListening = true;

            await _speechToText.StartListenAsync(
                new SpeechToTextOptions
                {
                    Culture = new CultureInfo("es-ES"),
                    ShouldReportPartialResults = true
                },
                _speechCts.Token);
        }
        catch (OperationCanceledException)
        {
            // Normal al cerrar la app o popup
        }
        catch (ObjectDisposedException)
        {
            // Normal cuando MAUI destruye servicios al cerrar
        }
        catch
        {
            await Toast.Make("Error de reconocimiento de voz").Show();
        }
    }

    private async Task StopListeningAsync()
    {
        try
        {
            _speechCts?.Cancel();
            await _speechToText.StopListenAsync(CancellationToken.None);
        }
        catch
        {
            // Ignorar errores durante cierre
        }
        finally
        {
            IsListening = false;

            _speechToText.RecognitionResultUpdated -= OnRecognitionTextUpdated;
            _speechToText.RecognitionResultCompleted -= OnRecognitionTextCompleted;
        }
    }

    public async Task<bool> FinalizeAudioAsync()
    {
        if (IsListening)
        {
            await StopListeningAsync();
            await Task.Delay(300);
        }

        return true;
    }

    private void OnRecognitionTextUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs e)
        => NewTaskTitle = e.RecognitionResult;

    private void OnRecognitionTextCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs e)
    {
        if (e.RecognitionResult.IsSuccessful)
            NewTaskTitle = e.RecognitionResult.Text;

        IsListening = false;
    }

    // =====================================================
    // CANCELACIÓN SEGURA AL CERRAR LA APP
    // =====================================================
    public void OnAppClosing()
    {
        _speechCts?.Cancel();
    }

    // =====================================================
    // INotifyPropertyChanged
    // =====================================================
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
