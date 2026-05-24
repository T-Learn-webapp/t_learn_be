using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Common;
using TLearn.Domain.Exceptions;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.Materials.Commands.DeleteMaterial;

public class DeleteLearningMaterialHandler
    : IRequestHandler<DeleteLearningMaterialCommand, Result<bool>>
{
    private readonly TLearnDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteLearningMaterialHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(
        DeleteLearningMaterialCommand request,
        CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId;

        if (!currentUserId.HasValue)
        {
            return Result<bool>.Failure("Chưa đăng nhập");
        }

        var material = await _context.LearningMaterials
            .Include(x => x.Subject)
            .FirstOrDefaultAsync(
                x => x.Id == request.MaterialId,
                ct);

        if (material == null || material.IsDeleted)
        {
            return Result<bool>.Failure("Tài liệu không tồn tại");
        }

        if (!material.Subject.CanUserEdit(currentUserId.Value))
        {
            return Result<bool>.Failure("Bạn không có quyền xoá tài liệu này");
        }

        material.IsDeleted = true;
        material.DeletedAt = DateTime.UtcNow;
        material.DeletedByUserId = currentUserId.Value;
        material.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}