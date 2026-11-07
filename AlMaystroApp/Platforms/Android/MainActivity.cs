using Android.App;
using Android.Content.PM;
using Android.OS;
using Firebase;

namespace AlMaystroApp
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            try { FirebaseApp.InitializeApp(this); }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Firebase Init Error: {ex.Message}");
            }

            CreateNotificationChannel();
            RequestNotificationPermission();
        }

        // Android 13+ needs the permission at runtime, otherwise no notification is ever shown
        private void RequestNotificationPermission()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu &&
                CheckSelfPermission(global::Android.Manifest.Permission.PostNotifications) != Permission.Granted)
            {
                RequestPermissions(new[] { global::Android.Manifest.Permission.PostNotifications }, 1001);
            }
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;

            // same id as AndroidManifest meta-data + the services: "maystro_channel"
            var channel = new NotificationChannel("maystro_channel", "إشعارات المايسترو", NotificationImportance.High)
            {
                Description = "قناة مخصصة لإشعارات تطبيق المايسترو"
            };
            var manager = (NotificationManager?)GetSystemService(NotificationService);
            manager?.CreateNotificationChannel(channel);
        }
    }
}