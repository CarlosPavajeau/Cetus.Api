using Application.Abstractions.Messaging;
using Application.Products.SearchAll;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Products;

internal sealed class SearchAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("products", async (
            IQueryHandler<SearchAllProductsQuery, IEnumerable<ProductResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchAllProductsQuery();

            var result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Products).HasPermission(ClerkPermissions.AppAccess);
    }
}
