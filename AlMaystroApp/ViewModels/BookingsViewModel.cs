using System.Collections.ObjectModel;
using AlMaystroApp.Models;
using AlMaystroApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlMaystroApp.ViewModels;

public partial class BookingsViewModel(ApiClient api) : ObservableObject
{
    public ObservableCollection<BookingItem> Items { get; } = new();
    [ObservableProperty] bool isRefreshing;
    [ObservableProperty] DateTime day = DateTime.Today;
    [ObservableProperty] bool allDays;          // false = selected day only
    [ObservableProperty] string statusFilter = ""; // "" = all

    partial void OnDayChanged(DateTime value) => _ = Load();
    partial void OnAllDaysChanged(bool value) => _ = Load();

    [RelayCommand]
    public async Task Load()
    {
        IsRefreshing = true;
        try
        {
            var list = AllDays
                ? await api.GetBookingsAsync(DateTime.Today, null, StatusFilter)
                : await api.GetBookingsAsync(Day, Day, StatusFilter);
            Items.Clear(); foreach (var b in list) Items.Add(b);
        }
        catch { /* 401 is handled by ApiClient.Unauthorized */ }
        finally { IsRefreshing = false; }
    }

    [RelayCommand]
    async Task Filter()
    {
        var pick = await Shell.Current.DisplayActionSheet("فلترة الحالة", "إلغاء", null,
            "الكل", "في الانتظار", "مؤكد", "ملغي");
        if (pick is null or "إلغاء") return;
        StatusFilter = pick switch { "في الانتظار" => "Pending", "مؤكد" => "Confirmed", "ملغي" => "Cancelled", _ => "" };
        await Load();
    }

    [RelayCommand]
    async Task Actions(BookingItem b)
    {
        var pick = await Shell.Current.DisplayActionSheet($"{b.CustomerName} - {b.Phone}", "إغلاق", null,
            "تأكيد", "إلغاء الحجز", "تم", "لم يحضر", "اتصال");
        string? status = pick switch { "تأكيد" => "Confirmed", "إلغاء الحجز" => "Cancelled", "تم" => "Completed", "لم يحضر" => "NoShow", _ => null };
        if (pick == "اتصال") { try { PhoneDialer.Default.Open(b.Phone); } catch { } return; }
        if (status is null) return;
        try { await api.SetBookingStatusAsync(b.Id, status); await Load(); }
        catch { await Shell.Current.DisplayAlert("خطأ", "فشل تحديث الحالة", "حسناً"); }
    }
}
