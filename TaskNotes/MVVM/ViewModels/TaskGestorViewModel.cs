using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Media;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TaskNotes.MVVM.Models;

namespace TaskNotes.MVVM.ViewModels;

public partial class TaskGestorViewModel : INotifyPropertyChanged
{
    // ==========================
    // 1. FUENTE DE LA VERDAD (PRIVADA)
    // ==========================
    // Aquí guardamos TODO siempre. Nunca borramos nada de aquí por un filtro.
    private List<TaskItem> _allTasks = new();
    private string _currentFilter = "Todas"; // Guardamos el filtro actual

    // ==========================
    // SERVICIOS
    // ==========================
    private readonly ISpeechToText _speechToText;

    // ==========================
    // COLECCIONES (PÚBLICAS)
    // ==========================
    // Esta es la lista que ve el usuario (se vacía y rellena según el filtro)
    public ObservableCollection<TaskItem> Tasks { get; } = new();

    public ObservableCollection<TaskPriority> Priorities { get; } =
        new(Enum.GetValues(typeof(TaskPriority)).Cast<TaskPriority>());

    // ==========================
    // PROPIEDADES VISUALES (FILTROS)
    // ==========================
    private bool _isFilterActive;
    public bool IsFilterActive
    {
        get => _isFilterActive;
        set { _isFilterActive = value; OnPropertyChanged(); }
    }

    private string _filterMessage;
    public string FilterMessage
    {
        get => _filterMessage;
        set { _filterMessage = value; OnPropertyChanged(); }
    }

    // ==========================
    // FORMULARIO
    // ==========================
    private string _newTaskTitle;
    public string NewTaskTitle
    {
        get => _newTaskTitle;
        set
        {
            _newTaskTitle = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsFormValid));
            OnPropertyChanged(nameof(CanSave));
        }
    }

    private string _newTaskDescription;
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

    // ==========================
    // ESTADOS DE EDICIÓN
    // ==========================
    private TaskItem? _taskBeingEdited;
    public bool IsEditing => _taskBeingEdited != null;
    public string SaveButtonText => IsEditing ? "Actualizar Tarea" : "Guardar Tarea";

    private bool _isListening;
    public bool IsListening
    {
        get => _isListening;
        set { _isListening = value; OnPropertyChanged(); }
    }

    private bool _isSaving;
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

    public bool IsFormValid => !string.IsNullOrWhiteSpace(NewTaskTitle);
    public bool CanSave => IsFormValid && !IsSaving && !IsListening;

    // ==========================
    // COMANDOS
    // ==========================
    public ICommand AddTaskCommand { get; }
    public ICommand DeleteTaskCommand { get; }
    public ICommand ToggleCompleteCommand { get; }
    public ICommand StartListeningCommand { get; }
    public ICommand StopListeningCommand { get; }

    // Comandos nuevos para Filtro y Orden
    public ICommand FilterTasksCommand { get; }
    public ICommand SortByPriorityCommand { get; }

    // ==========================
    // CONSTRUCTOR
    // ==========================
    public TaskGestorViewModel(ISpeechToText speechToText)
    {
        _speechToText = speechToText;

        NewTaskDate = DateTime.Now;
        NewTaskPriority = TaskPriority.Baja;

        AddTaskCommand = new Command(async () => await AddTaskAsync());
        DeleteTaskCommand = new Command<TaskItem>(DeleteTask);
        ToggleCompleteCommand = new Command<TaskItem>(ToggleComplete);

        StartListeningCommand = new Command(async () => await StartListeningAsync());
        StopListeningCommand = new Command(async () => await StopListeningAsync());

        // Inicializamos los nuevos comandos
        FilterTasksCommand = new Command<string>(ApplyFilterSelection);
        SortByPriorityCommand = new Command(SortByPriority);

        LoadMockData();
    }

    // ==========================
    // LOGICA DE FILTRADO (EL CEREBRO)
    // ==========================
    private void ApplyFilterSelection(string priorityString)
    {
        if (string.IsNullOrEmpty(priorityString)) return;

        _currentFilter = priorityString;
        RefreshTaskList();
    }

    // Método centralizado para actualizar la lista visible (Tasks) basándose en _allTasks
    private void RefreshTaskList()
    {
        // 1. Empezamos filtrando la lista maestra
        IEnumerable<TaskItem> filteredData = _allTasks;

        if (_currentFilter != "Todas" && _currentFilter != "Cancelar")
        {
            // Convertimos el string "Alta" al Enum TaskPriority.Alta
            if (Enum.TryParse<TaskPriority>(_currentFilter, out var priorityEnum))
            {
                filteredData = _allTasks.Where(t => t.Priority == priorityEnum);

                // UX: Actualizamos mensajes visuales
                IsFilterActive = true;
                FilterMessage = $"Filtro activo: {priorityEnum}";
            }
        }
        else
        {
            // Reset
            IsFilterActive = false;
            FilterMessage = string.Empty;
            _currentFilter = "Todas";
        }

        // 2. Actualizamos la colección Observable SIN romper la referencia
        Tasks.Clear();
        foreach (var task in filteredData)
        {
            Tasks.Add(task);
        }
    }

    private void SortByPriority()
    {
        // Ordenamos la lista visible actual: Alta (0) -> Media (1) -> Baja (2)
        var sorted = Tasks.OrderBy(t => t.Priority).ToList();

        Tasks.Clear();
        foreach (var task in sorted) Tasks.Add(task);

        Toast.Make("Lista ordenada por prioridad ⚡").Show();
    }

    // ==========================
    // CRUD ACTUALIZADO
    // ==========================
    private async Task AddTaskAsync()
    {
        if (!CanSave) return;
        IsSaving = true;

        try
        {
            if (IsEditing && _taskBeingEdited != null)
            {
                // UPDATE
                _taskBeingEdited.Title = NewTaskTitle;
                _taskBeingEdited.Description = NewTaskDescription;
                _taskBeingEdited.DueDate = NewTaskDate;
                _taskBeingEdited.Priority = NewTaskPriority;

                // IMPORTANTE: No necesitamos tocar _allTasks porque es una referencia en memoria,
                // pero sí refrescamos la lista visual por si el cambio de prioridad afecta al filtro actual.
                RefreshTaskList();
                await Toast.Make("Tarea actualizada 📝").Show();
            }
            else
            {
                // INSERT
                var newTask = new TaskItem
                {
                    Title = NewTaskTitle,
                    Description = NewTaskDescription,
                    DueDate = NewTaskDate,
                    Priority = NewTaskPriority,
                    IsCompleted = false
                };

                // Agregamos a la lista MAESTRA
                _allTasks.Add(newTask);

                // Refrescamos la visual (si hay un filtro activo, tal vez la nueva tarea no aparezca, eso es correcto)
                RefreshTaskList();

                await Toast.Make("Nueva tarea creada ✅").Show();
            }

            PrepareForNew();

            if (Application.Current.MainPage.Navigation.NavigationStack.Count > 1)
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

        // 1. Borramos de la maestra
        if (_allTasks.Contains(task))
        {
            _allTasks.Remove(task);
        }

        // 2. Refrescamos la visual
        RefreshTaskList();
    }

    // ==========================
    // AUXILIARES
    // ==========================
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
        task.IsCompleted = !task.IsCompleted;
        // No es necesario refrescar lista completa, el binding se encarga
    }

    private void LoadMockData()
    {
        // Cargamos datos falsos en la lista MAESTRA
        _allTasks.Add(new TaskItem { Title = "Aprender MAUI", Description = "Dominar MVVM", Priority = TaskPriority.Alta, DueDate = DateTime.Now.AddDays(1) });
        _allTasks.Add(new TaskItem { Title = "Comprar café", Description = "Grano entero", Priority = TaskPriority.Baja, DueDate = DateTime.Now });
        _allTasks.Add(new TaskItem { Title = "Revisar PRs", Description = "Github", Priority = TaskPriority.Media, DueDate = DateTime.Now.AddDays(2) });

        // Sincronizamos con la visual
        RefreshTaskList();
    }

    // ==========================
    // SPEECH TO TEXT & NOTIFY
    // ==========================
    private async Task StartListeningAsync()
    {
        try
        {
            var isGranted = await _speechToText.RequestPermissions(CancellationToken.None);
            if (!isGranted) { await Toast.Make("Sin permiso de micrófono").Show(); return; }

            _speechToText.RecognitionResultUpdated += OnRecognitionTextUpdated;
            _speechToText.RecognitionResultCompleted += OnRecognitionTextCompleted;

            await _speechToText.StartListenAsync(new SpeechToTextOptions { Culture = new CultureInfo("es-ES"), ShouldReportPartialResults = true }, CancellationToken.None);
            IsListening = true;
        }
        catch { await Toast.Make("Error de voz").Show(); }
    }

    private async Task StopListeningAsync()
    {
        await _speechToText.StopListenAsync(CancellationToken.None);
        IsListening = false;
        _speechToText.RecognitionResultUpdated -= OnRecognitionTextUpdated;
        _speechToText.RecognitionResultCompleted -= OnRecognitionTextCompleted;
    }

    // Agrega esto a tu TaskGestorViewModel
    public async Task<bool> FinalizeAudioAsync()
    {
        if (IsListening)
        {
            await StopListeningAsync();
            // Esperamos un momento pequeño para que el evento de "Completado" se dispare
            await Task.Delay(300);
        }
        return true; // Indica que ya podemos cerrar
    }

    private void OnRecognitionTextUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs args) => NewTaskTitle = args.RecognitionResult;
    private void OnRecognitionTextCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs args)
    {
        if (args.RecognitionResult.IsSuccessful) NewTaskTitle = args.RecognitionResult.Text;
        IsListening = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}