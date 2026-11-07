using System.Collections.ObjectModel;
using System.Globalization;
using AlMaystroApp.Models;
using AlMaystroApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlMaystroApp.ViewModels;

[QueryProperty(nameof(CategoryId), "categoryId")]
public partial class ServicesViewModel(ApiClient api) : ObservableObject
{
    [ObservableProperty] int categoryId;
    [ObservableProperty] string title = "";
    [ObservableProperty] bool isRefreshing;
    public ObservableCollection<ServiceItem> Items { get; } = new();

    [RelayCommand]
    public async Task Load()
    {
        IsRefreshing = true;
        try
        {
            var cat = (await api.GetCategoriesAsync()).FirstOrDefault(c => c.Id == CategoryId);
            Title = cat?.Name ?? "الخدمات";
            Items.Clear();
            foreach (var s in cat?.Services ?? new()) Items.Add(s);
        }
        catch { await Shell.Current.DisplayAlert("خطأ", "تعذر تحميل الخدمات", "حسناً"); }
        finally { IsRefreshing = false; }
    }

    static bool TryPrice(string? s, out decimal v) =>
        decimal.TryParse(s?.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out v) && v >= 0;

    // Ask for name, price (from), optional price (to). Returns null if cancelled/invalid.
    async Task<(string name, decimal from, decimal? to)?> Ask(ServiceItem? s)
    {
        var name = await Shell.Current.DisplayPromptAsync("الخدمة", "اسم الخدمة", initialValue: s?.Name ?? "");
        if (string.IsNullOrWhiteSpace(name)) return null;
        var f = await Shell.Current.DisplayPromptAsync("السعر", "السعر (أو السعر من)", keyboard: Keyboard.Numeric, initialValue: s?.PriceFrom.ToString("0.##", CultureInfo.InvariantCulture) ?? "");
        if (!TryPrice(f, out var from)) return null;
        var t = await Shell.Current.DisplayPromptAsync("السعر إلى", "اتركه فاضي لو السعر ثابت", keyboard: Keyboard.Numeric, initialValue: s?.PriceTo?.ToString("0.##", CultureInfo.InvariantCulture) ?? "");
        decimal? to = null;
        if (!string.IsNullOrWhiteSpace(t))
        {
            if (!TryPrice(t, out var tv) || tv < from) { await Shell.Current.DisplayAlert("خطأ", "سعر (إلى) لازم يكون أكبر من (من)", "حسناً"); return null; }
            to = tv;
        }
        return (name.Trim(), from, to);
    }

    [RelayCommand]
    async Task Add()
    {
        if (await Ask(null) is not { } r) return;
        await Run(() => api.AddServiceAsync(r.name, r.from, r.to, CategoryId));
    }

    [RelayCommand]
    async Task Edit(ServiceItem s)
    {
        if (await Ask(s) is not { } r) return;
        await Run(() => api.UpdateServiceAsync(s.Id, r.name, r.from, r.to, CategoryId));
    }

    [RelayCommand]
    async Task Delete(ServiceItem s)
    {
        if (!await Shell.Current.DisplayAlert("حذف", $"حذف \"{s.Name}\"؟", "حذف", "إلغاء")) return;
        await Run(() => api.DeleteServiceAsync(s.Id));
    }

    async Task Run(Func<Task> action)
    {
        try { await action(); await Load(); }
        catch { await Shell.Current.DisplayAlert("خطأ", "فشلت العملية", "حسناً"); }
    }
}
