using Application.Abstractions.Messaging;
using Application.Reviews.ProductReviews.SearchAll;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Reviews.ProductReviews;

internal sealed class SearchAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("reviews/products/{id:guid}", async (
            Guid id,
            IQueryHandler<SearchAllProductReviewsQuery, IEnumerable<ProductReviewResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchAllProductReviewsQuery(id);
            var result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Reviews).AllowAnonymous();
    }
}
