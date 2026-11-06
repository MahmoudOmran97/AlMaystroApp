namespace AlMaystroApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }

    public void SwitchTab(int index)
    {
        try
        {
            var tabBar = Items.OfType<TabBar>().FirstOrDefault();
            if (tabBar == null) return;

            if (index < tabBar.Items.Count)
            {
                // ← السطر الجديد: لازم تغير CurrentItem على الـ Shell نفسه
                CurrentItem = tabBar.Items[index];
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SwitchTab error: {ex.Message}");
        }
    }
}