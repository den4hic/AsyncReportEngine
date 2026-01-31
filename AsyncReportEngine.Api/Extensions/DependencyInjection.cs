using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.DataAccess.Context;
using AsyncReportEngine.DataAccess.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AsyncReportEngine.Api.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection RegisterDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ReportDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ReportDbContext).Assembly.FullName)));

        services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ReportDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IReportRepository, ReportRepository>();

        return services;
    }
}
