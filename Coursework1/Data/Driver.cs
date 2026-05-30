namespace Coursework1.Data;

public readonly record struct Driver
{
    public DriverLicense License { get; init; }
    public FullName FullName { get; init; }
    public VehicleCategory Categories { get; init; }
}