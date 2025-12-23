using System.Globalization;

namespace TaskNotes.MVVM.Converters;

public class BoolToTitleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isEditing)
        {
            return isEditing ? "Editar Tarea" : "Nueva Tarea";
        }
        return "Tarea";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}