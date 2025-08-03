namespace Application.Products;

public sealed record ProductImageResponse(long Id, string ImageUrl, string? AltText, int SortOrder);
