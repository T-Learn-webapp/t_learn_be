using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TLearn.Common;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.Subjects.Commands.InviteMember;

public class InviteMemberCommandHandler : IRequestHandler<InviteMemberCommand, Result<bool>>
{
    private readonly TLearnDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;
    private readonly ILogger<InviteMemberCommandHandler> _logger;

    public InviteMemberCommandHandler(
        TLearnDbContext context,
        UserManager<User> userManager,
        IEmailService emailService,
        IConfiguration config,
        ILogger<InviteMemberCommandHandler> logger)
    {
        _context = context;
        _userManager = userManager;
        _emailService = emailService;
        _config = config;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(InviteMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var subject = await _context.Subjects
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == request.SubjectId, cancellationToken);

           
            
            if (subject == null)
                return Result<bool>.Failure($"Subject with id '{request.SubjectId}' was not found.");
            
           
            // Check permission
            if (subject.UserId != request.InvitedBy && !subject.CanUserManage(request.InvitedBy))
                return Result<bool>.Failure("You don't have permission to invite members to this subject.");

            // Parse permission
            if (!Enum.TryParse<SubjectPermission>(request.Permission, true, out var permission))
                return Result<bool>.Failure($"Invalid permission. Allowed: ViewOnly, Comment, Edit, Manage");

            var invitedEmail = request.Email.ToLower();
            
            
            var existingMember = await _context.SubjectMembers
                .AnyAsync(m => m.SubjectId == request.SubjectId && m.User.Email == invitedEmail, cancellationToken);
            
            if (existingMember)
                return Result<bool>.Failure($"{invitedEmail} is already a member of this subject.");

            
            var existingInvitation = await _context.SubjectInvitations
                .FirstOrDefaultAsync(i => i.SubjectId == request.SubjectId && 
                                          i.Email == invitedEmail && 
                                          i.Status == InvitationStatus.Pending, cancellationToken);
            
            if (existingInvitation != null)
                return Result<bool>.Failure($"An invitation has already been sent to {invitedEmail}. Please wait for them to respond.");

            
            var inviteToken = Guid.NewGuid().ToString();
            var expiryDays = _config.GetValue<int>("SubjectInvitation:ExpiryDays", 7);
            
            
            var existingUser = await _userManager.FindByEmailAsync(invitedEmail);
            var isExistingUser = existingUser != null;

            var invitation = new SubjectInvitation
            {
                SubjectId = request.SubjectId,
                Email = invitedEmail,
                Permission = permission,
                InviteToken = inviteToken,
                InvitedBy = request.InvitedBy,
                ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
                Status = InvitationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                _context.SubjectInvitations.Add(invitation);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                
                var frontendUrl = _config["FrontendUrl"] ?? "http://localhost:3000";
                var acceptUrl = $"{frontendUrl}/accept-invitation?token={inviteToken}";
                
                if (isExistingUser)
                {
                    
                    await _emailService.SendSubjectInvitationEmail(
                        invitedEmail, 
                        subject.Name, 
                        acceptUrl, 
                        permission.ToString(),
                        existingUser.FullName);
                }
                else
                {
                    
                    var registerUrl = $"{frontendUrl}/register?email={Uri.EscapeDataString(invitedEmail)}&inviteToken={inviteToken}";
                    await _emailService.SendSubjectRegistrationInvitationEmail(
                        invitedEmail,
                        subject.Name,
                        registerUrl,
                        permission.ToString(),
                        subject.User?.FullName ?? "Subject owner");
                }

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error while inviting member");
                return Result<bool>.Failure("Failed to send invitation. Please try again.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while inviting member");
            return Result<bool>.Failure("An unexpected error occurred while sending invitation.");
        }
    }
}