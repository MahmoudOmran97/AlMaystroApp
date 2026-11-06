using AlMaystroApp.Services;

namespace AlMaystroApp.Pages;

public partial class AttendancePage : ContentPage
{
    private readonly ApiService _api;

    // ✅ احتفظ بالـ data
    private List<AlMaystroApp.Models.AttendanceDto> _currentData = new();

    public AttendancePage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        dpFrom.Date = DateTime.Today.AddDays(-30);
        dpTo.Date = DateTime.Today;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadData(dpFrom.Date, dpTo.Date);
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await LoadData(dpFrom.Date, dpTo.Date);
        refreshView.IsRefreshing = false;
    }

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        if (dpFrom.Date > dpTo.Date)
        {
            await DisplayAlert("خطأ", "تاريخ البداية يجب أن يكون قبل تاريخ النهاية", "حسناً");
            return;
        }
        await LoadData(dpFrom.Date, dpTo.Date);
    }

    private async Task LoadData(DateTime from, DateTime to)
    {
        loader.IsVisible = true;
        loader.IsRunning = true;

        if (from.Date == DateTime.Today && to.Date == DateTime.Today)
            _currentData = await _api.GetAttendanceAsync();
        else
            _currentData = await _api.GetAttendanceRangeAsync(from, to);

        lblCount.Text = $"✅ {_currentData.Count} سجل من {from:dd/MM} إلى {to:dd/MM}";

        attList.ItemsSource = _currentData.Select(a => new AttendanceItem
        {
            EmployeeId = a.EmployeeId ?? 0,
            Initial = (a.NameEmployee ?? "?").Substring(0, 1),
            Name = a.NameEmployee ?? "موظف",
            CheckInOut = $"دخول: {a.CheckIn ?? "–"}   |   خروج: {a.CheckOut ?? "–"}",
            DateFmt = a.AttendDate ?? "",
            Status = a.Status ?? "",
            StatusColor = GetStatusColor(a.Status),
            WorkedH = $"{a.WorkedHours} س",
            OvertimeH = $"إضافي: {a.OvertimeHours} س"
        }).ToList();

        loader.IsVisible = false;
        loader.IsRunning = false;
    }

    // ✅ Tap Handler
    private async void OnAttendanceTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is AttendanceItem item)
        {
            var original = _currentData.FirstOrDefault(x =>
                x.EmployeeId == item.EmployeeId &&
                x.AttendDate == item.DateFmt);

            if (original == null) return;

            string details =
                $"👤 الموظف: {original.NameEmployee ?? "–"}\n\n" +
                $"📅 التاريخ: {original.AttendDate ?? "–"}\n" +
                $"🟢 دخول: {original.CheckIn ?? "–"}\n" +
                $"🔴 خروج: {original.CheckOut ?? "–"}\n" +
                $"⏱️ ساعات العمل: {original.WorkedHours} س\n" +
                $"➕ الأوفر تايم: {original.OvertimeHours} س\n" +
                $"📋 الحالة: {original.Status ?? "–"}";

            await DisplayAlert($"تفاصيل حضور {original.NameEmployee}", details, "إغلاق");
        }
    }

    private static string GetStatusColor(string? s)
    {
        if (s == null) return "#9A9080";
        if (s.Contains("حاضر") || s.Contains("تم")) return "#4CAF7A";
        if (s.Contains("غائب")) return "#D46C6C";
        if (s.Contains("إجازة")) return "#D4A84C";
        return "#9A9080";
    }
}

// ✅ Model
public class AttendanceItem
{
    public int EmployeeId { get; set; }
    public string Initial { get; set; } = "";
    public string Name { get; set; } = "";
    public string CheckInOut { get; set; } = "";
    public string DateFmt { get; set; } = "";
    public string Status { get; set; } = "";
    public string StatusColor { get; set; } = "";
    public string WorkedH { get; set; } = "";
    public string OvertimeH { get; set; } = "";
}