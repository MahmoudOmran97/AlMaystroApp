using System.Collections.ObjectModel;
using AlMaystroApp.Models;
using AlMaystroApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlMaystroApp.ViewModels;

public partial class CategoriesViewModel(ApiClient api) : ObservableObject
{
    public ObservableCollection<CategoryItem> Items { get; } = new();
    [ObservableProperty] bool isRefreshing;

    [RelayCommand]
    public async Task Load()
    {
        IsRefreshing = true;
        try
        {
            var list = await api.GetCategoriesAsync();
            Items.Clear(); foreach (var c in list) Items.Add(c);
        }
        catch { await Shell.Current.DisplayAlert("خطأ", "تعذر تحميل الأقسام", "حسناً"); }
        finally { IsRefreshing = false; }
    }

    [RelayCommand]
    async Task Add()
    {
        var name = await Shell.Current.DisplayPromptAsync("قسم جديد", "اسم القسم");
        if (string.IsNullOrWhiteSpace(name)) return;
        await Run(() => api.AddCategoryAsync(name, Items.Count + 1));
    }

    [RelayCommand]
    async Task Edit(CategoryItem c)
    {
        var name = await Shell.Current.DisplayPromptAsync("تعديل القسم", "اسم القسم", initialValue: c.Name);
        if (string.IsNullOrWhiteSpace(name)) return;
        await Run(() => api.UpdateCategoryAsync(c.Id, name, c.SortOrder));
    }

    [RelayCommand]
    async Task Delete(CategoryItem c)
    {
        if (!await Shell.Current.DisplayAlert("حذف", $"حذف قسم \"{c.Name}\" وكل خدماته؟", "حذف", "إلغاء")) return;
        await Run(() => api.DeleteCategoryAsync(c.Id));
    }

    [RelayCommand]
    Task Open(CategoryItem c) =>
        Shell.Current.GoToAsync($"services?categoryId={c.Id}");

    [RelayCommand]
    async Task Logout()
    {
        api.Logout();
        try { await Shell.Current.GoToAsync(".."); } catch { }
    }

    async Task Run(Func<Task> action)
    {
        try { await action(); await Load(); }
        catch { await Shell.Current.DisplayAlert("خطأ", "فشلت العملية", "حسناً"); }
    }
}
