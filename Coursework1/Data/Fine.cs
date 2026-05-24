namespace Coursework1.Data;

public struct Fine
{
    public DriverLicense License { get; init; }
    public string Article { get; set; }
    public int Price { get; init; }
    public DateTime Date { get; set; }
    
    public Fine(DriverLicense license, string article, int price, DateTime date)
    {
        License = license;
        Article = article;
        Price = price;
        Date = date;
    }
}