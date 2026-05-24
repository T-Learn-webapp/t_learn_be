using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.Subjects.DTOs;
using TLearn.Common;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Hubs;

namespace TLearn.Application.Features.Subjects.Commands.AcceptInvitation;

public class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, Result<AcceptInvitationResult>>
{
    private readonly TLearnDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AcceptInvitationCommandHandler> _logger;
    private readonly IHubContext<SubjectHub> _hubContext;

    public AcceptInvitationCommandHandler(
        TLearnDbContext context,
        UserManager<User> userManager,
        ILogger<AcceptInvitationCommandHandler> logger,
        IHubContext<SubjectHub> hubContext)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task<Result<AcceptInvitationResult>> Handle(AcceptInvitationCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var invitation = await _context.SubjectInvitations
                .Include(i => i.Subject)
                .ThenInclude(s => s.Members)
                .FirstOrDefaultAsync(i => i.InviteToken == request.Token && i.Status == InvitationStatus.Pending,
                    cancellationToken);

            if (invitation == null)
                return Result<AcceptInvitationResult>.Failure("Lời mời không hợp lệ hoặc đã được sử dụng.");

            if (invitation.ExpiresAt < DateTime.UtcNow)
            {
                invitation.Status = InvitationStatus.Expired;
                await _context.SaveChangesAsync(cancellationToken);
                return Result<AcceptInvitationResult>.Failure("Lời mời này đã hết hạn.");
            }

            // Check if user is logged in
            User? user = null;
            bool isNewUser = false;

            if (request.UserId.HasValue && request.UserId != Guid.Empty)
            {
                // User is logged in
                user = await _userManager.FindByIdAsync(request.UserId.Value.ToString());
                if (user == null)
                    return Result<AcceptInvitationResult>.Failure("Không tìm thấy người dùng.");

                if (user.Email?.ToLower() != invitation.Email.ToLower())
                    return Result<AcceptInvitationResult>.Failure(
                        "Lời mời này được gửi tới một địa chỉ email khác.");
            }
            else if (!string.IsNullOrEmpty(request.RegisterData?.Password))
            {
                user = new User
                {
                    UserName = invitation.Email,
                    Email = invitation.Email,
                    FullName = request.RegisterData.FullName,
                    EmailConfirmed = true,
                    SubscriptionType = "Free",
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user, request.RegisterData.Password);
                if (!createResult.Succeeded)
                    return Result<AcceptInvitationResult>.Failure(string.Join(", ",
                        createResult.Errors.Select(e => e.Description)));

                isNewUser = true;
            }
            else
            {
                return Result<AcceptInvitationResult>.Failure("Vui lòng đăng nhập hoặc đăng ký để chấp nhận lời mời.");
            }

            // Kiểm tra thành viên hiện có, bao gồm cả thành viên đã bị xoá mềm
            var existingMember = await _context.SubjectMembers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    m => m.SubjectId == invitation.SubjectId &&
                         m.UserId == user.Id,
                    cancellationToken);

            if (existingMember != null && !existingMember.IsDeleted)
                return Result<AcceptInvitationResult>.Failure("Bạn đã là thành viên của môn học này.");

            try
            {
                SubjectMember member;

                if (existingMember != null && existingMember.IsDeleted)
                {
                    // Khôi phục thành viên từng bị xoá khỏi subject
                    existingMember.IsDeleted = false;
                    existingMember.DeletedAt = null;
                    existingMember.DeletedByUserId = null;
                    existingMember.Permission = invitation.Permission;
                    existingMember.InvitedBy = invitation.InvitedBy;
                    existingMember.JoinedAt = DateTime.UtcNow;

                    member = existingMember;

                    var deletedAssignments = await _context.TodoAssignments
                        .Where(a =>
                            a.UserId == user.Id &&
                            a.TodoItem.LearningMaterial.SubjectId == invitation.SubjectId &&
                            a.IsDeleted)
                        .ToListAsync(cancellationToken);

                    foreach (var assignment in deletedAssignments)

                    {
                        assignment.IsDeleted = false;

                        assignment.DeletedAt = null;

                        assignment.DeletedByUserId = null;
                    }
                }
                else
                {
                    // Thêm thành viên mới
                    member = new SubjectMember
                    {
                        SubjectId = invitation.SubjectId,
                        UserId = user.Id,
                        Permission = invitation.Permission,
                        InvitedBy = invitation.InvitedBy,
                        JoinedAt = DateTime.UtcNow
                    };

                    _context.SubjectMembers.Add(member);
                }

                // Update invitation
                invitation.IsUsed = true;
                invitation.UsedAt = DateTime.UtcNow;
                invitation.Status = InvitationStatus.Accepted;
                invitation.AcceptedUserId = user.Id;

                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                var result = new AcceptInvitationResult

                {
                    SubjectId = invitation.SubjectId,

                    SubjectName = invitation.Subject.Name,

                    IsNewUser = isNewUser,

                    UserId = user.Id,

                    Email = user.Email ?? invitation.Email,

                    FullName = user.FullName
                };

                var realtimeDto = new SubjectMemberJoinedRealtimeDto

                {
                    SubjectId = invitation.SubjectId,

                    SubjectName = invitation.Subject.Name,

                    UserId = user.Id,

                    Email = user.Email ?? invitation.Email,

                    FullName = user.FullName,

                    Permission = invitation.Permission,

                    InvitedBy = invitation.InvitedBy,

                    JoinedAt = member.JoinedAt,

                    IsNewUser = isNewUser
                };

                var subjectUserIds = invitation.Subject.Members
                    .Select(m => m.UserId)
                    .ToHashSet();

                subjectUserIds.Add(invitation.Subject.UserId);

                subjectUserIds.Add(user.Id);

                foreach (var userId in subjectUserIds)
                {
                    await _hubContext.Clients
                        .Group($"user-{userId}")
                        .SendAsync("SubjectMemberJoined", realtimeDto, cancellationToken);
                }

                return Result<AcceptInvitationResult>.Success(result);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Database error while accepting invitation");
                return Result<AcceptInvitationResult>.Failure("Không thể chấp nhận lời mời. Vui lòng thử lại.");
            }
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            _logger.LogError(ex, "Unexpected error while accepting invitation");
            return Result<AcceptInvitationResult>.Failure("Đã xảy ra lỗi không xác định.");
        }
    }
}