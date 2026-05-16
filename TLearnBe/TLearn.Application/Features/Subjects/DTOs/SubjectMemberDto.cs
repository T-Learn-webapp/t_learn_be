namespace TLearn.Application.Features.Subjects.DTOs;

public class SubjectMemberDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string Permission { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}