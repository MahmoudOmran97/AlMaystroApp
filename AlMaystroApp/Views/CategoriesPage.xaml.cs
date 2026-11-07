using AlMaystroApp.ViewModels;
namespace AlMaystroApp.Views;
public partial class CategoriesPage : ContentPage
{
    readonly CategoriesViewModel _vm;
    public CategoriesPage(CategoriesViewModel vm) { InitializeComponent(); BindingContext = _vm = vm; }
    protected override async void OnAppearing() { base.OnAppearing(); await _vm.Load(); }
}
