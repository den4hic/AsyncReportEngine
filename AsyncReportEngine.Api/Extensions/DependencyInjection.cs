using AsyncReportEngine.Api.BackgroundServices;
using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.DataAccess.Context;
using AsyncReportEngine.DataAccess.Repositories;
using AsyncReportEngine.Services;
using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.MappingProfiles;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AsyncReportEngine.Api.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection RegisterDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ReportDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ReportDbContext).Assembly.FullName)));

        services.AddAutoMapper(config =>
        {
            config.AddProfile<CatalogProfile>();
            config.AddProfile<OrderProfile>();
            config.AddProfile<PartnerProfile>();
            config.AddProfile<ReportProfile>();
        });

        services.AddIdentityCore<IdentityUser>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ReportDbContext>()
            .AddDefaultTokenProviders();

        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["Secret"];

        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
            };
        });

        services.AddSignalR();
        services.AddHostedService<InMemoryReportWorker>();

        services.AddSingleton<QueueClient>(provider =>
        {
            var connectionString = configuration.GetConnectionString("AzureStorage");
            var queueName = "report-jobs";

            var client = new QueueClient(connectionString, queueName);
            client.CreateIfNotExists();

            return client;
        });

        services.AddSingleton<BlobServiceClient>(provider =>
        {
            var connectionString = configuration.GetConnectionString("AzureStorage");
            return new BlobServiceClient(connectionString);
        });

        services.AddScoped<IQueueService, QueueService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ISyncReportService, SyncReportService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBlobService, BlobService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IPartnerService, PartnerService>();
        services.AddScoped<IReportHistoryService, ReportHistoryService>();

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPartnerRepository, PartnerRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();

        services.AddSingleton<IInMemoryQueue, InMemoryQueue>();

        return services;
    }
}