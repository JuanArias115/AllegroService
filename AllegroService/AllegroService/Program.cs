using AllegroService.Api.Auth;
using AllegroService.Api.Common;
using AllegroService.Api.Middlewares;
using AllegroService.Application;
using AllegroService.Application.Common;
using AllegroService.Application.Interfaces;
using AllegroService.Infrastructure.Persistence;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

var firebaseProjectId = builder.Configuration[$"{FirebaseOptions.SectionName}:ProjectId"]
    ?? builder.Configuration["Firebase__ProjectId"];
if (string.IsNullOrWhiteSpace(firebaseProjectId))
{
    throw new InvalidOperationException("Firebase ProjectId configuration is required. Configure Firebase:ProjectId or env var Firebase__ProjectId.");
}

var firebaseIssuer = $"https://securetoken.google.com/{firebaseProjectId}";

builder.Services.Configure<FirebaseOptions>(builder.Configuration.GetSection(FirebaseOptions.SectionName));
builder.Services.Configure<BusinessRulesOptions>(builder.Configuration.GetSection(BusinessRulesOptions.SectionName));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();

builder.Services.AddApplicationLayer();

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors)
            .Select(x => new ServiceError("validation_error", string.IsNullOrWhiteSpace(x.ErrorMessage) ? "Invalid request." : x.ErrorMessage))
            .ToList();

        return new BadRequestObjectResult(ApiResponse<object>.Fail(errors));
    };
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.Authority = firebaseIssuer;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = firebaseIssuer,
            ValidateAudience = true,
            ValidAudience = firebaseProjectId,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtBearer");

                logger.LogWarning(context.Exception, "JWT authentication failed. Path: {Path}", context.HttpContext.Request.Path);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtBearer");

                var sub = context.Principal?.FindFirstValue(FirebaseClaimTypes.Subject)
                    ?? context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

                logger.LogInformation("JWT token validated successfully. sub: {Sub}", string.IsNullOrWhiteSpace(sub) ? "<missing>" : sub);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("Admin", policy => policy.RequireClaim(FirebaseClaimTypes.Role, "Admin"));
    options.AddPolicy("Reception", policy => policy.RequireClaim(FirebaseClaimTypes.Role, "Reception"));
    options.AddPolicy("Restaurant", policy => policy.RequireClaim(FirebaseClaimTypes.Role, "Restaurant"));
    options.AddPolicy("Inventory", policy => policy.RequireClaim(FirebaseClaimTypes.Role, "Inventory"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AllegroService API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseMiddleware<ResolveTenantContextMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.Run();
