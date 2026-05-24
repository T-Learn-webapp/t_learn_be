namespace TLearn.Application.Features.Materials.DTOs;

public class UpdateMaterialInfoRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
}