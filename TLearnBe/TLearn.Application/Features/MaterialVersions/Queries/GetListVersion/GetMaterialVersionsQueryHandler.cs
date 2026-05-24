using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.MaterialVersions.DTOs;
using TLearn.Common;
using TLearn.Common.Pagination;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application.Features.MaterialVersions.Queries.GetListVersion;

public class GetMaterialVersionsQueryHandler
    : IRequestHandler<GetMaterialVersionsQuery, Result<PagedResult<MaterialVersionListDto>>>

{
    private readonly TLearnDbContext _context;

    private readonly ILogger<GetMaterialVersionsQueryHandler> _logger;

    public GetMaterialVersionsQueryHandler(
        TLearnDbContext context,
        ILogger<GetMaterialVersionsQueryHandler> logger)

    {
        _context = context;

        _logger = logger;
    }

    public async Task<Result<PagedResult<MaterialVersionListDto>>> Handle(
        GetMaterialVersionsQuery request,
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
                return Result<PagedResult<MaterialVersionListDto>>
                    .Failure("Tài liệu không tồn tại.");
            }

            if (!material.Subject.CanUserView(request.UserId))

            {
                return Result<PagedResult<MaterialVersionListDto>>
                    .Failure("Bạn không có quyền xem lịch sử tài liệu này.");
            }

            var query = _context.LearningMaterialVersions
                .AsNoTracking()
                .Where(x => x.LearningMaterialId == request.MaterialId);

            var totalCount = await query.CountAsync(cancellationToken);

            var versions = await query
                .OrderByDescending(x => x.VersionNumber)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new MaterialVersionListDto

                {
                    Id = x.Id,

                    MaterialId = x.LearningMaterialId,

                    VersionNumber = x.VersionNumber,

                    Title = x.Title,

                    Summary = x.Summary,

                    EditedByUserId = x.EditedByUserId,

                    EditedByUserName = x.EditedByUser.FullName,

                    CreatedAt = x.CreatedAt,

                    ChangeNote = x.ChangeNote
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<MaterialVersionListDto>(
                versions,
                totalCount,
                request.PageNumber,
                request.PageSize);

            return Result<PagedResult<MaterialVersionListDto>>.Success(result);
        }

        catch (Exception ex)

        {
            _logger.LogError(
                ex,
                "Lỗi khi lấy danh sách lịch sử tài liệu {MaterialId}",
                request.MaterialId);

            return Result<PagedResult<MaterialVersionListDto>>
                .Failure("Đã xảy ra lỗi khi lấy danh sách lịch sử tài liệu.");
        }
    }
}