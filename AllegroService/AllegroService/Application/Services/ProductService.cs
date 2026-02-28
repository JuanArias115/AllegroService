using AllegroService.Application.Common;
using AllegroService.Application.DTOs.Common;
using AllegroService.Application.DTOs.Products;
using AllegroService.Application.Interfaces;
using AllegroService.Domain.Entities;
using AllegroService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AllegroService.Application.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserContext _currentUser;

    public ProductService(AppDbContext dbContext, ICurrentUserContext currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult<PagedResponse<ProductDto>>> GetPagedAsync(ListQueryRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var query = _dbContext.Products.AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.GlampingId == glampingId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(term)
                || x.Sku.ToLower().Contains(term)
                || x.Category.Name.ToLower().Contains(term));
        }

        query = request.Sort?.ToLower() switch
        {
            "sku" => query.OrderBy(x => x.Sku),
            "sku_desc" => query.OrderByDescending(x => x.Sku),
            "price" => query.OrderBy(x => x.SalePrice),
            "price_desc" => query.OrderByDescending(x => x.SalePrice),
            _ => query.OrderBy(x => x.Name)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => MapDto(x))
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResponse<ProductDto>>.Success(new PagedResponse<ProductDto>
        {
            Items = items,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }

    public async Task<ServiceResult<ProductDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var product = await _dbContext.Products.AsNoTracking()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == id, cancellationToken);

        return product is null
            ? ServiceResult<ProductDto>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Product not found."))
            : ServiceResult<ProductDto>.Success(MapDto(product));
    }

    public async Task<ServiceResult<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();

        var category = await _dbContext.ProductCategories.AsNoTracking()
            .FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == request.CategoryId, cancellationToken);

        if (category is null)
        {
            return ServiceResult<ProductDto>.Failure(StatusCodes.Status400BadRequest, new ServiceError("invalid_category", "Category does not exist for this tenant."));
        }

        var sku = request.Sku.Trim();
        var duplicateSku = await _dbContext.Products.AnyAsync(x => x.GlampingId == glampingId && x.Sku == sku, cancellationToken);
        if (duplicateSku)
        {
            return ServiceResult<ProductDto>.Failure(StatusCodes.Status409Conflict, new ServiceError("duplicate_sku", "A product with this SKU already exists."));
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            GlampingId = glampingId,
            Sku = sku,
            Name = request.Name.Trim(),
            CategoryId = request.CategoryId,
            Unit = request.Unit.Trim(),
            SalePrice = request.SalePrice,
            CostPrice = request.CostPrice,
            IsActive = request.IsActive,
            TrackStock = request.TrackStock
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<ProductDto>.Success(
            new ProductDto(product.Id, product.Sku, product.Name, product.CategoryId, category.Name, product.Unit, product.SalePrice, product.CostPrice, product.IsActive, product.TrackStock),
            StatusCodes.Status201Created);
    }

    public async Task<ServiceResult<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == id, cancellationToken);

        if (product is null)
        {
            return ServiceResult<ProductDto>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Product not found."));
        }

        var category = await _dbContext.ProductCategories.AsNoTracking()
            .FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == request.CategoryId, cancellationToken);

        if (category is null)
        {
            return ServiceResult<ProductDto>.Failure(StatusCodes.Status400BadRequest, new ServiceError("invalid_category", "Category does not exist for this tenant."));
        }

        var sku = request.Sku.Trim();
        var duplicateSku = await _dbContext.Products.AnyAsync(x => x.GlampingId == glampingId && x.Sku == sku && x.Id != id, cancellationToken);
        if (duplicateSku)
        {
            return ServiceResult<ProductDto>.Failure(StatusCodes.Status409Conflict, new ServiceError("duplicate_sku", "A product with this SKU already exists."));
        }

        product.Sku = sku;
        product.Name = request.Name.Trim();
        product.CategoryId = request.CategoryId;
        product.Unit = request.Unit.Trim();
        product.SalePrice = request.SalePrice;
        product.CostPrice = request.CostPrice;
        product.IsActive = request.IsActive;
        product.TrackStock = request.TrackStock;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<ProductDto>.Success(
            new ProductDto(product.Id, product.Sku, product.Name, product.CategoryId, category.Name, product.Unit, product.SalePrice, product.CostPrice, product.IsActive, product.TrackStock));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == id, cancellationToken);

        if (product is null)
        {
            return ServiceResult<bool>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Product not found."));
        }

        var usedInCharges = await _dbContext.ChargeItems.AnyAsync(x => x.GlampingId == glampingId && x.ProductId == id, cancellationToken);
        if (usedInCharges)
        {
            return ServiceResult<bool>.Failure(
                StatusCodes.Status409Conflict,
                new ServiceError("has_history", "Cannot delete a product used in charges."));
        }

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Success(true);
    }

    private static ProductDto MapDto(Product product)
        => new(product.Id, product.Sku, product.Name, product.CategoryId, product.Category.Name, product.Unit, product.SalePrice, product.CostPrice, product.IsActive, product.TrackStock);
}
