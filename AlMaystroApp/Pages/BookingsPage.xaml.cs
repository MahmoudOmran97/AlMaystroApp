using AlMaystroApp.Services;

namespace AlMaystroApp.Pages;

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

    private async Task LoadData(DateTime from, DateTime to)
    {
        loader.IsVisible = true;
        loader.IsRunning = true;

        List<AlMaystroApp.Models.BookingDto> data;

        if (from.Date == DateTime.Today.AddDays(-30) && to.Date == DateTime.Today)
            data = await _api.GetBookingsAsync();
        else
            data = await _api.GetBookingsRangeAsync(from, to);

        lblCount.Text = $"📅 {data.Count} حجز من {from:dd/MM} إلى {to:dd/MM}";

        bookList.ItemsSource = data.Select(b => new
        {
            Initial = (b.CusName ?? "?").Substring(0, 1),
            CusName = b.CusName ?? "عميل",
            Phone = b.CusPhone ?? "–",
            BookDate = (b.BookingDate ?? "").Length > 10
                       ? b.BookingDate!.Substring(0, 10) : (b.BookingDate ?? "")
        }).ToList();

        loader.IsVisible = false;
        loader.IsRunning = false;
    }
}