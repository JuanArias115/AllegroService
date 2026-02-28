namespace AllegroService.Application.DTOs.Categories;

public sealed record CategoryDto(Guid Id, string Name);

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
}
