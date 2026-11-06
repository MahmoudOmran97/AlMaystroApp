using AlMaystroApp.Services;

namespace AlMaystroApp.Pages;

public partial class AppointmentsPage : ContentPage
{
    private readonly ApiService _api;

    // ✅ احتفظ بالـ data عشان تقدر توصلها في الـ Tap
    private List<AlMaystroApp.Models.AppointmentDto> _currentData = new();

    public AppointmentsPage(ApiService api)
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
            _currentData = await _api.GetAppointmentsAsync();
        else
            _currentData = await _api.GetAppointmentsRangeAsync(from, to);

        lblCount.Text = $"📋 {_currentData.Count} فاتورة من {from:dd/MM} إلى {to:dd/MM}";

        apptList.ItemsSource = _currentData.Select(a => new InvoiceItem
        {
            InvoiceId = a.InvoiceId ?? 0,
            Initial = (a.CusName ?? "?").Substring(0, 1),
            CusName = a.CusName ?? "عميل",
            BarberInfo = $"{a.SellCus ?? "–"}  ·  #{a.InvoiceId}", // ✅ SellCus بدل BarberName
            AppStatus = a.AppStatus ?? "",
            StatusColor = GetStatusColor(a.AppStatus),
            PayFmt = $"{Math.Round(a.Pay ?? 0):N0} ج",
            DateFmt = (a.SellDate ?? "").Length > 10
                    ? a.SellDate!.Substring(0, 10) : (a.SellDate ?? "")
        }).ToList();

        loader.IsVisible = false;
        loader.IsRunning = false;
    }

    // ✅ حدث الـ Tap
    private async void OnInvoiceTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is InvoiceItem item)
        {
            // جيب الـ original data
            var original = _currentData.FirstOrDefault(x => x.InvoiceId == item.InvoiceId);
            if (original == null) return;

            // ✅ اعرض التفاصيل في Popup
            string details =
                $"🧾 فاتورة رقم: #{original.InvoiceId}\n\n" +
                $"👤 العميل: {original.CusName ?? "–"}\n" +
               $"🪑 الكرسي: {original.SellCus ?? "–"}\n" +
                $"💰 المدفوع: {Math.Round(original.Pay ?? 0):N0} ج\n" +
                $"📊 الإجمالي: {Math.Round(original.SellTprice ?? 0):N0} ج\n" +
                $"📁 الحالة: {original.AppStatus ?? "–"}\n" +
                $"🏦 الخزينة: {original.Safe ?? "–"}\n" +
                $"👨‍💼 الكاشير: {original.NameCasher ?? "–"}\n" +
                $"📅 التاريخ: {original.SellDate ?? "–"}";

            await DisplayAlert($"تفاصيل الفاتورة #{original.InvoiceId}", details, "إغلاق");
        }
    }

    private static string GetStatusColor(string? s)
    {
        if (s == null) return "#9A9080";
        if (s.Contains("مدفوع") || s.Contains("مكتمل")) return "#5AADD4";
        if (s.Contains("غير مدفوع") || s.Contains("جاري")) return "#D4A84C";
        if (s.Contains("ملغي")) return "#D46C6C";
        return "#5AADD4";
    }
}

// ✅ Model للـ List
public class InvoiceItem
{
    public int InvoiceId { get; set; }
    public string Initial { get; set; } = "";
    public string CusName { get; set; } = "";
    public string BarberInfo { get; set; } = "";
    public string AppStatus { get; set; } = "";
    public string StatusColor { get; set; } = "";
    public string PayFmt { get; set; } = "";
    public string DateFmt { get; set; } = "";
}