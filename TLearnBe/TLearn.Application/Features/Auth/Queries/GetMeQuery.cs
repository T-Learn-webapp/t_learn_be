using MediatR;
using TLearn.Application.Features.Auth.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.Auth.Queries;

public class GetMeQuery : IRequest<Result<UserDto>>
{
    public Guid UserId { get; set; }
}