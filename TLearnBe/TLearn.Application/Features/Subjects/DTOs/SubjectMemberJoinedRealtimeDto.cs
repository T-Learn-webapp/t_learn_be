using TLearn.Domain.Entities;

namespace TLearn.Application.Features.Subjects.DTOs;

public class SubjectMemberJoinedRealtimeDto

{

    public Guid SubjectId { get; set; }

    public string SubjectName { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public SubjectPermission Permission { get; set; }

    public Guid InvitedBy { get; set; }

    public DateTime JoinedAt { get; set; }

    public bool IsNewUser { get; set; }

}