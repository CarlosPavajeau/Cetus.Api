using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Orders.DeliveryFees.Create;
using Application.Orders.DeliveryFees.Find;
using Application.Orders.DeliveryFees.SearchAll;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;
using DeliveryFeeResponse = Application.Orders.DeliveryFees.Find.DeliveryFeeResponse;

namespace Cetus.Api.Endpoints.Orders;

internal sealed class DeliveryFees : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("orders/delivery-fees", async (
            CreateDeliveryFeeCommand command,
            ICommandHandler<CreateDeliveryFeeCommand, DeliveryFeeResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Orders).HasPermission(ClerkPermissions.AppAccess);

        app.MapGet("orders/delivery-fees", async (
            IQueryHandler<SearchAllDeliveryFeesQuery,
                IEnumerable<Application.Orders.DeliveryFees.SearchAll.DeliveryFeeResponse>> handler,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchAllDeliveryFeesQuery();

            var result = await cache.GetOrCreateAsync(
                "delivery-fees",
                async token => await handler.Handle(query, token),
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Orders).HasPermission(ClerkPermissions.AppAccess);

        app.MapGet("orders/delivery-fees/{cityId:guid}", async (
            Guid cityId,
            IQueryHandler<FindDeliveryFeeQuery, DeliveryFeeResponse> handler,
            HybridCache cache,
            ITenantContext tenant,
            CancellationToken cancellationToken) =>
        {
            var query = new FindDeliveryFeeQuery(cityId);

            var result = await cache.GetOrCreateAsync(
                $"delivery-fee-{cityId}-${tenant.Id}",
                async token => await handler.Handle(query, token),
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.Orders);
    }
}
