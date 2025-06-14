using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Coupons.SearchAll;

internal sealed class SearchAllCouponsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<SearchAllCouponsQuery, IEnumerable<CouponResponse>>
{
    public async Task<Result<IEnumerable<CouponResponse>>> Handle(SearchAllCouponsQuery query,
        CancellationToken cancellationToken)
    {
        var coupons = await context.Coupons
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        var response = coupons.Select(CouponResponse.FromCoupon).ToList();
        return response;
    }
}
