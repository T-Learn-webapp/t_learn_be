using MediatR;
using TLearn.Common;

namespace TLearn.Application.Features.Subjects.Commands.AcceptInvitation;

public class AcceptInvitationCommand : IRequest<Result<AcceptInvitationResult>>
{
    public string Token { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public RegisterData? RegisterData { get; set; }
}

public class RegisterData
{
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public class AcceptInvitationResult
{
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public bool IsNewUser { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
}