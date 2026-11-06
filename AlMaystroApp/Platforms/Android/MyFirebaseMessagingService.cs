using Android.App;
using Android.Content;
using Firebase.Messaging;
using Android.OS;

namespace AlMaystroApp.Platforms.Android;

[Service(Exported = false)]
[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
public class MyFirebaseMessagingService : FirebaseMessagingService
{
    public override void OnMessageReceived(RemoteMessage message)
    {
        base.OnMessageReceived(message);

        var title = message.GetNotification()?.Title ?? "المايسترو ✂️";
        var body = message.GetNotification()?.Body ?? "إشعار جديد";

        ShowNotification(title, body);
    }

    private void ShowNotification(string title, string body)
    {
        var manager = (NotificationManager?)GetSystemService(NotificationService);
        if (manager == null) return;

        const string channelId = "maystro_channel";

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(
                channelId, "إشعارات المايسترو", NotificationImportance.High);
            manager.CreateNotificationChannel(channel);
        }

        var intent = new Intent(this, typeof(MainActivity));
        intent.AddFlags(ActivityFlags.ClearTop);
        var pendingIntent = PendingIntent.GetActivity(
            this, 0, intent,
            PendingIntentFlags.OneShot | PendingIntentFlags.Immutable);

        // --- FIX STARTS HERE ---
        // This looks up the icon ID by its filename "appicon2" at runtime
        int iconId = Resources.GetIdentifier("appicon2", "mipmap", PackageName);

        // If the icon isn't found (returns 0), use a system default so it doesn't crash
        if (iconId == 0) iconId = global::Android.Resource.Drawable.SymDefAppIcon;
        // --- FIX ENDS HERE ---

        var notif = new global::Android.App.Notification.Builder(this, channelId)
            .SetContentTitle(title)
            .SetContentText(body)
            .SetSmallIcon(iconId) // Use the integer ID we found above
            .SetAutoCancel(true)
            .SetContentIntent(pendingIntent)
            .SetPriority((int)NotificationPriority.High)
            .Build();

        manager.Notify(new Random().Next(), notif);
    }
    // لما Firebase يجدد الـ Token
    public override void OnNewToken(string token)
    {
        base.OnNewToken(token);
        System.Diagnostics.Debug.WriteLine($"🔑 FCM Token: {token}");
        // ابعت الـ token للـ API عشان يحفظه
    }
}