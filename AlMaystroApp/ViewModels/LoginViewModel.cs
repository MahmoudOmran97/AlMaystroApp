using AlMaystroApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlMaystroApp.ViewModels;

public partial class LoginViewModel(ApiClient api) : ObservableObject
{
    [ObservableProperty] string username = "";
    [ObservableProperty] string password = "";
    [ObservableProperty] string error = "";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(NotBusy))] bool isBusy;
    public bool NotBusy => !IsBusy;

    [RelayCommand]
    async Task Login()
    {
        if (IsBusy) return;
        IsBusy = true; Error = "";
        try
        {
            if (await api.LoginAsync(Username.Trim(), Password)) await Shell.Current.GoToAsync("//main/categories");
            else Error = "بيانات الدخول غير صحيحة";
        }
        catch { Error = "تعذر الاتصال بالسيرفر"; }
        finally { IsBusy = false; }
    }
}
