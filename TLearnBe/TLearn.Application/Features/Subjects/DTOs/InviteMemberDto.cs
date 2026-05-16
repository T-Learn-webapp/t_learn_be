namespace TLearn.Application.Features.Subjects.DTOs;

public class InviteMemberDto
{
    public string Email { get; set; } = string.Empty;
    public string Permission { get; set; } = "ViewOnly";
}