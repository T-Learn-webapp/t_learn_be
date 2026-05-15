namespace TLearn.Application.Features.Materials.DTOs;

public class UpdateMaterialDto
{
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? Summary { get; set; }
}