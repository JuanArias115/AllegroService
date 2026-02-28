using AllegroService.Application.Common;
using AllegroService.Application.DTOs.Categories;
using AllegroService.Application.DTOs.Common;
using AllegroService.Application.Interfaces;
using AllegroService.Domain.Entities;
using AllegroService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AllegroService.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserContext _currentUser;

    public CategoryService(AppDbContext dbContext, ICurrentUserContext currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult<PagedResponse<CategoryDto>>> GetPagedAsync(ListQueryRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var query = _dbContext.ProductCategories.AsNoTracking().Where(x => x.GlampingId == glampingId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(term));
        }

        query = request.Sort?.ToLower() switch
        {
            "name_desc" => query.OrderByDescending(x => x.Name),
            _ => query.OrderBy(x => x.Name)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new CategoryDto(x.Id, x.Name))
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResponse<CategoryDto>>.Success(new PagedResponse<CategoryDto>
        {
            Items = items,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }

    public async Task<ServiceResult<CategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var category = await _dbContext.ProductCategories.AsNoTracking()
            .Where(x => x.GlampingId == glampingId && x.Id == id)
            .Select(x => new CategoryDto(x.Id, x.Name))
            .FirstOrDefaultAsync(cancellationToken);

        return category is null
            ? ServiceResult<CategoryDto>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Category not found."))
            : ServiceResult<CategoryDto>.Success(category);
    }

    public async Task<ServiceResult<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var name = request.Name.Trim();

        var exists = await _dbContext.ProductCategories.AnyAsync(
            x => x.GlampingId == glampingId && x.Name == name,
            cancellationToken);

        if (exists)
        {
            return ServiceResult<CategoryDto>.Failure(
                StatusCodes.Status409Conflict,
                new ServiceError("duplicate_name", "A category with this name already exists."));
        }

        var category = new ProductCategory
        {
            Id = Guid.NewGuid(),
            GlampingId = glampingId,
            Name = name
        };

        _dbContext.ProductCategories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<CategoryDto>.Success(new CategoryDto(category.Id, category.Name), StatusCodes.Status201Created);
    }

    public async Task<ServiceResult<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var category = await _dbContext.ProductCategories.FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == id, cancellationToken);

        if (category is null)
        {
            return ServiceResult<CategoryDto>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Category not found."));
        }

        var name = request.Name.Trim();
        var duplicate = await _dbContext.ProductCategories.AnyAsync(
            x => x.GlampingId == glampingId && x.Name == name && x.Id != id,
            cancellationToken);

        if (duplicate)
        {
            return ServiceResult<CategoryDto>.Failure(
                StatusCodes.Status409Conflict,
                new ServiceError("duplicate_name", "A category with this name already exists."));
        }

        category.Name = name;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<CategoryDto>.Success(new CategoryDto(category.Id, category.Name));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var category = await _dbContext.ProductCategories.FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == id, cancellationToken);

        if (category is null)
        {
            return ServiceResult<bool>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Category not found."));
        }

        var hasProducts = await _dbContext.Products.AnyAsync(x => x.GlampingId == glampingId && x.CategoryId == id, cancellationToken);
        if (hasProducts)
        {
            return ServiceResult<bool>.Failure(
                StatusCodes.Status409Conflict,
                new ServiceError("has_products", "Cannot delete a category with products."));
        }

        _dbContext.ProductCategories.Remove(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Success(true);
    }
}
