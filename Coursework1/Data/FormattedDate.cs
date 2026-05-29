namespace Coursework1.Data;

public readonly record struct FormattedDate(DateTime Value)
{
    public override string ToString() => Value.ToString("dd MMM yyyy");
    
    public static implicit operator FormattedDate(DateTime dt) => new(dt);
    public static implicit operator DateTime(FormattedDate dt) => dt.Value;
}