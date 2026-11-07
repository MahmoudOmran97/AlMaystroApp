using AlMaystroApp.Services;
using AlMaystroApp.ViewModels;
using CommunityToolkit.Maui;
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
            .UseMauiCommunityToolkit()
            .UseLocalNotification()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Cairo-Regular.ttf", "Cairo");
                fonts.AddFont("Cairo-Bold.ttf", "CairoBold");
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // ---- الخدمات القديمة (الفواتير / الحضور / الإشعارات) ----
        builder.Services.AddSingleton<NotificationPollingService>();
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddTransient<Pages.DashboardPage>();
        builder.Services.AddTransient<Pages.AppointmentsPage>();
        builder.Services.AddTransient<Pages.AttendancePage>();
        builder.Services.AddTransient<Pages.BookingsPage>();
        builder.Services.AddTransient<Pages.NotificationsPage>();

        // ---- شاشات الإدارة الجديدة (تسجيل دخول / أقسام / خدمات / حجوزات المالك) ----
        builder.Services.AddSingleton<ApiClient>();
        builder.Services.AddTransient<LoginViewModel>();      builder.Services.AddTransient<Views.LoginPage>();
        builder.Services.AddTransient<CategoriesViewModel>(); builder.Services.AddTransient<Views.CategoriesPage>();
        builder.Services.AddTransient<ServicesViewModel>();   builder.Services.AddTransient<Views.ServicesPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
