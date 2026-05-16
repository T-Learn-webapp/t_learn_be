using MediatR;
using TLearn.Common;

namespace TLearn.Application.Features.Subjects.Commands.RemoveMember;

public class RemoveMemberCommand : IRequest<Result<bool>>
{
    public Guid SubjectId { get; set; }
    public Guid MemberId { get; set; }
    public Guid CurrentUserId { get; set; }
}