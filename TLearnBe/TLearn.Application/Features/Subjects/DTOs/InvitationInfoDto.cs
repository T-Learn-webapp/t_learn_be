namespace TLearn.Application.Features.Subjects.DTOs;

public class InvitationInfoDto
{
    public string Token { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string InviterName { get; set; } = string.Empty;
    public string InviterEmail { get; set; } = string.Empty;
    public string Permission { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsExistingUser { get; set; }
}