using AlMaystroApp.Services;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;

namespace AlMaystroApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseLocalNotification()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Cairo-Regular.ttf", "Cairo");
                fonts.AddFont("Cairo-Bold.ttf", "CairoBold");
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });
        builder.Services.AddSingleton<NotificationPollingService>();
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddTransient<Pages.DashboardPage>();
        builder.Services.AddTransient<Pages.AppointmentsPage>();
        builder.Services.AddTransient<Pages.AttendancePage>();
        builder.Services.AddTransient<Pages.BookingsPage>();
        builder.Services.AddTransient<Pages.NotificationsPage>(); // ← جديد

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}