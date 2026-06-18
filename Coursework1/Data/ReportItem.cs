namespace Coursework1.Data;

public record struct ReportItem
{
    public DriverLicense License { get; set; }
    public FullName FullName { get; set; }
    public VehicleCategory Categories { get; set; }
    public string Article { get; set; }
    public uint Price { get; init; }
    public FormattedDate Date { get; set; }
}