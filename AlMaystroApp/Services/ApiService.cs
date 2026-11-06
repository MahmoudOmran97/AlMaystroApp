using System.Net.Http.Json;
using AlMaystroApp.Models;

namespace AlMaystroApp.Services;

public class ApiService
{
    private readonly HttpClient _client;
    private const string BASE = "https://almaystro.runasp.net/api";

    public ApiService()
    {
        _client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        _client.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<SummaryDto?> GetSummaryAsync()
    {
        try { return await _client.GetFromJsonAsync<SummaryDto>($"{BASE}/appointment/summary/today"); }
        catch { return null; }
    }

    // اليوم فقط
    public async Task<List<AppointmentDto>> GetAppointmentsAsync()
    {
        try
        {
            return await _client.GetFromJsonAsync<List<AppointmentDto>>($"{BASE}/appointment/today")
                   ?? new();
        }
        catch { return new(); }
    }

    // آخر 30 يوم
    public async Task<List<AppointmentDto>> GetAppointmentsRangeAsync(DateTime from, DateTime to)
    {
        try
        {
            var url = $"{BASE}/appointment/range?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
            return await _client.GetFromJsonAsync<List<AppointmentDto>>(url) ?? new();
        }
        catch { return new(); }
    }

    public async Task<List<AttendanceDto>> GetAttendanceAsync()
    {
        try
        {
            return await _client.GetFromJsonAsync<List<AttendanceDto>>($"{BASE}/attendance/today")
                   ?? new();
        }
        catch { return new(); }
    }

    // آخر 30 يوم حضور
    public async Task<List<AttendanceDto>> GetAttendanceRangeAsync(DateTime from, DateTime to)
    {
        try
        {
            var url = $"{BASE}/attendance/range?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
            return await _client.GetFromJsonAsync<List<AttendanceDto>>(url) ?? new();
        }
        catch { return new(); }
    }

    public async Task<List<BookingDto>> GetBookingsAsync()
    {
        try
        {
            return await _client.GetFromJsonAsync<List<BookingDto>>($"{BASE}/booking")
                   ?? new();
        }
        catch { return new(); }
    }

    // آخر 30 يوم حجوزات
    public async Task<List<BookingDto>> GetBookingsRangeAsync(DateTime from, DateTime to)
    {
        try
        {
            var url = $"{BASE}/booking/range?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
            return await _client.GetFromJsonAsync<List<BookingDto>>(url) ?? new();
        }
        catch { return new(); }
    }

    // Notifications
    public async Task<List<NotificationDto>> GetNotificationsAsync()
    {
        try
        {
            return await _client.GetFromJsonAsync<List<NotificationDto>>($"{BASE}/notification/all")
                   ?? new();
        }
        catch { return new(); }
    }

    public async Task<List<NotificationDto>> GetUnreadNotificationsAsync()
    {
        try
        {
            return await _client.GetFromJsonAsync<List<NotificationDto>>($"{BASE}/notification/unread")
                   ?? new();
        }
        catch { return new(); }
    }
}