namespace Coursework1.Data;

public record struct FineWithId
{
    public int Id;
    public Fine Fine { get; set; }
}
