using Application.Abstractions.Messaging;
using Application.Coupons;
using Application.Coupons.Create;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Domain.Coupons;

namespace Cetus.Api.Endpoints.Coupons;

internal sealed class Create : IEndpoint
{
    private sealed record Request(
        string Code,
        string? Description,
        CouponDiscountType DiscountType,
        decimal DiscountValue,
        int? UsageLimit,
        DateTime? StartDate,
        DateTime? EndDate,
        IReadOnlyList<RuleRequest> Rules
    );

    private sealed record RuleRequest(CouponRuleType RuleType, string Value);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("coupons", async (
            Request request,
            ICommandHandler<CreateCouponCommand, CouponResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateCouponCommand(
                request.Code,
                request.Description,
                request.DiscountType,
                request.DiscountValue,
                request.UsageLimit,
                request.StartDate,
                request.EndDate,
                request.Rules.Select(r => new CreateCouponRuleCommand(r.RuleType, r.Value)).ToList()
            );

            var result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Coupons);
    }
}
