namespace Sum_gRPC.Models;
public class OutboxMessage
{
    public int Id { get; set; }
    public string Content { get; set; } // JSON payload
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedOn { get; set; }
}
