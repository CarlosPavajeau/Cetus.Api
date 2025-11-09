using Application.Abstractions.Messaging;
using Application.Products.Variants;
using Application.Products.Variants.Find;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Products.Variants;

internal sealed class Find : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("products/variants/{id:long}", async (
            long id,
            IQueryHandler<FindProductVariantQuery, ProductVariantResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new FindProductVariantQuery(id);
            var result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        });
    }
}
