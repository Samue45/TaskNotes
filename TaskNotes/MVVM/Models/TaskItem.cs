using CommunityToolkit.Mvvm.ComponentModel;

namespace TaskNotes.MVVM.Models;

public enum TaskPriority
{
    Alta = 0,
    Media = 1,
    Baja = 2
}

public partial class TaskItem : ObservableObject
{
    [ObservableProperty]
    private Guid id;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private DateTime dueDate;

    [ObservableProperty]
    private TaskPriority priority;

    [ObservableProperty]
    private bool isCompleted;

    public TaskItem()
    {
        Id = Guid.NewGuid();
        DueDate = DateTime.Now;
        Priority = TaskPriority.Baja;
        IsCompleted = false;
    }
}
