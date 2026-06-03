namespace TLearn.Application.Features.MaterialVersions.DTOs;

public class MaterialVersionDetailDto
{
    public Guid Id { get; set; }

    public Guid MaterialId { get; set; }

    public long VersionNumber { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public string? Summary { get; set; }

    public string? YjsSnapshot { get; set; }

    public Guid EditedByUserId { get; set; }

    public string EditedByUserName { get; set; } = string.Empty;
    public string? ContributorsJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? ChangeNote { get; set; }
}