namespace Coursework1.Data;

public readonly record struct DriverLicense
{
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