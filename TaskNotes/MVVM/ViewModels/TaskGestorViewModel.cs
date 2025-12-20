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
    // Servicio de Voz a Texto
    private readonly ISpeechToText _speechToText;
    public ObservableCollection<TaskItem> Tasks { get; set; } = new ObservableCollection<TaskItem>();

    // 2. PROPIEDADES DEL FORMULARIO
    private string _newTaskTitle;
    public string NewTaskTitle
    {
        get => _newTaskTitle;
        set { _newTaskTitle = value; OnPropertyChanged(); }
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

    // Propiedad para saber si está escuchando (útil para cambiar el icono del micrófono en la UI)
    private bool _isListening;
    public bool IsListening
    {
        get => _isListening;
        set { _isListening = value; OnPropertyChanged(); }
    }

    // 3. COMANDOS
    public ICommand AddTaskCommand { get; private set; }
    public ICommand DeleteTaskCommand { get; private set; }
    public ICommand ToggleCompleteCommand { get; private set; }

    // Comandos de Voz
    public ICommand StartListeningCommand { get; private set; }
    public ICommand StopListeningCommand { get; private set; }

    // 4. CONSTRUCTOR
    public TaskGestorViewModel(ISpeechToText speechToText)
    {
        _speechToText = speechToText;

        // Valores por defecto
        NewTaskDate = DateTime.Now;
        NewTaskPriority = TaskPriority.Baja;

        // Definición de Comandos
        AddTaskCommand = new Command(async () => await AddTaskAsync());
        DeleteTaskCommand = new Command<TaskItem>(DeleteTask);
        ToggleCompleteCommand = new Command<TaskItem>(ToggleComplete);

        // Definición de Comandos de Voz
        StartListeningCommand = new Command(async () => await StartListeningAsync());
        StopListeningCommand = new Command(async () => await StopListeningAsync());

        LoadMockData();
    }

    // 5. LÓGICA DE VOZ (StartAsync / StopAsync)
    private async Task StartListeningAsync()
    {
        try
        {
            // 1. Verificar Permisos
            var isGranted = await _speechToText.RequestPermissions(CancellationToken.None);
            if (!isGranted)
            {
                await Toast.Make("Permiso de micrófono denegado").Show(CancellationToken.None);
                return;
            }

            // 2. Suscribirse a eventos
            _speechToText.RecognitionResultUpdated += OnRecognitionTextUpdated;
            _speechToText.RecognitionResultCompleted += OnRecognitionTextCompleted;

            // 3. Configurar opciones (Idioma del dispositivo, reportar resultados parciales)
            var options = new SpeechToTextOptions
            {
                Culture = new CultureInfo("es-ES"),
                ShouldReportPartialResults = true
            };

            // 4. Iniciar escucha
            await _speechToText.StartListenAsync(options, CancellationToken.None);
            IsListening = true;
        }
        catch (Microsoft.Maui.ApplicationModel.PermissionException ex)  
        {
            await Toast.Make("Por favor, habilite el reconocimiento de voz en línea en la configuración de privacidad del sistema.").Show(CancellationToken.None);
        }
    }

    private async Task StopListeningAsync()
    {
        await _speechToText.StopListenAsync(CancellationToken.None);
        IsListening = false;

        // Es importante desuscribirse para evitar fugas de memoria
        _speechToText.RecognitionResultUpdated -= OnRecognitionTextUpdated;
        _speechToText.RecognitionResultCompleted -= OnRecognitionTextCompleted;
    }

    // Evento: Se ejecuta mientras hablas (tiempo real)
    private void OnRecognitionTextUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs args)
    {
        // Actualizamos el Título con lo que se está reconociendo
        NewTaskTitle = args.RecognitionResult;
    }

    // Evento: Se ejecuta cuando terminas de hablar o pausas
    private void OnRecognitionTextCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs args)
    {
        // 1. Extraemos el objeto resultado (que es de tipo SpeechToTextResult)
        var result = args.RecognitionResult;

        // 2. Verificamos el éxito dentro de 'result', no de 'args'
        if (result.IsSuccessful)
        {
            // 3. Asignamos la propiedad .Text (que es el string), no el objeto completo
            NewTaskTitle = result.Text;
        }
        else
        {
            // 4. Manejo de errores accediendo a la excepción dentro de result
            if (result.Exception != null)
            {
                Toast.Make($"Error: {result.Exception.Message}").Show(CancellationToken.None);
            }
        }

        // Apagamos el estado de escucha
        IsListening = false;
    }

    // 6. LÓGICA DE GESTIÓN DE TAREAS
    private async Task AddTaskAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTaskTitle)) return;

        var task = new TaskItem
        {
            Title = NewTaskTitle,
            Description = NewTaskDescription,
            DueDate = NewTaskDate,
            Priority = NewTaskPriority,
            IsCompleted = false
        };

        Tasks.Add(task);

        // Resetear campos
        NewTaskTitle = string.Empty;
        NewTaskDescription = string.Empty;
        NewTaskDate = DateTime.Now;

        // Si estamos en una página de navegación, volvemos atrás
        if (Application.Current.MainPage.Navigation.NavigationStack.Count > 1)
            await Application.Current.MainPage.Navigation.PopAsync();
    }

    private void DeleteTask(TaskItem task)
    {
        if (Tasks.Contains(task)) Tasks.Remove(task);
    }

    private void ToggleComplete(TaskItem task)
    {
        // Lógica para marcar como completada
        OnPropertyChanged(nameof(Tasks));
    }

    private void LoadMockData()
    {
        Tasks.Add(new TaskItem { Title = "Aprender MAUI", Description = "Dominar MVVM", Priority = TaskPriority.Alta, DueDate = DateTime.Now });
        Tasks.Add(new TaskItem { Title = "Prueba de Voz", Description = "Dime algo", Priority = TaskPriority.Media, IsCompleted = true });
    }

    // 7. NOTIFICACIÓN DE CAMBIOS
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}