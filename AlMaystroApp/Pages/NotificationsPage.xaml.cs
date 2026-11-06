using AlMaystroApp.Services;

namespace AlMaystroApp.Pages;

public partial class NotificationsPage : ContentPage
{
    private readonly ApiService _api;

    public NotificationsPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadData();
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await LoadData();
        refreshView.IsRefreshing = false;
    }

    private async Task LoadData()
    {
        loader.IsVisible = true;
        loader.IsRunning = true;

        var data = await _api.GetNotificationsAsync();

        notifList.ItemsSource = data.Select(n => new
        {
            Message = n.Notfication1 ?? "إشعار",
            DateFmt = n.CreatedAt?.ToString("dd/MM/yyyy  hh:mm tt") ?? "",
            BgColor = n.IsRead ? "#1C1C1C" : "#1A1408",
            BorderColor = n.IsRead ? "#242424" : "#3A2E18"
        }).ToList();

        loader.IsVisible = false;
        loader.IsRunning = false;
    }
}