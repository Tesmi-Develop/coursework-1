namespace Coursework1.Data;

public readonly record struct Driver
{
    public DriverLicense License { get; init; }
    public FullName Name { get; init; }
    public VehicleCategory Categories { get; init; }
 
    public Driver(DriverLicense license, FullName name, VehicleCategory categories)
    {
        Name = name;
        License = license;
        Categories = categories;
    }
    
    public string CategoriesDisplay => Categories == VehicleCategory.None 
        ? "—" 
        : Categories.ToString().Replace(",", ", ");
}