using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Globalization;
using TaskNotes.MVVM.Models;
using CommunityToolkit.Maui.Media;
using CommunityToolkit.Maui.Alerts;
using System.Threading;

namespace TaskNotes.MVVM.ViewModels;

public class TaskGestorViewModel : INotifyPropertyChanged
{
    // ==========================
    // SERVICIOS
    // ==========================
    private readonly ISpeechToText _speechToText;

    // ==========================
    // COLECCIONES
    // ==========================
    public ObservableCollection<TaskItem> Tasks { get; } = new();

    public ObservableCollection<TaskPriority> Priorities { get; } =
        new(Enum.GetValues(typeof(TaskPriority)).Cast<TaskPriority>());

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

    // ==========================
    // ESTADOS UX
    // ==========================
    private bool _isListening;
    public bool IsListening
    {
        get => _isListening;
        set
        {
            _isListening = value;
            OnPropertyChanged();
        }
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

    public bool IsFormValid =>
        !string.IsNullOrWhiteSpace(NewTaskTitle);

    public bool CanSave =>
        IsFormValid && !IsSaving && !IsListening;

    // ==========================
    // COMANDOS
    // ==========================
    public ICommand AddTaskCommand { get; }
    public ICommand DeleteTaskCommand { get; }
    public ICommand ToggleCompleteCommand { get; }

    public ICommand StartListeningCommand { get; }
    public ICommand StopListeningCommand { get; }

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

        LoadMockData();
    }

    // ==========================
    // VOZ A TEXTO
    // ==========================
    private async Task StartListeningAsync()
    {
        try
        {
            var isGranted = await _speechToText.RequestPermissions(CancellationToken.None);
            if (!isGranted)
            {
                await Toast.Make("Permiso de micrófono denegado").Show();
                return;
            }

            _speechToText.RecognitionResultUpdated += OnRecognitionTextUpdated;
            _speechToText.RecognitionResultCompleted += OnRecognitionTextCompleted;

            await _speechToText.StartListenAsync(new SpeechToTextOptions
            {
                Culture = new CultureInfo("es-ES"),
                ShouldReportPartialResults = true
            }, CancellationToken.None);

            IsListening = true;
        }
        catch
        {
            await Toast.Make("Error al iniciar el reconocimiento de voz").Show();
        }
    }

    private async Task StopListeningAsync()
    {
        await _speechToText.StopListenAsync(CancellationToken.None);
        IsListening = false;

        _speechToText.RecognitionResultUpdated -= OnRecognitionTextUpdated;
        _speechToText.RecognitionResultCompleted -= OnRecognitionTextCompleted;
    }

    private void OnRecognitionTextUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs args)
    {
        NewTaskTitle = args.RecognitionResult;
    }

    private void OnRecognitionTextCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs args)
    {
        if (args.RecognitionResult.IsSuccessful)
            NewTaskTitle = args.RecognitionResult.Text;

        IsListening = false;
    }

    // ==========================
    // GESTIÓN DE TAREAS
    // ==========================
    private async Task AddTaskAsync()
    {
        if (!CanSave) return;

        IsSaving = true;

        try
        {
            Tasks.Add(new TaskItem
            {
                Title = NewTaskTitle,
                Description = NewTaskDescription,
                DueDate = NewTaskDate,
                Priority = NewTaskPriority,
                IsCompleted = false
            });

            await Toast.Make("Tarea guardada ✅").Show();

            NewTaskTitle = string.Empty;
            NewTaskDescription = string.Empty;
            NewTaskDate = DateTime.Now;

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
        if (Tasks.Contains(task))
            Tasks.Remove(task);
    }

    private void ToggleComplete(TaskItem task)
    {
        task.IsCompleted = !task.IsCompleted;
        OnPropertyChanged(nameof(Tasks));
    }

    private void LoadMockData()
    {
        Tasks.Add(new TaskItem
        {
            Title = "Aprender MAUI",
            Description = "Dominar MVVM",
            Priority = TaskPriority.Alta,
            DueDate = DateTime.Now
        });
    }

    // ==========================
    // NOTIFICACIÓN
    // ==========================
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
