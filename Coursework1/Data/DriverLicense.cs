namespace Coursework1.Data;

public readonly record struct DriverLicense
{
    public static readonly DriverLicense Invalid = new(-1, -1);
    
    public int Series { get; init; }
    public int Number { get; init; }

    public DriverLicense(int series, int number)
    {
        Series = series;
        Number = number;
    }

    public override string ToString()
    {
        return $"{Series}-{Number}";
    }
}