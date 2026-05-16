using MediatR;
using TLearn.Common;

namespace TLearn.Application.Features.Subjects.Commands.UpdateMemberPermission;

public class UpdateMemberPermissionCommand : IRequest<Result<bool>>
{
    public Guid SubjectId { get; set; }
    public Guid MemberId { get; set; }
    public string Permission { get; set; } = string.Empty;
    public Guid CurrentUserId { get; set; }
}