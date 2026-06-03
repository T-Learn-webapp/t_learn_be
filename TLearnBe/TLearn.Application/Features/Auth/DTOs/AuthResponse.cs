namespace TLearn.Application.Features.Auth.DTOs;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiry { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string SubscriptionType { get; set; } = "Free";

    public bool EmailConfirmed { get; set; }

    public Guid? CurrentSubscriptionId { get; set; }

    public string? CurrentPlanType { get; set; }

    public string? CurrentPlanName { get; set; }

    public DateTime? SubscriptionStartDate { get; set; }

    public DateTime? SubscriptionEndDate { get; set; }

    public bool HasActiveSubscription { get; set; }

    public int? RemainingVipDays { get; set; }

    public int AiUsageLimit { get; set; }

    public int AiUsedCount { get; set; }

    public int AiRemainingCount { get; set; }

    public string AiUsageResetType { get; set; } = string.Empty;

    public DateTime? AiUsageResetAt { get; set; }
}