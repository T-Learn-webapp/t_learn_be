using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.Subjects.DTOs;
using TLearn.Common;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application.Features.Subjects.Queries.GetInvitationInfo;

public class GetInvitationInfoQueryHandler : IRequestHandler<GetInvitationInfoQuery, Result<InvitationInfoDto>>
{
    private readonly TLearnDbContext _context;
    private readonly ILogger<GetInvitationInfoQueryHandler> _logger;

    public GetInvitationInfoQueryHandler(TLearnDbContext context, ILogger<GetInvitationInfoQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<InvitationInfoDto>> Handle(GetInvitationInfoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var invitation = await _context.SubjectInvitations
                .Include(i => i.Subject)
                .Include(i => i.Inviter)
                .FirstOrDefaultAsync(i => i.InviteToken == request.Token && i.Status == InvitationStatus.Pending, cancellationToken);

            if (invitation == null)
                return Result<InvitationInfoDto>.Failure("Invalid or already used invitation.");

            if (invitation.ExpiresAt < DateTime.UtcNow)
                return Result<InvitationInfoDto>.Failure("This invitation has expired.");

            var isExistingUser = !string.IsNullOrEmpty(invitation.AcceptedUserId?.ToString());

            return Result<InvitationInfoDto>.Success(new InvitationInfoDto
            {
                Token = request.Token,
                SubjectName = invitation.Subject.Name,
                InviterName = invitation.Inviter?.FullName ?? invitation.Inviter?.Email ?? "Unknown",
                InviterEmail = invitation.Inviter?.Email ?? string.Empty,
                Permission = invitation.Permission.ToString(),
                ExpiresAt = invitation.ExpiresAt,
                IsExistingUser = isExistingUser
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invitation info");
            return Result<InvitationInfoDto>.Failure("An error occurred while retrieving invitation information.");
        }
    }
}