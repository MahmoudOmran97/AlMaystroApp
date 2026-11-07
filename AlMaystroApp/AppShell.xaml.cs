using AlMaystroApp.Services;
using AlMaystroApp.Views;

namespace AlMaystroApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("services", typeof(ServicesPage));

        // Token expired / rejected -> back to login
        var api = IPlatformApplication.Current!.Services.GetRequiredService<ApiClient>();
        api.Unauthorized += () => MainThread.BeginInvokeOnMainThread(async () =>
        {
            try { await GoToAsync("//login"); } catch { }
        });
    }
}