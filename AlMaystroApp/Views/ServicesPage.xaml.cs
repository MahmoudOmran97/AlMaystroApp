using AlMaystroApp.ViewModels;
namespace AlMaystroApp.Views;
public partial class ServicesPage : ContentPage
{
    readonly ServicesViewModel _vm;
    public ServicesPage(ServicesViewModel vm) { InitializeComponent(); BindingContext = _vm = vm; }
    protected override async void OnAppearing() { base.OnAppearing(); await _vm.Load(); }
}
