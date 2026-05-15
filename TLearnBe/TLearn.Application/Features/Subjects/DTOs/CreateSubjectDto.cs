namespace TLearn.Application.Features.Subjects.DTOs;

public class CreateSubjectDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public bool IsPublic { get; set; } = true;
}