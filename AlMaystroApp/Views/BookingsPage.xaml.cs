using AlMaystroApp.ViewModels;
namespace AlMaystroApp.Views;
public partial class BookingsPage : ContentPage
{
    readonly BookingsViewModel _vm;
    public BookingsPage(BookingsViewModel vm) { InitializeComponent(); BindingContext = _vm = vm; }
    protected override async void OnAppearing() { base.OnAppearing(); await _vm.Load(); }
}
