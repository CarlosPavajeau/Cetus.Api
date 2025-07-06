using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Coupons.SearchAllRules;

internal sealed class SearchAllCouponRulesQueryHandler(IApplicationDbContext context)
    : IQueryHandler<SearchAllCouponRulesQuery, IEnumerable<CouponRuleResponse>>
{
    public async Task<Result<IEnumerable<CouponRuleResponse>>> Handle(SearchAllCouponRulesQuery query,
        CancellationToken cancellationToken)
    {
        var rules = await context.CouponRules
            .Where(r => r.CouponId == query.Id)
            .Select(CouponRuleResponse.Map)
            .ToListAsync(cancellationToken);

        return rules;
    }
}
