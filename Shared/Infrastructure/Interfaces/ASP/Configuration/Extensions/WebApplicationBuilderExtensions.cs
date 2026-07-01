using SafeFlow.API.Alerts.Application.Internal.CommandServices;
using SafeFlow.API.Alerts.Application.Internal.QueryServices;
using SafeFlow.API.Alerts.Application.Services;
using SafeFlow.API.Alerts.Domain.Repositories;
using SafeFlow.API.Alerts.Infrastructure.Persistence.EFC.Repositories;
using SafeFlow.API.Analytics.Application.Internal;
using SafeFlow.API.Analytics.Application.Services;
using SafeFlow.API.EnvironmentalMonitoring.Application.Internal;
using SafeFlow.API.EnvironmentalMonitoring.Application.Services;
using SafeFlow.API.EnvironmentalMonitoring.Domain.Repositories;
using SafeFlow.API.EnvironmentalMonitoring.Infrastructure.Persistence.EFC.Repositories;
using SafeFlow.API.Iam.Application.Internal.CommandServices;
using SafeFlow.API.Iam.Application.Internal.QueryServices;
using SafeFlow.API.Iam.Application.Outbound;
using SafeFlow.API.Iam.Application.Services;
using SafeFlow.API.Iam.Domain.Repositories;
using SafeFlow.API.Iam.Infrastructure.Hashing.Services;
using SafeFlow.API.Iam.Infrastructure.Persistence.EFC.Repositories;
using SafeFlow.API.Iam.Infrastructure.Tokens.Jwt.Configuration;
using SafeFlow.API.Iam.Infrastructure.Tokens.Jwt.Services;
using SafeFlow.API.Inventory.Application.Internal.CommandServices;
using SafeFlow.API.Inventory.Application.Internal.QueryServices;
using SafeFlow.API.Inventory.Application.Services;
using SafeFlow.API.Inventory.Domain.Repositories;
using SafeFlow.API.Inventory.Infrastructure.Persistence.EFC.Repositories;
using SafeFlow.API.Logistics.Application.Internal.CommandServices;
using SafeFlow.API.Logistics.Application.Internal.QueryServices;
using SafeFlow.API.Logistics.Application.Services;
using SafeFlow.API.Logistics.Domain.Repositories;
using SafeFlow.API.Logistics.Infrastructure.Persistence.EFC.Repositories;
using SafeFlow.API.Reporting.Application.Internal;
using SafeFlow.API.Reporting.Application.Services;
using SafeFlow.API.Reporting.Domain.Repositories;
using SafeFlow.API.Reporting.Infrastructure.Persistence.EFC.Repositories;
using SafeFlow.API.Shared.Domain.Repositories;
using SafeFlow.API.Shared.Infrastructure.Interfaces.ASP.Configuration;
using SafeFlow.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using SafeFlow.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SafeFlow.API.Shared.Infrastructure.Interfaces.ASP.Configuration.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddSafeFlowServices(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseMySQL(connectionString));

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<IInventoryLineRepository, InventoryLineRepository>();
        builder.Services.AddScoped<IInventoryCommandService, InventoryCommandService>();
        builder.Services.AddScoped<IInventoryQueryService, InventoryQueryService>();

        builder.Services.AddScoped<ILogisticsQueryRepository, LogisticsQueryRepository>();
        builder.Services.AddScoped<ILogisticsQueryService, LogisticsQueryService>();
        builder.Services.AddScoped<ILogisticsCommandService, LogisticsCommandService>();

        builder.Services.AddScoped<IAlertRepository, AlertRepository>();
        builder.Services.AddScoped<IAlertCommandService, AlertCommandService>();
        builder.Services.AddScoped<IAlertQueryService, AlertQueryService>();

        builder.Services.AddScoped<ITemperatureReadingRepository, TemperatureReadingRepository>();
        builder.Services.AddScoped<IEnvironmentalMonitoringCommandService, EnvironmentalMonitoringCommandService>();
        builder.Services.AddScoped<IEnvironmentalMonitoringQueryService, EnvironmentalMonitoringQueryService>();

        builder.Services.AddScoped<IReportingQueryRepository, ReportingQueryRepository>();
        builder.Services.AddScoped<IReportingQueryService, ReportingQueryService>();

        builder.Services.AddScoped<IAnalyticsQueryService, AnalyticsQueryService>();

        builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUserCommandService, UserCommandService>();
        builder.Services.AddScoped<IUserQueryService, UserQueryService>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IHashingService, HashingService>();

        builder.Services.AddControllers(options =>
            options.Conventions.Add(new KebabCaseRouteNamingConvention()));

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddCors(options =>
        {
            var extraOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? [];

            options.AddPolicy("SafeFlowFrontend", policy =>
            {
                policy.SetIsOriginAllowed(origin =>
                    {
                        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                            return false;
                        if (uri.Scheme is not "http" and not "https")
                            return false;
                        if (uri.Host is "localhost" or "127.0.0.1")
                            return true;
                        if (uri.Host.EndsWith(".netlify.app", StringComparison.OrdinalIgnoreCase))
                            return true;
                        return extraOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
                    })
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return builder;
    }
}
