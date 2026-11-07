using AlMaystroApp.Services;
using AlMaystroApp.ViewModels;
using AlMaystroApp.Views;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace AlMaystroApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit();
        // ---- keep these from your OLD MauiProgram.cs if they were there (check: git diff MauiProgram.cs) ----
        // .UseLocalNotification()                       // Plugin.LocalNotification
        // .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); })

        // ---- new owner-app services / pages ----
        builder.Services.AddSingleton<ApiClient>();
        builder.Services.AddTransient<LoginViewModel>(); builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<CategoriesViewModel>(); builder.Services.AddTransient<CategoriesPage>();
        builder.Services.AddTransient<ServicesViewModel>(); builder.Services.AddTransient<ServicesPage>();
        builder.Services.AddTransient<BookingsViewModel>(); builder.Services.AddTransient<BookingsPage>();

        // ---- existing services (REQUIRED by NotificationForegroundService: GetService<ApiService>()) ----
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<NotificationPollingService>();
        // If ApiService needs an HttpClient/other args, register them the way your old file did.

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}