using Android.App;
using Android.Content;
using Android.OS;
using AlMaystroApp.Services;

namespace AlMaystroApp.Platforms.Android;

[Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeDataSync)]
public class NotificationForegroundService : Service
{
    private CancellationTokenSource? _cts;
    private readonly HashSet<int> _shown = new();            // don't re-notify the same notification every poll
    private const int ServiceNotifId = 1001;
    private const string BgChannelId = "maystro_bg_channel";      // persistent "running" notification
    private const string AlertChannelId = "maystro_channel";      // real alerts (same id as manifest + MainActivity)

    public override IBinder? OnBind(Intent? intent) => null;

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        CreateChannels();
        StartForeground(ServiceNotifId, BuildPersistentNotif());

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        Task.Run(() => PollLoop(token));

        return StartCommandResult.Sticky;
    }

    public override void OnDestroy()
    {
        _cts?.Cancel();
        base.OnDestroy();
    }

    // ── Polling ──────────────────────────────────────
    private async Task PollLoop(CancellationToken token)
    {
        try
        {
            await Task.Delay(5000, token);      // let the app finish starting
            while (!token.IsCancellationRequested)
            {
                try { await CheckUnread(); }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"❌ BG Poll: {ex.Message}"); }
                await Task.Delay(TimeSpan.FromSeconds(30), token);
            }
        }
        catch (TaskCanceledException) { /* service stopped */ }
    }

    private async Task CheckUnread()
    {
        var api = IPlatformApplication.Current?.Services.GetService<ApiService>();
        if (api == null)
        {
            System.Diagnostics.Debug.WriteLine("❌ ApiService is not registered in MauiProgram");
            return;
        }

        var unread = await api.GetUnreadNotificationsAsync();
        System.Diagnostics.Debug.WriteLine($"📬 BG Unread: {unread.Count}");

        foreach (var notif in unread)
        {
            if (!_shown.Add(notif.Id)) continue;
            ShowLocalNotif(notif.Id, notif.Notfication1 ?? "إشعار جديد");
        }
    }

    // ── Real alert ───────────────────────────────────
    private void ShowLocalNotif(int id, string message)
    {
        var manager = (NotificationManager?)GetSystemService(NotificationService);
        if (manager == null) return;

        var notif = new global::Android.App.Notification.Builder(this, AlertChannelId)
            .SetContentTitle("✂️ المايسترو")
            .SetContentText(message)
            .SetSmallIcon(GetSmallIcon())
            .SetAutoCancel(true)
            .SetPriority((int)NotificationPriority.High)
            .Build();

        manager.Notify(id, notif);
    }

    // ── Persistent notification required for a foreground service ──
    private global::Android.App.Notification BuildPersistentNotif()
    {
        var intent = new Intent(this, typeof(MainActivity));
        intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);

        var pendingIntent = PendingIntent.GetActivity(this, 0, intent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        return new global::Android.App.Notification.Builder(this, BgChannelId)
            .SetContentTitle("المايسترو")
            .SetContentText("التطبيق يعمل في الخلفية لمراقبة الطلبات")
            .SetSmallIcon(GetSmallIcon())
            .SetContentIntent(pendingIntent)
            .SetAutoCancel(false)
            .SetOngoing(true)
            .Build();
    }

    private int GetSmallIcon()
    {
        int id = Resources?.GetIdentifier("appicon2", "mipmap", PackageName) ?? 0;
        return id != 0 ? id : global::Android.Resource.Drawable.IcDialogInfo;   // never 0, or Build() throws
    }

    private void CreateChannels()
    {
        var manager = (NotificationManager?)GetSystemService(NotificationService);
        if (manager == null) return;

        manager.CreateNotificationChannel(new NotificationChannel(
            BgChannelId, "المايسترو خلفية", NotificationImportance.Low));
        manager.CreateNotificationChannel(new NotificationChannel(
            AlertChannelId, "إشعارات المايسترو", NotificationImportance.High));
    }
}