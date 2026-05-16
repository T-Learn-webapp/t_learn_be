using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Common;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application.Features.Subjects.Commands.AcceptInvitation;

public class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, Result<AcceptInvitationResult>>
{
    private readonly TLearnDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AcceptInvitationCommandHandler> _logger;

    public AcceptInvitationCommandHandler(
        TLearnDbContext context,
        UserManager<User> userManager,
        ILogger<AcceptInvitationCommandHandler> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<AcceptInvitationResult>> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            
            var invitation = await _context.SubjectInvitations
                .Include(i => i.Subject)
                .FirstOrDefaultAsync(i => i.InviteToken == request.Token && i.Status == InvitationStatus.Pending, cancellationToken);

            if (invitation == null)
                return Result<AcceptInvitationResult>.Failure("Invalid or already used invitation.");

            if (invitation.ExpiresAt < DateTime.UtcNow)
            {
                invitation.Status = InvitationStatus.Expired;
                await _context.SaveChangesAsync(cancellationToken);
                return Result<AcceptInvitationResult>.Failure("This invitation has expired.");
            }

            // Check if user is logged in
            User? user = null;
            bool isNewUser = false;

            if (request.UserId.HasValue && request.UserId != Guid.Empty)
            {
                // User is logged in
                user = await _userManager.FindByIdAsync(request.UserId.Value.ToString());
                if (user == null)
                    return Result<AcceptInvitationResult>.Failure("User not found.");
                
                if (user.Email?.ToLower() != invitation.Email.ToLower())
                    return Result<AcceptInvitationResult>.Failure("This invitation was sent to a different email address.");
            }
            else if (!string.IsNullOrEmpty(request.RegisterData?.Password))
            {
                
                user = new User
                {
                    UserName = invitation.Email,
                    Email = invitation.Email,
                    FullName = request.RegisterData.FullName,
                    EmailConfirmed= true, 
                    SubscriptionType = "Free",
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user, request.RegisterData.Password);
                if (!createResult.Succeeded)
                    return Result<AcceptInvitationResult>.Failure(string.Join(", ", createResult.Errors.Select(e => e.Description)));
                
                isNewUser = true;
            }
            else
            {
                return Result<AcceptInvitationResult>.Failure("Please login or register to accept this invitation.");
            }

            // Check if already a member
            var existingMember = await _context.SubjectMembers
                .AnyAsync(m => m.SubjectId == invitation.SubjectId && m.UserId == user.Id, cancellationToken);

            if (existingMember)
                return Result<AcceptInvitationResult>.Failure("You are already a member of this subject.");


            try
            {
                // Add member
                var member = new SubjectMember
                {
                    SubjectId = invitation.SubjectId,
                    UserId = user.Id,
                    Permission = invitation.Permission,
                    InvitedBy = invitation.InvitedBy,
                    JoinedAt = DateTime.UtcNow
                };

                _context.SubjectMembers.Add(member);

                // Update invitation
                invitation.IsUsed = true;
                invitation.UsedAt = DateTime.UtcNow;
                invitation.Status = InvitationStatus.Accepted;
                invitation.AcceptedUserId = user.Id;

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result<AcceptInvitationResult>.Success(new AcceptInvitationResult
                {
                    SubjectId = invitation.SubjectId,
                    SubjectName = invitation.Subject.Name,
                    IsNewUser = isNewUser,
                    UserId = user.Id,
                    Email = user.Email ?? invitation.Email,
                    FullName = user.FullName
                });
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Database error while accepting invitation");
                return Result<AcceptInvitationResult>.Failure("Failed to accept invitation. Please try again.");
            }
        }
        catch (Exception ex)
        {
                await transaction.RollbackAsync(cancellationToken);
            
            _logger.LogError(ex, "Unexpected error while accepting invitation");
            return Result<AcceptInvitationResult>.Failure("An unexpected error occurred.");
        }
    }
}