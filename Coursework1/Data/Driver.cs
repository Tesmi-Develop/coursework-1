namespace Coursework1.Data;

public readonly record struct Driver
{
    public DriverLicense License { get; init; }
    public FullName Name { get; init; }
    public VehicleCategory Categories { get; init; }
    
    public string CategoriesDisplay => Categories == VehicleCategory.None 
        ? "—" 
        : Categories.ToString().Replace(",", ", ");
}