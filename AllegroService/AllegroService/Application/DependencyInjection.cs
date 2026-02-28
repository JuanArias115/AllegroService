using AllegroService.Application.Interfaces;
using AllegroService.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AllegroService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddScoped<IUnitService, UnitService>();
        services.AddScoped<IGuestService, GuestService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IStayService, StayService>();
        services.AddScoped<IFolioService, FolioService>();
        services.AddScoped<IUserTenantService, UserTenantService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
