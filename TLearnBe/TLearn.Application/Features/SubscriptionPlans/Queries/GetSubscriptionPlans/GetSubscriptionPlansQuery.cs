using MediatR;
using TLearn.Application.Features.SubscriptionPlans.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.SubscriptionPlans.Queries.GetSubscriptionPlans;

public class GetSubscriptionPlansQuery
    : IRequest<Result<List<SubscriptionPlanDto>>>
{
    public bool OnlyActive { get; set; } = true;
}