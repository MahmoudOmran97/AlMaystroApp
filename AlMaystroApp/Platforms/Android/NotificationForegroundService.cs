using Android.App;
using Android.Content;
using Android.OS;
using AlMaystroApp.Services;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace AlMaystroApp.Platforms.Android;

[Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeDataSync)]
public class NotificationForegroundService : Service
{
    private CancellationTokenSource? _cts;
    private const int ServiceNotifId = 1001;
    private const string ChannelId = "maystro_bg_channel";

    public override IBinder? OnBind(Intent? intent) => null;

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        CreateChannel();
        StartForeground(ServiceNotifId, BuildPersistentNotif());

        _cts = new CancellationTokenSource();
        Task.Run(() => PollLoop(_cts.Token));

        return StartCommandResult.Sticky; // ← يرجع تلقائي لو اتقتل
    }

    public override void OnDestroy()
    {
        _cts?.Cancel();
        base.OnDestroy();
    }

    // ── Polling ──────────────────────────────────────
    private async Task PollLoop(CancellationToken token)
    {
        // انتظر أول مرة عشان التطبيق يتحمل
        await Task.Delay(5000, token);

        while (!token.IsCancellationRequested)
        {
            try { await CheckUnread(); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ BG Poll: {ex.Message}");
            }
            await Task.Delay(TimeSpan.FromSeconds(30), token);
        }
    }

    private async Task CheckUnread()
    {
        // جيب الـ ApiService من الـ DI
        var api = IPlatformApplication.Current?.Services
                      .GetService<ApiService>();
        if (api == null) return;

        var unread = await api.GetUnreadNotificationsAsync();
        System.Diagnostics.Debug.WriteLine($"📬 BG Unread: {unread.Count}");

        if (!unread.Any()) return;

        foreach (var notif in unread)
        {
            ShowLocalNotif(notif.Id, notif.Notfication1 ?? "إشعار جديد");
        }
    }

    // ── إشعار محلي ──────────────────────────────────
    private void ShowLocalNotif(int id, string message)
    {
        var manager = (NotificationManager?)GetSystemService(NotificationService);
        if (manager == null) return;

        // Channel للإشعارات الحقيقية
        var channel = new NotificationChannel("maystro_channel", "إشعارات المايسترو",
            NotificationImportance.High);
        manager.CreateNotificationChannel(channel);

        var notif = new global::Android.App.Notification.Builder(this, "maystro_channel")
            .SetContentTitle("✂️ المايسترو")
            .SetContentText(message)
            .SetSmallIcon(this.Resources.GetIdentifier("abc_btn_check_material", "drawable", this.PackageName))// غيّر للأيكون بتاعك
            .SetAutoCancel(true)
            .SetPriority((int)NotificationPriority.High)
            .Build();

        manager.Notify(id, notif);
    }

    // ── الإشعار الدائم (عشان الـ Foreground Service) ──
    private global::Android.App.Notification BuildPersistentNotif()
    {
        var intent = new Intent(this, typeof(MainActivity));
        intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);

        var pendingIntent = PendingIntent.GetActivity(this, 0, intent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        // ملاحظة: تأكد أن ChannelId هنا يطابق المعرف في CreateChannel() وهو "maystro_bg_channel"
        var channelId = "maystro_bg_channel";

        return new global::Android.App.Notification.Builder(this, channelId)
            .SetContentTitle("المايسترو")
            .SetContentText("التطبيق يعمل في الخلفية لمراقبة الطلبات")
            // استخدام أيقونة معلومات النظام العامة (متوفرة دائماً)
            .SetSmallIcon(global::Android.Resource.Drawable.IcDialogInfo)
            .SetContentIntent(pendingIntent)
            .SetAutoCancel(false) // الخدمة الدائمة يفضل أن تكون false
            .SetOngoing(true)
            .Build();
    }
    private void CreateChannel()
    {
        var manager = (NotificationManager?)GetSystemService(NotificationService);
        var channel = new NotificationChannel(ChannelId, "المايسترو خلفية",
            NotificationImportance.Low); // Low عشان متزعجش المستخدم
        manager?.CreateNotificationChannel(channel);
    }
}