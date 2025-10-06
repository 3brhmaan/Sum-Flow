namespace Sum_REST.Models;

public class AccumlatorRequest
{
    public int Value { get; set; }
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    public DateTime ProcessedOn { get; set; }
}