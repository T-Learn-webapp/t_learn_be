namespace TLearn.Application.Features.Materials.DTOs;

public class CreateMaterialDto
{
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? Summary { get; set; }
    public Guid SubjectId { get; set; }
}