using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.SearchForSale;

internal sealed class SearchAllProductsForSaleQueryHandler(IApplicationDbContext context, ITenantContext tenant)
    : IQueryHandler<SearchAllProductsForSaleQuery, PagedResult<SimpleProductForSaleResponse>>
{
    public async Task<Result<PagedResult<SimpleProductForSaleResponse>>> Handle(SearchAllProductsForSaleQuery query,
        CancellationToken cancellationToken)
    {
        int page = query.Page <= 0 ? 1 : query.Page;
        int size = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 100);

        var productsQuery = context.ProductVariants
            .AsNoTracking()
            .Include(p => p.Product)
            .Where(p => p.DeletedAt == null
                        && p.Enabled
                        && p.Product!.Enabled
                        && p.Product!.DeletedAt == null
                        && p.Product!.StoreId == tenant.Id
            );

        if (query.CategoryIds != null && query.CategoryIds.Any())
        {
            productsQuery = productsQuery.Where(p => query.CategoryIds.Contains(p.Product!.CategoryId));
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            productsQuery = productsQuery.Where(p =>
                p.Product!.SearchVector!.Matches(EF.Functions.PlainToTsQuery("spanish", query.SearchTerm)));
        }

        int total = await productsQuery.CountAsync(cancellationToken);

        var products = await productsQuery
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(SimpleProductForSaleResponse.Map)
            .ToListAsync(cancellationToken);

        var response = products
            .DistinctBy(p => p.Id)
            .ToList();

        var payload = PagedResult<SimpleProductForSaleResponse>
            .Create(response, page, total, size);

        return payload;
    }
}
