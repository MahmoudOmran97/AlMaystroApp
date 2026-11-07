using AlMaystroApp.Services;
using AlMaystroApp.Views;

namespace AlMaystroApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // شاشات الإدارة (بتتفتح من زرار ⚙ في الصفحة الرئيسية)
        Routing.RegisterRoute("login", typeof(LoginPage));
        Routing.RegisterRoute("categories", typeof(CategoriesPage));
        Routing.RegisterRoute("services", typeof(ServicesPage));

        // لو التوكن خلص أو اترفض -> رجوع لتسجيل الدخول
        var api = IPlatformApplication.Current?.Services.GetService<ApiClient>();
        if (api != null)
        {
            api.Unauthorized += () => MainThread.BeginInvokeOnMainThread(async () =>
            {
                try { await GoToAsync("login"); } catch { }
            });
        }
    }

    public void SwitchTab(int index)
    {
        try
        {
            var tabBar = Items.OfType<TabBar>().FirstOrDefault();
            if (tabBar == null) return;

            if (index < tabBar.Items.Count)
            {
                CurrentItem = tabBar.Items[index];
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SwitchTab error: {ex.Message}");
        }
    }
}
