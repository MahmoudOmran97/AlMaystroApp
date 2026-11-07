using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AlMaystroApp.Models;

namespace AlMaystroApp.Services;

public class ApiClient
{
    // Android emulator => http://10.0.2.2:PORT ; real device/production => your server HTTPS URL
    const string BaseUrl = "https://almaystro.runasp.net";
    const string TokenKey = "jwt";
    readonly HttpClient _http = new() { BaseAddress = new Uri(BaseUrl), Timeout = TimeSpan.FromSeconds(20) };
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public event Action? Unauthorized;
    public bool IsLoggedIn { get; private set; }

    public async Task InitAsync()
    {
        var t = await SecureStorage.Default.GetAsync(TokenKey);
        if (!string.IsNullOrEmpty(t)) { SetToken(t); IsLoggedIn = true; }
    }

    void SetToken(string? t) =>
        _http.DefaultRequestHeaders.Authorization = t is null ? null : new AuthenticationHeaderValue("Bearer", t);

    public async Task<bool> LoginAsync(string user, string pass)
    {
        var res = await _http.PostAsJsonAsync("api/auth/login", new { username = user, password = pass });
        if (!res.IsSuccessStatusCode) return false;
        var r = await res.Content.ReadFromJsonAsync<LoginResult>(Json);
        await SecureStorage.Default.SetAsync(TokenKey, r!.Token);
        SetToken(r.Token); IsLoggedIn = true;
        return true;
    }

    public void Logout() { SecureStorage.Default.Remove(TokenKey); SetToken(null); IsLoggedIn = false; }

    async Task<HttpResponseMessage> Send(Func<Task<HttpResponseMessage>> call)
    {
        var res = await call();
        if (res.StatusCode == HttpStatusCode.Unauthorized) { Logout(); Unauthorized?.Invoke(); }
        res.EnsureSuccessStatusCode();
        return res;
    }

    // Categories & services
    public async Task<List<CategoryItem>> GetCategoriesAsync() =>
        await _http.GetFromJsonAsync<List<CategoryItem>>("api/categories", Json) ?? new();
    public Task AddCategoryAsync(string name, int order) => Send(() => _http.PostAsJsonAsync("api/categories", new { name, sortOrder = order }));
    public Task UpdateCategoryAsync(int id, string name, int order) => Send(() => _http.PutAsJsonAsync($"api/categories/{id}", new { name, sortOrder = order }));
    public Task DeleteCategoryAsync(int id) => Send(() => _http.DeleteAsync($"api/categories/{id}"));

    public Task AddServiceAsync(string name, decimal from, decimal? to, int categoryId) =>
        Send(() => _http.PostAsJsonAsync("api/services", new { name, priceFrom = from, priceTo = to, categoryId }));
    public Task UpdateServiceAsync(int id, string name, decimal from, decimal? to, int categoryId) =>
        Send(() => _http.PutAsJsonAsync($"api/services/{id}", new { name, priceFrom = from, priceTo = to, categoryId }));
    public Task DeleteServiceAsync(int id) => Send(() => _http.DeleteAsync($"api/services/{id}"));

    // Bookings
    public async Task<List<BookingItem>> GetBookingsAsync(DateTime? from = null, DateTime? to = null, string? status = null)
    {
        var q = new List<string>();
        if (from is not null) q.Add($"from={from:yyyy-MM-dd}");
        if (to is not null) q.Add($"to={to:yyyy-MM-dd}");
        if (!string.IsNullOrEmpty(status)) q.Add($"status={status}");
        var url = "api/bookings" + (q.Count > 0 ? "?" + string.Join("&", q) : "");
        var res = await Send(() => _http.GetAsync(url));
        return await res.Content.ReadFromJsonAsync<List<BookingItem>>(Json) ?? new();
    }
    public Task SetBookingStatusAsync(int id, string status) =>
        Send(() => _http.PutAsJsonAsync($"api/bookings/{id}/status", new { status }));
}
