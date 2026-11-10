using AlMaystroApp.Models;
using AlMaystroApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AlMaystroApp.Pages;

// صف واحد في قائمة الحجوزات
public class BookingRow
{
    public int Id { get; set; }
    public string Status { get; set; } = "Pending";   // الحالة الأصلية من السيرفر
    public string Initial { get; set; } = "?";
    public string CusName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string BookDate { get; set; } = "";
    public string ServiceName { get; set; } = "";
    public bool HasService => !string.IsNullOrWhiteSpace(ServiceName);
    public string StatusText { get; set; } = "";
    public Color StatusColor { get; set; } = Colors.Gray;
}

public partial class BookingsPage : ContentPage
{
    private readonly ApiService _api;

    public BookingsPage(ApiService api)
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

    private static (string Text, Color Color) StatusInfo(string? status) => status switch
    {
        "Confirmed" => ("بيتخدم", Color.FromArgb("#3DAA6A")),
        "Cancelled" => ("ملغي", Color.FromArgb("#CC2200")),
        "Completed" => ("تم", Color.FromArgb("#4A9DC4")),
        "NoShow"    => ("لم يحضر", Color.FromArgb("#9A8A6A")),
        _           => ("في الانتظار", Color.FromArgb("#C9A84C"))
    };

    private async Task LoadData(DateTime from, DateTime to)
    {
        loader.IsVisible = true;
        loader.IsRunning = true;

        List<BookingDto> data;

        if (from.Date == DateTime.Today.AddDays(-30) && to.Date == DateTime.Today)
            data = await _api.GetBookingsAsync();
        else
            data = await _api.GetBookingsRangeAsync(from, to);

        lblCount.Text = $"📅 {data.Count} حجز من {from:dd/MM} إلى {to:dd/MM}";

        bookList.ItemsSource = data.Select(b =>
        {
            var (text, color) = StatusInfo(b.Status);
            return new BookingRow
            {
                Id = b.Id ?? 0,
                Initial = string.IsNullOrWhiteSpace(b.CusName) ? "?" : b.CusName!.Trim().Substring(0, 1),
                CusName = b.CusName ?? "عميل",
                Phone = b.CusPhone ?? "–",
                BookDate = b.BookingDate ?? "",
                ServiceName = b.ServiceName ?? "",
                Status = b.Status ?? "Pending",
                StatusText = text,
                StatusColor = color
            };
        }).ToList();

        loader.IsVisible = false;
        loader.IsRunning = false;
    }

    // اضغط على حجز -> إلغاء / رجوع للانتظار / اتصال
    // "بيتخدم" و"تم" بيتحددوا من الكاشير (لما تتعمل فاتورة) ومينفعش يتغيروا من الموبايل.
    private async void OnBookingTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is not BookingRow row) return;

        string title = $"{row.CusName} - {row.Phone}";
        string? pick;

        if (row.Status is "Confirmed" or "Completed")
        {
            title += row.Status == "Completed"
                ? "\nتم (اتعملّه فاتورة) - مينفعش تتغير"
                : "\nبيتخدم دلوقتي على الكاشير - مينفعش تتغير";
            pick = await DisplayActionSheet(title, "إغلاق", null, "اتصال");
        }
        else if (row.Status == "Pending")
        {
            pick = await DisplayActionSheet(title, "إغلاق", null, "إلغاء الحجز", "اتصال");
        }
        else // Cancelled / NoShow
        {
            pick = await DisplayActionSheet(title, "إغلاق", null, "رجوع للانتظار", "اتصال");
        }

        if (pick == "اتصال")
        {
            try { PhoneDialer.Default.Open(row.Phone); } catch { }
            return;
        }

        string? status = pick switch
        {
            "إلغاء الحجز" => "Cancelled",
            "رجوع للانتظار" => "Pending",
            _ => null
        };
        if (status is null) return;

        try
        {
            var owner = IPlatformApplication.Current!.Services.GetRequiredService<ApiClient>();
            await owner.InitAsync();
            if (!owner.IsLoggedIn)
            {
                await DisplayAlert("تسجيل الدخول", "لازم تسجل دخول الإدارة الأول، وبعدها ارجع وغيّر حالة الحجز.", "حسناً");
                await Shell.Current.GoToAsync("login?next=back");
                return;
            }

            await owner.SetBookingStatusAsync(row.Id, status);
            await LoadData(dpFrom.Date, dpTo.Date);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            // الكاشير اشتغل على الحجز ده قبل ما الموبايل يتحدّث
            await DisplayAlert("مينفعش", "الحجز ده اتخدم أو اتعمله فاتورة من الكاشير ومينفعش تتغير حالته.", "حسناً");
            await LoadData(dpFrom.Date, dpTo.Date);
        }
        catch
        {
            await DisplayAlert("خطأ", "فشل تحديث الحالة", "حسناً");
        }
    }
}
