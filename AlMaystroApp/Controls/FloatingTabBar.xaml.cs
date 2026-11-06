namespace AlMaystroApp.Controls;

public partial class FloatingTabBar : ContentView
{
    public static readonly BindableProperty ActiveTabProperty =
        BindableProperty.Create(nameof(ActiveTab), typeof(string), typeof(FloatingTabBar),
            defaultValue: "", propertyChanged: OnActiveTabChanged);

    public string ActiveTab
    {
        get => (string)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
    }

    public FloatingTabBar()
    {
        InitializeComponent();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        ApplyActiveTab(ActiveTab);
    }

    private static void OnActiveTabChanged(BindableObject bindable, object oldVal, object newVal)
    {
        if (bindable is FloatingTabBar bar)
            bar.ApplyActiveTab(newVal?.ToString() ?? "");
    }

    private void ApplyActiveTab(string tab)
    {
        var frames = new[] { frmInvoice, frmAttend, frmBooking, frmNotif };
        var labels = new[] { lblInvoice, lblAttend, lblBooking, lblNotif };

        foreach (var f in frames)
        {
            f.BackgroundColor = Colors.Transparent;
            f.BorderColor = Colors.Transparent;
        }
        foreach (var l in labels)
            l.TextColor = Color.FromArgb("#606060");

        frmHome.BackgroundColor = Color.FromArgb("#CC2200");
        frmHome.BorderColor = Color.FromArgb("#FF4422");
        lblHome.TextColor = Color.FromArgb("#CC2200");

        var (frm, lbl) = tab switch
        {
            "Invoices" => (frmInvoice, lblInvoice),
            "Attend" => (frmAttend, lblAttend),
            "Bookings" => (frmBooking, lblBooking),
            "Notif" => (frmNotif, lblNotif),
            _ => (null, null)
        };

        if (frm != null)
        {
            frm.BackgroundColor = Color.FromArgb("#331100");
            frm.BorderColor = Color.FromArgb("#CC2200");
        }
        if (lbl != null)
            lbl.TextColor = Color.FromArgb("#CC2200");
    }

    private static readonly string[] Routes =
 {
    "//dashboard",      // index 0 - الرئيسية
    "//appointments",   // index 1 - الفواتير
    "//attendance",     // index 2 - الحضور
    "//bookings",       // index 3 - الحجوزات
    "//notifications"   // index 4 - إشعارات
};

    private void Navigate(int index)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                if (index < Routes.Length)
                    await Shell.Current.GoToAsync(Routes[index]);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Nav error: {ex.Message}");
            }
        });
    }

    private void OnHomeTapped(object s, EventArgs e) => Navigate(0);
    private void OnInvoiceTapped(object s, EventArgs e) => Navigate(1);
    private void OnAttendTapped(object s, EventArgs e) => Navigate(2);
    private void OnBookingTapped(object s, EventArgs e) => Navigate(3);
    private void OnNotifTapped(object s, EventArgs e) => Navigate(4);
}