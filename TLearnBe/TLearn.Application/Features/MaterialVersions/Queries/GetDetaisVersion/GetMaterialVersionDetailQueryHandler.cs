using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.MaterialVersions.DTOs;
using TLearn.Common;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application.Features.MaterialVersions.Queries.GetDetaisVersion;

public class GetMaterialVersionDetailQueryHandler
    : IRequestHandler<GetMaterialVersionDetailQuery, Result<MaterialVersionDetailDto>>
{
    private readonly TLearnDbContext _context;
    private readonly ILogger<GetMaterialVersionDetailQueryHandler> _logger;

    public GetMaterialVersionDetailQueryHandler(
        TLearnDbContext context,
        ILogger<GetMaterialVersionDetailQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<MaterialVersionDetailDto>> Handle(
        GetMaterialVersionDetailQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var material = await _context.LearningMaterials
                .AsNoTracking()
                .Include(x => x.Subject)
                .ThenInclude(x => x.Members)
                .FirstOrDefaultAsync(
                    x => x.Id == request.MaterialId &&
                         !x.IsDeleted,
                    cancellationToken);

            if (material == null)
            {
                return Result<MaterialVersionDetailDto>
                    .Failure("Tài liệu không tồn tại.");
            }

            if (!material.Subject.CanUserView(request.UserId))
            {
                return Result<MaterialVersionDetailDto>
                    .Failure("Bạn không có quyền xem phiên bản này.");
            }

            var version = await _context.LearningMaterialVersions
                .AsNoTracking()
                .Where(x =>
                    x.Id == request.VersionId &&
                    x.LearningMaterialId == request.MaterialId)
                .Select(x => new MaterialVersionDetailDto
                {
                    Id = x.Id,
                    MaterialId = x.LearningMaterialId,
                    VersionNumber = x.VersionNumber,
                    Title = x.Title,
                    Content = x.Content,
                    Summary = x.Summary,
                    YjsSnapshot = x.YjsSnapshot,
                    EditedByUserId = x.EditedByUserId,
                    EditedByUserName = x.EditedByUser.FullName,
                    ContributorsJson = x.ContributorsJson,
                    CreatedAt = x.CreatedAt,
                    ChangeNote = x.ChangeNote
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (version == null)
            {
                return Result<MaterialVersionDetailDto>
                    .Failure("Phiên bản không tồn tại.");
            }

            return Result<MaterialVersionDetailDto>.Success(version);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Lỗi khi lấy chi tiết phiên bản {VersionId} của tài liệu {MaterialId}",
                request.VersionId,
                request.MaterialId);

            return Result<MaterialVersionDetailDto>
                .Failure("Đã xảy ra lỗi khi lấy chi tiết phiên bản.");
        }
    }
}