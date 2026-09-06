namespace UKAccounts.Application.DTOs;

public class AgedPayablesReport
{
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime AsOfDate { get; set; }
    public List<AgedPayableLine> Lines { get; set; } = new();
    public decimal TotalOutstanding => Lines.Sum(l => l.Total);
    public decimal Current => Lines.Sum(l => l.Current);
    public decimal Days1to30 => Lines.Sum(l => l.Days1to30);
    public decimal Days31to60 => Lines.Sum(l => l.Days31to60);
    public decimal Days61to90 => Lines.Sum(l => l.Days61to90);
    public decimal Over90 => Lines.Sum(l => l.Over90);
}

public class AgedPayableLine
{
    public Guid BillId { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public DateTime BillDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Total => Current + Days1to30 + Days31to60 + Days61to90 + Over90;
    public decimal Current { get; set; }
    public decimal Days1to30 { get; set; }
    public decimal Days31to60 { get; set; }
    public decimal Days61to90 { get; set; }
    public decimal Over90 { get; set; }
}
