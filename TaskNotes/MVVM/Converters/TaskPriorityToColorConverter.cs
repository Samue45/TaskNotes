using System.Globalization;
using TaskNotes.MVVM.Models;

namespace TaskNotes.MVVM.Converters;

public class TaskPriorityToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not TaskPriority priority)
            return Colors.Gray;

        return priority switch
        {
            TaskPriority.Alta => Color.FromArgb("#FF6B6B"),
            TaskPriority.Media => Color.FromArgb("#FFD93D"),
            TaskPriority.Baja => Color.FromArgb("#6BCB77"),
            _ => Colors.Gray
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}