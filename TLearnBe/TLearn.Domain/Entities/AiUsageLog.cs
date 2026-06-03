namespace TLearn.Domain.Entities;

public class AiUsageLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public string Feature { get; set; } = string.Empty;

    public DateTime UsedAt { get; set; } = DateTime.UtcNow;
}