using MediatR;
using TLearn.Application.Features.Admin.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.Admin.Queries.GetAdminRevenue;

public class GetAdminRevenueQuery : IRequest<Result<AdminRevenueDto>>

{
    public AdminRevenueRangeType RangeType { get; set; } = AdminRevenueRangeType.Daily;

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }
}