using AlMaystroApp.Views;

namespace AlMaystroApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        // The splash page opens AppShell after its animation (see SplashPage.snippet.txt)
        MainPage = new SplashPage();
    }
}