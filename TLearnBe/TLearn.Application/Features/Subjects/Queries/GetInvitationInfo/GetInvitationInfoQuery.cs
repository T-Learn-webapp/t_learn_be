using MediatR;
using TLearn.Application.Features.Subjects.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.Subjects.Queries.GetInvitationInfo;

public class GetInvitationInfoQuery : IRequest<Result<InvitationInfoDto>>
{
    public string Token { get; set; } = string.Empty;
}