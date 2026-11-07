namespace AlMaystroApp.Models;

public record LoginResult(string Token, DateTime ExpiresAt);
public record ServiceItem(int Id, string Name, decimal PriceFrom, decimal? PriceTo, int CategoryId)
{
    public string PriceText => PriceTo is null ? $"{PriceFrom:0.##} ج" : $"{PriceFrom:0.##} - {PriceTo:0.##} ج";
}
public record CategoryItem(int Id, string Name, int SortOrder, List<ServiceItem> Services)
{
    public string CountText => $"{Services.Count} خدمة";
}
public record BookingItem(int Id, string CustomerName, string Phone, int? ServiceId, string? ServiceName,
    DateTime Date, string Status, string Source, string? Notes)
{
    public string When => Date.ToString("ddd d MMM - hh:mm tt");
    public string StatusAr => Status switch
    {
        "Pending" => "في الانتظار", "Confirmed" => "مؤكد", "Cancelled" => "ملغي",
        "Completed" => "تم", "NoShow" => "لم يحضر", _ => Status
    };
    public string SourceAr => Source switch { "Web" => "الموقع", "Desktop" => "الكاشير", "Mobile" => "الموبايل", _ => Source };
}
