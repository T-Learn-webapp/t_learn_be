using MediatR;
using TLearn.Common;

namespace TLearn.Application.Features.Subjects.Commands.InviteMember;

public class InviteMemberCommand : IRequest<Result<bool>>
{
    public Guid SubjectId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Permission { get; set; } = "ViewOnly";
    public Guid InvitedBy { get; set; }
}
