using AllegroService.Application.Common;
using AllegroService.Application.DTOs.Folios;
using AllegroService.Application.Interfaces;
using AllegroService.Domain.Entities;
using AllegroService.Domain.Enums;
using AllegroService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AllegroService.Application.Services;

public class FolioService : IFolioService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserContext _currentUser;
    private readonly BusinessRulesOptions _businessRulesOptions;

    public FolioService(
        AppDbContext dbContext,
        ICurrentUserContext currentUser,
        IOptions<BusinessRulesOptions> businessRulesOptions)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _businessRulesOptions = businessRulesOptions.Value;
    }

    public async Task<ServiceResult<FolioDetailDto>> GetByIdAsync(Guid folioId, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();

        var folio = await _dbContext.Folios.AsNoTracking()
            .FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == folioId, cancellationToken);

        if (folio is null)
        {
            return ServiceResult<FolioDetailDto>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Folio not found."));
        }

        var charges = await _dbContext.Charges.AsNoTracking()
            .Where(x => x.GlampingId == glampingId && x.FolioId == folioId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var chargeIds = charges.Select(x => x.Id).ToArray();

        var chargeItems = await _dbContext.ChargeItems.AsNoTracking()
            .Where(x => x.GlampingId == glampingId && chargeIds.Contains(x.ChargeId))
            .ToListAsync(cancellationToken);

        var payments = await _dbContext.Payments.AsNoTracking()
            .Where(x => x.GlampingId == glampingId && x.FolioId == folioId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var chargeDtos = charges
            .Select(charge => new ChargeDto(
                charge.Id,
                charge.Source,
                charge.Description,
                charge.Qty,
                charge.UnitPrice,
                charge.Total,
                chargeItems
                    .Where(item => item.ChargeId == charge.Id)
                    .Select(item => new ChargeItemDto(item.Id, item.ProductId, item.Qty, item.UnitPrice, item.Total))
                    .ToList(),
                charge.CreatedAt))
            .ToList();

        var paymentDtos = payments
            .Select(payment => new PaymentDto(payment.Id, payment.Amount, payment.Method, payment.Status, payment.PaidAt, payment.Reference, payment.CreatedAt))
            .ToList();

        var chargesTotal = chargeDtos.Sum(x => x.Total);
        var paymentsTotal = paymentDtos.Where(x => x.Status == PaymentStatus.Paid).Sum(x => x.Amount);
        var balance = decimal.Round(chargesTotal - paymentsTotal, 2, MidpointRounding.AwayFromZero);

        return ServiceResult<FolioDetailDto>.Success(new FolioDetailDto(
            folio.Id,
            folio.StayId,
            folio.Status,
            folio.OpenedAt,
            folio.ClosedAt,
            chargesTotal,
            paymentsTotal,
            balance,
            chargeDtos,
            paymentDtos));
    }

    public async Task<ServiceResult<ChargeDto>> AddChargeAsync(Guid folioId, AddChargeRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var folio = await _dbContext.Folios
            .FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == folioId, cancellationToken);

        if (folio is null)
        {
            return ServiceResult<ChargeDto>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Folio not found."));
        }

        if (folio.Status != FolioStatus.Open)
        {
            return ServiceResult<ChargeDto>.Failure(StatusCodes.Status409Conflict, new ServiceError("folio_closed", "Cannot add charges to a closed folio."));
        }

        var normalizedItems = request.Items?.ToList() ?? new List<AddChargeItemRequest>();

        var chargeId = Guid.NewGuid();
        var chargeItems = new List<ChargeItem>();
        var stockMovements = new List<StockMovement>();

        decimal chargeQty;
        decimal chargeUnitPrice;
        decimal chargeTotal;

        if (normalizedItems.Count == 0)
        {
            var qty = request.Qty ?? 0;
            var unitPrice = request.UnitPrice ?? 0;

            if (qty <= 0 || unitPrice < 0)
            {
                return ServiceResult<ChargeDto>.Failure(
                    StatusCodes.Status400BadRequest,
                    new ServiceError("invalid_charge", "Qty must be > 0 and UnitPrice must be >= 0."));
            }

            chargeQty = qty;
            chargeUnitPrice = unitPrice;
            chargeTotal = decimal.Round(qty * unitPrice, 2, MidpointRounding.AwayFromZero);
        }
        else
        {
            var productIds = normalizedItems.Where(x => x.ProductId.HasValue).Select(x => x.ProductId!.Value).Distinct().ToList();

            var products = await _dbContext.Products
                .Where(x => x.GlampingId == glampingId && productIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            if (products.Count != productIds.Count)
            {
                return ServiceResult<ChargeDto>.Failure(StatusCodes.Status400BadRequest, new ServiceError("invalid_product", "One or more products do not exist for this tenant."));
            }

            var requiresStock = normalizedItems.Any(item => item.ProductId.HasValue && products[item.ProductId.Value].TrackStock);

            Location? location = null;
            Dictionary<Guid, StockBalance> stockBalances = new();

            if (requiresStock)
            {
                if (!request.LocationId.HasValue)
                {
                    return ServiceResult<ChargeDto>.Failure(StatusCodes.Status400BadRequest, new ServiceError("location_required", "LocationId is required for stock-tracked products."));
                }

                location = await _dbContext.Locations
                    .FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == request.LocationId.Value, cancellationToken);

                if (location is null)
                {
                    return ServiceResult<ChargeDto>.Failure(StatusCodes.Status400BadRequest, new ServiceError("invalid_location", "Location does not exist for this tenant."));
                }

                var trackedProductIds = normalizedItems
                    .Where(item => item.ProductId.HasValue && products[item.ProductId.Value].TrackStock)
                    .Select(item => item.ProductId!.Value)
                    .Distinct()
                    .ToList();

                stockBalances = await _dbContext.StockBalances
                    .Where(x => x.GlampingId == glampingId
                        && x.LocationId == location.Id
                        && trackedProductIds.Contains(x.ProductId))
                    .ToDictionaryAsync(x => x.ProductId, cancellationToken);
            }

            var calculatedTotal = 0m;

            foreach (var item in normalizedItems)
            {
                Product? product = null;
                decimal unitPrice;

                if (item.ProductId.HasValue)
                {
                    product = products[item.ProductId.Value];

                    var canOverride = request.AllowOverridePrice && _businessRulesOptions.AllowOverridePrice;
                    unitPrice = canOverride && item.UnitPrice.HasValue
                        ? item.UnitPrice.Value
                        : product.SalePrice;
                }
                else
                {
                    if (!item.UnitPrice.HasValue)
                    {
                        return ServiceResult<ChargeDto>.Failure(
                            StatusCodes.Status400BadRequest,
                            new ServiceError("price_required", "UnitPrice is required for non-product charge items."));
                    }

                    unitPrice = item.UnitPrice.Value;
                }

                var itemTotal = decimal.Round(item.Qty * unitPrice, 2, MidpointRounding.AwayFromZero);
                calculatedTotal += itemTotal;

                chargeItems.Add(new ChargeItem
                {
                    Id = Guid.NewGuid(),
                    GlampingId = glampingId,
                    ChargeId = chargeId,
                    ProductId = item.ProductId,
                    Qty = item.Qty,
                    UnitPrice = unitPrice,
                    Total = itemTotal
                });

                if (product is not null && product.TrackStock)
                {
                    var locationId = request.LocationId!.Value;

                    if (!stockBalances.TryGetValue(product.Id, out var balance) || balance.QtyOnHand < item.Qty)
                    {
                        return ServiceResult<ChargeDto>.Failure(
                            StatusCodes.Status409Conflict,
                            new ServiceError("insufficient_stock", $"Insufficient stock for product {product.Sku}."));
                    }

                    balance.QtyOnHand -= item.Qty;

                    stockMovements.Add(new StockMovement
                    {
                        Id = Guid.NewGuid(),
                        GlampingId = glampingId,
                        ProductId = product.Id,
                        LocationId = locationId,
                        Type = StockMovementType.Out,
                        Qty = item.Qty,
                        ReferenceType = "Charge",
                        ReferenceId = chargeId
                    });
                }
            }

            chargeQty = 1;
            chargeUnitPrice = calculatedTotal;
            chargeTotal = decimal.Round(calculatedTotal, 2, MidpointRounding.AwayFromZero);
        }

        var charge = new Charge
        {
            Id = chargeId,
            GlampingId = glampingId,
            FolioId = folioId,
            Source = request.Source,
            Description = request.Description.Trim(),
            Qty = chargeQty,
            UnitPrice = chargeUnitPrice,
            Total = chargeTotal
        };

        _dbContext.Charges.Add(charge);
        if (chargeItems.Count > 0)
        {
            _dbContext.ChargeItems.AddRange(chargeItems);
        }

        if (stockMovements.Count > 0)
        {
            _dbContext.StockMovements.AddRange(stockMovements);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var chargeDto = new ChargeDto(
            charge.Id,
            charge.Source,
            charge.Description,
            charge.Qty,
            charge.UnitPrice,
            charge.Total,
            chargeItems.Select(item => new ChargeItemDto(item.Id, item.ProductId, item.Qty, item.UnitPrice, item.Total)).ToList(),
            charge.CreatedAt);

        return ServiceResult<ChargeDto>.Success(chargeDto, StatusCodes.Status201Created);
    }

    public async Task<ServiceResult<PaymentDto>> AddPaymentAsync(Guid folioId, AddPaymentRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();

        var folio = await _dbContext.Folios
            .FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == folioId, cancellationToken);

        if (folio is null)
        {
            return ServiceResult<PaymentDto>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Folio not found."));
        }

        if (folio.Status != FolioStatus.Open)
        {
            return ServiceResult<PaymentDto>.Failure(StatusCodes.Status409Conflict, new ServiceError("folio_closed", "Cannot add payments to a closed folio."));
        }

        if (request.Amount <= 0)
        {
            return ServiceResult<PaymentDto>.Failure(StatusCodes.Status400BadRequest, new ServiceError("invalid_amount", "Payment amount must be greater than zero."));
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            GlampingId = glampingId,
            FolioId = folioId,
            Amount = request.Amount,
            Method = request.Method,
            Status = PaymentStatus.Paid,
            PaidAt = request.PaidAt ?? DateTimeOffset.UtcNow,
            Reference = string.IsNullOrWhiteSpace(request.Reference) ? null : request.Reference.Trim()
        };

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<PaymentDto>.Success(
            new PaymentDto(payment.Id, payment.Amount, payment.Method, payment.Status, payment.PaidAt, payment.Reference, payment.CreatedAt),
            StatusCodes.Status201Created);
    }
}
