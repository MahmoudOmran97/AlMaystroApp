using AlMaystroApp.Services;

namespace AlMaystroApp
{
    public partial class App : Application
    {
        public App(NotificationPollingService pollingService)
        {
            InitializeComponent();
            MainPage = new SplashPage();

            // استخدام Task.Run مع معالجة استثنائية لضمان عدم انهيار التطبيق
            Task.Run(async () => {
                try
                {
                    // زيادة مدة التأخير لضمان انتهاء MainActivity من InitializeApp
                    await Task.Delay(3000);

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        StartBackgroundService();
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Initial Start Error: {ex.Message}");
                }
            });
        }

        private void StartBackgroundService()
        {
#if ANDROID
            try
            {
                // 1. تشغيل خدمة الـ Foreground أولاً
                var intent = new Android.Content.Intent(
                    Android.App.Application.Context,
                    typeof(AlMaystroApp.Platforms.Android.NotificationForegroundService));

                if (OperatingSystem.IsAndroidVersionAtLeast(26))
                    Android.App.Application.Context.StartForegroundService(intent);
                else
                    Android.App.Application.Context.StartService(intent);

                // 2. الاشتراك في Firebase داخل بلوك try-catch منفصل
                // لضمان أنه حتى لو فشل Firebase، لا تتوقف الخدمة الأساسية
                try 
                {
                    Firebase.FirebaseApp.InitializeApp(Android.App.Application.Context); // تأكيد إضافي للتهيئة
                    Firebase.Messaging.FirebaseMessaging.Instance.SubscribeToTopic("maystro_all");
                    System.Diagnostics.Debug.WriteLine("✅ Subscribed to Firebase Topic");
                }
                catch (Exception fbEx)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Firebase Subscription Error: {fbEx.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Service Logic Error: {ex.Message}");
            }
#endif
        }
    }
}