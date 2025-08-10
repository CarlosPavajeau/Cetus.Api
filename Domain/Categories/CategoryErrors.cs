using SharedKernel;

namespace Domain.Categories;

public static class CategoryErrors
{
    public static Error NotFound(string slug) =>
        Error.NotFound(
            "Categories.NotFound",
            $"Category with slug '{slug}' was not found."
        );
}
