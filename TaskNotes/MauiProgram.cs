using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui.Media;
using TaskNotes.MVVM.ViewModels; // Asegúrate de añadir este using
using TaskNotes.MVVM.Views;      // Asegúrate de añadir este using

namespace TaskNotes
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // --- CONFIGURACIÓN DE SERVICIOS (INYECCIÓN DE DEPENDENCIAS) ---

            // 1. Registramos el servicio de Voz del Community Toolkit
            // Usamos la instancia estática .Default que ya viene implementada para Windows/Android/iOS
            builder.Services.AddSingleton<ISpeechToText>(SpeechToText.Default);

            // 2. Registramos el ViewModel como Singleton
            // Esto permite que el ViewModel reciba el ISpeechToText en su constructor automáticamente
            builder.Services.AddSingleton<TaskGestorViewModel>();

            // 3. Registramos las Vistas (Opcional, pero recomendado)
            builder.Services.AddTransient<HomePageView>();
            builder.Services.AddTransient<MainPage>();

            return builder.Build();
        }
    }
}