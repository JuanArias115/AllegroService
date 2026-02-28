using AllegroService.Application.Common;
using AllegroService.Application.DTOs.Common;
using AllegroService.Application.DTOs.Guests;
using AllegroService.Application.Interfaces;
using AllegroService.Domain.Entities;
using AllegroService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AllegroService.Application.Services;

public class GuestService : IGuestService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserContext _currentUser;

    public GuestService(AppDbContext dbContext, ICurrentUserContext currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult<PagedResponse<GuestDto>>> GetPagedAsync(ListQueryRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var query = _dbContext.Guests.AsNoTracking().Where(x => x.GlampingId == glampingId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(x => x.FullName.ToLower().Contains(term)
                || x.Email.ToLower().Contains(term)
                || x.Phone.ToLower().Contains(term));
        }

        query = request.Sort?.ToLower() switch
        {
            "email" => query.OrderBy(x => x.Email),
            "email_desc" => query.OrderByDescending(x => x.Email),
            _ => query.OrderBy(x => x.FullName)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new GuestDto(x.Id, x.FullName, x.DocumentId, x.Phone, x.Email))
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResponse<GuestDto>>.Success(new PagedResponse<GuestDto>
        {
            Items = items,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }

    public async Task<ServiceResult<GuestDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();

        var guest = await _dbContext.Guests.AsNoTracking()
            .Where(x => x.GlampingId == glampingId && x.Id == id)
            .Select(x => new GuestDto(x.Id, x.FullName, x.DocumentId, x.Phone, x.Email))
            .FirstOrDefaultAsync(cancellationToken);

        return guest is null
            ? ServiceResult<GuestDto>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Guest not found."))
            : ServiceResult<GuestDto>.Success(guest);
    }

    public async Task<ServiceResult<GuestDto>> CreateAsync(CreateGuestRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();

        var guest = new Guest
        {
            Id = Guid.NewGuid(),
            GlampingId = glampingId,
            FullName = request.FullName.Trim(),
            DocumentId = string.IsNullOrWhiteSpace(request.DocumentId) ? null : request.DocumentId.Trim(),
            Phone = request.Phone.Trim(),
            Email = request.Email.Trim()
        };

        _dbContext.Guests.Add(guest);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<GuestDto>.Success(
            new GuestDto(guest.Id, guest.FullName, guest.DocumentId, guest.Phone, guest.Email),
            StatusCodes.Status201Created);
    }

    public async Task<ServiceResult<GuestDto>> UpdateAsync(Guid id, UpdateGuestRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var guest = await _dbContext.Guests.FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == id, cancellationToken);

        if (guest is null)
        {
            return ServiceResult<GuestDto>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Guest not found."));
        }

        guest.FullName = request.FullName.Trim();
        guest.DocumentId = string.IsNullOrWhiteSpace(request.DocumentId) ? null : request.DocumentId.Trim();
        guest.Phone = request.Phone.Trim();
        guest.Email = request.Email.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<GuestDto>.Success(new GuestDto(guest.Id, guest.FullName, guest.DocumentId, guest.Phone, guest.Email));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var guest = await _dbContext.Guests.FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == id, cancellationToken);

        if (guest is null)
        {
            return ServiceResult<bool>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Guest not found."));
        }

        var hasReservations = await _dbContext.Reservations.AnyAsync(x => x.GlampingId == glampingId && x.GuestId == id, cancellationToken);
        if (hasReservations)
        {
            return ServiceResult<bool>.Failure(
                StatusCodes.Status409Conflict,
                new ServiceError("has_history", "Cannot delete a guest with reservation history."));
        }

        _dbContext.Guests.Remove(guest);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Success(true);
    }
}
