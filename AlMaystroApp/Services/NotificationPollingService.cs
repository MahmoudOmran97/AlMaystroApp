using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace AlMaystroApp.Services;

public class NotificationPollingService
{
    private readonly ApiService _api;
    private CancellationTokenSource? _cts;
    private int _lastKnownId = 0;

    public NotificationPollingService(ApiService api)
    {
        _api = api;
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();
        Task.Run(() => PollLoop(_cts.Token));
        System.Diagnostics.Debug.WriteLine("✅ Polling Started");
    }

    public void Stop() => _cts?.Cancel();

    private async Task PollLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await CheckForNewNotifications();
            }
            catch (Exception ex)
            {
                // ← هنا بيظهر الـ error الحقيقي
                System.Diagnostics.Debug.WriteLine($"❌ Poll Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            }
            await Task.Delay(TimeSpan.FromSeconds(30), token);
        }
    }

    private async Task CheckForNewNotifications()
    {
        System.Diagnostics.Debug.WriteLine("🔄 Checking notifications...");

        var data = await _api.GetNotificationsAsync();

        System.Diagnostics.Debug.WriteLine($"📦 Got {data?.Count()} notifications");

        if (data == null || !data.Any()) return;

        var latest = data.OrderByDescending(n => n.CreatedAt).First();
        var latestId = latest.Id;

        System.Diagnostics.Debug.WriteLine($"🆔 Latest ID: {latestId}, LastKnown: {_lastKnownId}");

        // اطلب permission أول مرة
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var granted = await LocalNotificationCenter.Current
                .RequestNotificationPermission();
            System.Diagnostics.Debug.WriteLine($"🔐 Permission granted: {granted}");
        });

        if (_lastKnownId == 0)
        {
            _lastKnownId = latestId;
            System.Diagnostics.Debug.WriteLine($"📝 First run — saved ID: {_lastKnownId}");
            return;
        }

        if (latestId > _lastKnownId)
        {
            _lastKnownId = latestId;
            System.Diagnostics.Debug.WriteLine($"🔔 New notification! Showing...");

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var request = new NotificationRequest
                {
                    NotificationId = latestId,
                    Title = "المايسترو 🔔",
                    Description = latest.Notfication1 ?? "إشعار جديد",
                    Android = new AndroidOptions
                    {
                        ChannelId = "maystro_channel",
                        Priority = AndroidPriority.High,
                    }
                };
                var result = await LocalNotificationCenter.Current.Show(request);
                System.Diagnostics.Debug.WriteLine($"📤 Show result: {result}");
            });
        }
    }
}