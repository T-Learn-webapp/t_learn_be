namespace TLearn.Application.Features.MaterialVersions.DTOs;

public class MaterialVersionListDto
{
    public Guid Id { get; set; }

    public Guid MaterialId { get; set; }

    public long VersionNumber { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public Guid EditedByUserId { get; set; }

    public string EditedByUserName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string? ChangeNote { get; set; }
}