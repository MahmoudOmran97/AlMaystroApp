using Android.App;
using Android.Content.PM;
using Android.OS;
using Firebase; // تأكد من إضافة هذا الـ namespace

namespace AlMaystroApp
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // 1. تهيئة Firebase يدوياً لضمان عدم حدوث Crash
            try
            {
                FirebaseApp.InitializeApp(this);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Firebase Init Error: {ex.Message}");
            }

            // 2. إنشاء قناة الإشعارات (Notification Channel) 
            // ضروري جداً لأندرويد 8.0 فما فوق لتظهر الإشعارات
            CreateNotificationChannel();
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelId = "maystro_notifications"; // يجب أن يطابق المعرف في الخدمة
                var channelName = "إشعارات المايسترو";
                var channelDescription = "قناة مخصصة لإشعارات تطبيق المايسترو";
                var importance = NotificationImportance.High;

                var channel = new NotificationChannel(channelId, channelName, importance)
                {
                    Description = channelDescription
                };

                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.CreateNotificationChannel(channel);
            }
        }
    }
}