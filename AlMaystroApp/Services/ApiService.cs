using System.Net.Http.Json;
using AlMaystroApp.Models;

namespace AlMaystroApp.Services;

public class ApiService
{
    private readonly HttpClient _client;
    private const string BASE = "https://almaystro.runasp.net/api";
    private DateTime _lastAlert = DateTime.MinValue;

    /// <summary>آخر خطأ حصل في أي طلب (null لو آخر طلب نجح)</summary>
    public string? LastError { get; private set; }

    public ApiService()
    {
        _client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        _client.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    // ------------------------------------------------------------------
    // كل الطلبات بتعدّي من هنا. قبل كده أي خطأ كان بيتبلع (catch { return new(); })
    // فكان التطبيق يعرض "مفيش داتا" حتى لو السيرفر راجع 500 / 404 / مفيش نت.
    // دلوقتي: الخطأ بيتسجل في LastError + Debug output + رسالة للمستخدم.
    // ------------------------------------------------------------------
    private async Task<List<T>> GetListAsync<T>(string path, bool silent = false)
    {
        try
        {
            using var res = await _client.GetAsync($"{BASE}/{path}");
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                if (body.Length > 200) body = body[..200];
                throw new HttpRequestException($"HTTP {(int)res.StatusCode} {res.ReasonPhrase} {body}".Trim());
            }

            var data = await res.Content.ReadFromJsonAsync<List<T>>();
            LastError = null;
            return data ?? new();
        }
        catch (Exception ex)
        {
            Report(path, ex, silent);
            return new();
        }
    }

    private async Task<T?> GetObjectAsync<T>(string path, bool silent = false) where T : class
    {
        try
        {
            using var res = await _client.GetAsync($"{BASE}/{path}");
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                if (body.Length > 200) body = body[..200];
                throw new HttpRequestException($"HTTP {(int)res.StatusCode} {res.ReasonPhrase} {body}".Trim());
            }

            var data = await res.Content.ReadFromJsonAsync<T>();
            LastError = null;
            return data;
        }
        catch (Exception ex)
        {
            Report(path, ex, silent);
            return null;
        }
    }

    private void Report(string path, Exception ex, bool silent)
    {
        LastError = $"{path}\n{ex.GetType().Name}: {ex.Message}";
        System.Diagnostics.Debug.WriteLine($"[ApiService] {LastError}");

        if (silent) return;                                                // الطلبات الدورية (polling) ما تزعجش المستخدم
        if (DateTime.Now - _lastAlert < TimeSpan.FromSeconds(20)) return;  // رسالة واحدة كل 20 ثانية بحد أقصى
        _lastAlert = DateTime.Now;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                var page = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (page != null)
                    await page.DisplayAlert("تعذر تحميل البيانات", LastError, "حسناً");
            }
            catch { }
        });
    }

    // ------------------------------------------------------------------

    public Task<SummaryDto?> GetSummaryAsync() =>
        GetObjectAsync<SummaryDto>("appointment/summary/today");

    // اليوم فقط
    public Task<List<AppointmentDto>> GetAppointmentsAsync() =>
        GetListAsync<AppointmentDto>("appointment/today");

    // نطاق تواريخ
    public Task<List<AppointmentDto>> GetAppointmentsRangeAsync(DateTime from, DateTime to) =>
        GetListAsync<AppointmentDto>($"appointment/range?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");

    public Task<List<AttendanceDto>> GetAttendanceAsync() =>
        GetListAsync<AttendanceDto>("attendance/today");

    public Task<List<AttendanceDto>> GetAttendanceRangeAsync(DateTime from, DateTime to) =>
        GetListAsync<AttendanceDto>($"attendance/range?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");

    public Task<List<BookingDto>> GetBookingsAsync() =>
        GetListAsync<BookingDto>("booking");

    public Task<List<BookingDto>> GetBookingsRangeAsync(DateTime from, DateTime to) =>
        GetListAsync<BookingDto>($"booking/range?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");

    // Notifications
    public Task<List<NotificationDto>> GetNotificationsAsync() =>
        GetListAsync<NotificationDto>("notification/all");

    // بيتندهي من NotificationPollingService كل شوية => silent
    public Task<List<NotificationDto>> GetUnreadNotificationsAsync() =>
        GetListAsync<NotificationDto>("notification/unread", silent: true);
}
