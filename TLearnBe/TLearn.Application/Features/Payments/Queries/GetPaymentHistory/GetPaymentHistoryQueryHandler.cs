using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.Payments.DTOs;
using TLearn.Common;
using TLearn.Common.Pagination;
using TLearn.Domain.Constants;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application.Features.Payments.Queries.GetPaymentHistory;

public class GetPaymentHistoryQueryHandler
    : IRequestHandler<GetPaymentHistoryQuery, Result<PagedResult<PaymentHistoryDto>>>

{
    private readonly TLearnDbContext _context;

    private readonly ILogger<GetPaymentHistoryQueryHandler> _logger;

    public GetPaymentHistoryQueryHandler(
        TLearnDbContext context,
        ILogger<GetPaymentHistoryQueryHandler> logger)

    {
        _context = context;

        _logger = logger;
    }

    public async Task<Result<PagedResult<PaymentHistoryDto>>> Handle(
        GetPaymentHistoryQuery request,
        CancellationToken cancellationToken)

    {
        try

        {
            if (request.UserId == Guid.Empty)

            {
                return Result<PagedResult<PaymentHistoryDto>>
                    .Failure("UserId không hợp lệ.");
            }

            var pageNumber = request.PageNumber <= 0
                ? 1
                : request.PageNumber;

            var pageSize = request.PageSize <= 0
                ? 10
                : request.PageSize;

            pageSize = Math.Min(pageSize, 50);

            var query = _context.Payments
                .AsNoTracking()
                .Where(x => x.UserId == request.UserId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Status))

            {
                var status = request.Status.Trim().ToUpper();

                query = query.Where(x => x.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(request.PlanType))

            {
                var planType = request.PlanType.Trim();

                query = query.Where(x => x.PlanType == planType);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))

            {
                var keyword = request.Search.Trim();

                query = query.Where(x =>
                    x.PayOSOrderCode.Contains(keyword) ||
                    x.PlanName.Contains(keyword) ||
                    (x.Description != null &&
                     x.Description.Contains(keyword)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PaymentHistoryDto

                {
                    Id = x.Id,

                    OrderCode = x.PayOSOrderCode,

                    Amount = x.Amount,

                    Currency = x.Currency,

                    PlanType = x.PlanType,

                    PlanName = x.PlanName,

                    Status = x.Status,

                    PaymentMethod = x.PaymentMethod,

                    Description = x.Description,

                    PayOSPaymentLinkId = x.PayOSPaymentLinkId,

                    ExpiresAt = x.ExpiresAt,

                    PaidAt = x.PaidAt,

                    CancelledAt = x.CancelledAt,

                    ExpiredAt = x.ExpiredAt,

                    CreatedAt = x.CreatedAt,

                    UpdatedAt = x.UpdatedAt,

                    IsPaid = x.Status == PaymentStatuses.Paid,

                    IsPending = x.Status == PaymentStatuses.Pending,

                    IsCancelled = x.Status == PaymentStatuses.Cancelled,

                    IsExpired = x.Status == PaymentStatuses.Expired
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<PaymentHistoryDto>(
                items,
                totalCount,
                pageNumber,
                pageSize);

            return Result<PagedResult<PaymentHistoryDto>>.Success(result);
        }

        catch (Exception ex)

        {
            _logger.LogError(
                ex,
                "Lỗi khi lấy lịch sử thanh toán của user {UserId}",
                request.UserId);

            return Result<PagedResult<PaymentHistoryDto>>
                .Failure("Đã xảy ra lỗi khi lấy lịch sử thanh toán.");
        }
    }
}