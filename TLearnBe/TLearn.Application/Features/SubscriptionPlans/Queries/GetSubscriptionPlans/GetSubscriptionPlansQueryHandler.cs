using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.SubscriptionPlans.DTOs;
using TLearn.Common;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application.Features.SubscriptionPlans.Queries.GetSubscriptionPlans;

public class GetSubscriptionPlansQueryHandler
    : IRequestHandler<GetSubscriptionPlansQuery, Result<List<SubscriptionPlanDto>>>

{
    private readonly TLearnDbContext _context;

    private readonly ILogger<GetSubscriptionPlansQueryHandler> _logger;

    public GetSubscriptionPlansQueryHandler(
        TLearnDbContext context,
        ILogger<GetSubscriptionPlansQueryHandler> logger)

    {
        _context = context;

        _logger = logger;
    }

    public async Task<Result<List<SubscriptionPlanDto>>> Handle(
        GetSubscriptionPlansQuery request,
        CancellationToken cancellationToken)

    {
        try

        {
            var query = _context.SubscriptionPlans
                .AsNoTracking()
                .AsQueryable();

            if (request.OnlyActive)

            {
                query = query.Where(x => x.IsActive);
            }

            var plans = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Amount)
                .Select(x => new SubscriptionPlanDto

                {
                    Id = x.Id,

                    PlanType = x.PlanType,

                    PlanName = x.PlanName,

                    Description = x.Description,

                    Amount = x.Amount,

                    Currency = x.Currency,

                    DurationDays = x.DurationDays,

                    IsActive = x.IsActive,

                    SortOrder = x.SortOrder
                })
                .ToListAsync(cancellationToken);

            return Result<List<SubscriptionPlanDto>>.Success(plans);
        }

        catch (Exception ex)

        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách gói nâng cấp");

            return Result<List<SubscriptionPlanDto>>.Failure(
                "Đã xảy ra lỗi khi lấy danh sách gói nâng cấp.");
        }
    }
}