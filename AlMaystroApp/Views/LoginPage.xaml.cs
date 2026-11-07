using AlMaystroApp.ViewModels;
namespace AlMaystroApp.Views;
public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm) { InitializeComponent(); BindingContext = vm; }
}
