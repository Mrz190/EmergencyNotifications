using API.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class DatabaseExtension
    {
        public static IServiceCollection ConnectSqlServer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DbConnection"));
            });

            return services;
        }

        public static IServiceCollection ConnectMongo(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<MongoDbContext>(serviceProvider =>
            {
                var mongoSettings = configuration.GetSection("MongoSettings");
                var connectionsStrings = mongoSettings.GetValue<string>("ConnectionString");
                var databaseName = mongoSettings.GetValue<string>("DatabaseName");
                return new MongoDbContext(connectionsStrings, databaseName);
            });

            return services;
        }
    }
}