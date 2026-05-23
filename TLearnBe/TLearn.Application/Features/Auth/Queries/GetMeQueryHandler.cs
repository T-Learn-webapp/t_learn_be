using MediatR;
using Microsoft.AspNetCore.Identity;
using TLearn.Application.Features.Auth.DTOs;
using TLearn.Common;
using TLearn.Domain.Entities;

namespace TLearn.Application.Features.Auth.Queries;

public class GetMeQueryHandler : IRequestHandler<GetMeQuery, Result<UserDto>>
{
    private readonly UserManager<User> _userManager;

    public GetMeQueryHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<UserDto>> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        
        if (user == null)
            return Result<UserDto>.Failure("User not found");
        
        return Result<UserDto>.Success(new UserDto
        {
            Id = user.Id,
            FullName = user.FullName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            SubscriptionType = user.SubscriptionType ?? "Free",
            EmailConfirmed = user.EmailConfirmed
        });
    }
}