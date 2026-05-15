namespace TLearn.Application.Features.Subjects.DTOs;

public class SubjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public bool IsPublic { get; set; }
    public int MaterialCount { get; set; }
    public DateTime CreatedAt { get; set; }
}