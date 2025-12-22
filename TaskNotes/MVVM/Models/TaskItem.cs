using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Graphics;
using Microsoft.VisualBasic;

namespace TaskNotes.MVVM.Models;

public enum TaskPriority
{
    Baja,
    Media,
    Alta
}

public partial class TaskItem : ObservableObject
{
    [ObservableProperty]
    private string id;

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string description;

    [ObservableProperty]
    private DateTime dueDate;

    [ObservableProperty]
    private TimeSpan dueTime; // Añadido por si quieres hora específica

    [ObservableProperty]
    private TaskPriority priority;

    [ObservableProperty]
    private bool isCompleted;

    // Propiedad calculada solo para la UI (Color de la etiqueta)
    public Color PriorityColor
    {
        get
        {
            return Priority switch
            {
                TaskPriority.Alta => Color.FromArgb("#FF6B6B"),   // Rojo suave
                TaskPriority.Media => Color.FromArgb("#FFD93D"),  // Amarillo suave
                TaskPriority.Baja => Color.FromArgb("#6BCB77"),   // Verde suave
                _ => Colors.Gray
            };
        }
    }

    partial void OnPriorityChanged(TaskPriority value)
    {
        OnPropertyChanged(nameof(PriorityColor));
    }

    public TaskItem()
    {
        Id = Guid.NewGuid().ToString();
        DueDate = DateTime.Now;
        Priority = TaskPriority.Baja;
    }
}