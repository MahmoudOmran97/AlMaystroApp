using AlMaystroApp.Models;
using AlMaystroApp.Services;
using Plugin.LocalNotification;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AlMaystroApp.Pages;

public partial class DashboardPage : ContentPage
{
    private readonly ApiService _api;

    public DashboardPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        lblDate.Text = DateTime.Now.ToString("dddd، d MMMM", new System.Globalization.CultureInfo("ar-EG"));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LocalNotificationCenter.Current.RequestNotificationPermission();
        await LoadData();
    }

    private async void OnRefreshClicked(object sender, EventArgs e) => await LoadData();
    private async void OnRefreshing(object sender, EventArgs e)
    {
        await LoadData();
        refreshView.IsRefreshing = false;
    }

    private async Task LoadData()
    {
        try
        {
            // Show loader
            loader.IsVisible = true;
            loader.IsRunning = true;
            statsGrid.IsVisible = false;
            finCard.IsVisible = false;
            recentSection.IsVisible = false;
            errFrame.IsVisible = false;
           
            btnRefresh.Opacity = 0.5; // اجعل الزر باهتاً أثناء التحميل
            btnRefresh.InputTransparent = true;
            // Fetch in parallel
            var summaryTask = _api.GetSummaryAsync();
            var apptTask = _api.GetAppointmentsAsync();
            await Task.WhenAll(summaryTask, apptTask);

            var summary = await summaryTask;
            var appts = await apptTask;

            // Summary cards
            if (summary != null)
            {
                lblCount.Text = summary.TotalInvoices.ToString();
                lblPaid.Text = Fmt(summary.TotalPaid);
                lblRev.Text = Fmt(summary.TotalRevenue);
                lblUnpaid.Text = Fmt(summary.TotalUnpaid);

                // الملخص المالي
                fTotal.Text = Fmt(summary.TotalPaid + summary.TotalUnpaid); // إجمالي الفواتير
                fPaid.Text = Fmt(summary.TotalPaid);
                fRev.Text = Fmt(summary.TotalRevenue);
                fUnpaid.Text = Fmt(summary.TotalUnpaid);

                finCard.IsVisible = true;
            }

            // Recent appointments
            if (appts.Any())
            {
                var items = appts.Take(6).Select(a => new ApptItem
                {
                    Initial = (a.CusName ?? "?").Substring(0, 1),
                    CusName = a.CusName ?? "عميل",
                    SubInfo = $"{a.SellCus ?? "–"} · #{a.InvoiceId}".Trim('·', ' '), // ✅ SellCus بدل BarberName
                    PayFmt = Fmt(a.Pay ?? 0),
                    Time = (a.SellDate ?? "").Length > 15 ? a.SellDate!.Substring(11, 5) : ""
                }).ToList();
                recentList.ItemsSource = items;
                recentSection.IsVisible = true;
            }

            statsGrid.IsVisible = true;
        }
        catch (Exception ex)
        {
            lblErr.Text = "⚠️ فشل التحميل: " + ex.Message;
            errFrame.IsVisible = true;
        }
        finally
        {
            loader.IsVisible = false;
            loader.IsRunning = false;
            btnRefresh.Opacity = 1.0; // أعد الزر لوضعه الطبيعي
            btnRefresh.InputTransparent = false;
        }
    }

    private static string Fmt(double n) => $"{Math.Round(n):N0} ج";
}

// Local item model for the list
public class ApptItem
{
    public string Initial { get; set; } = "";
    public string CusName { get; set; } = "";
    public string SubInfo { get; set; } = "";
    public string PayFmt { get; set; } = "";
    public string Time { get; set; } = "";
}
