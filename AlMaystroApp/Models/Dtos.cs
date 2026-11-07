namespace AlMaystroApp.Models;

public class SummaryDto
{
    public int TotalInvoices { get; set; }
    public double TotalPaid { get; set; }
    public double TotalUnpaid { get; set; }
    public double TotalRevenue { get; set; }
}

public class AppointmentDto
{
    public int? Id { get; set; }
    public int? InvoiceId { get; set; }
    public string? CusName { get; set; }
    public string? SellCus { get; set; }
    public string? NameCasher { get; set; }
    public double? Pay { get; set; }
    public double? TotalPrice { get; set; }
    public string? AppStatus { get; set; }
    public string? SellDate { get; set; }
    public double? SellTprice { get; set; }
    public double? Stay { get; set; }
    public string? Safe { get; set; }
}

public class AttendanceDto
{
    public int? EmployeeId { get; set; }
    public string? NameEmployee { get; set; }
    public string? AttendDate { get; set; }
    public string? CheckIn { get; set; }
    public string? CheckOut { get; set; }
    public string? Status { get; set; }
    public decimal WorkedHours { get; set; }
    public decimal OvertimeHours { get; set; }
}

public class BookingDto
{
    public int? Id { get; set; }
    public string? CusName { get; set; }
    public string? CusPhone { get; set; }
    public string? BookingDate { get; set; }
    public string? Status { get; set; }       // Pending / Confirmed / Cancelled / Completed / NoShow
    public string? Source { get; set; }       // Web / Desktop / Mobile
    public string? ServiceName { get; set; }
    public string? Notes { get; set; }
}

public class NotificationDto
{
    public int Id { get; set; }
    public string? Notfication1 { get; set; }
    public DateTime? CreatedAt { get; set; }
    public bool IsRead { get; set; }
}