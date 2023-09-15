using Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class DbContextConfigurationExtensions
{
    public static void AddServerAccessServices(this IServiceCollection self, string connectionString, string dbProvider = "SqlServer", int maxRetryCount = 3)
    {
        if (dbProvider.Equals("MySql", StringComparison.OrdinalIgnoreCase))
        {
            self.AddDbContext<ApiServerContext, MySqlContext>
                (options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                            x => x.EnableRetryOnFailure(maxRetryCount: maxRetryCount)));
        }
        else
        {
            self.AddDbContext<ApiServerContext, SqlServerContext>
                (options => options.UseSqlServer(connectionString,
                            x => x.EnableRetryOnFailure(maxRetryCount: maxRetryCount)));
        }
    }
}

