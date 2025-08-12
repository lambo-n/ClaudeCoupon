using System;
using System.Data;
using EcommerceCouponLibrary.Core.Interfaces;
using EcommerceCouponLibrary.Providers.Dapper;
using EcommerceCouponLibrary.Providers.EfCore;
using EcommerceCouponLibrary.Providers.InMemory;
using EcommerceCouponLibrary.Providers.Mongo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace EcommerceCouponLibrary.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCouponLibraryInMemory(this IServiceCollection services, Action<InMemoryCouponRepository>? configure = null)
        {
            var repo = new InMemoryCouponRepository();
            configure?.Invoke(repo);
            services.AddSingleton<ICouponRepository>(repo);
            return services;
        }

        public static IServiceCollection AddCouponLibraryEfCore<TDbContext>(this IServiceCollection services)
            where TDbContext : CouponDbContext
        {
            services.AddScoped<ICouponRepository, EfCoreCouponRepository>();
            return services;
        }

        public static IServiceCollection AddCouponLibraryMongo(this IServiceCollection services, IMongoDatabase database)
        {
            services.AddSingleton<ICouponRepository>(new MongoCouponRepository(database));
            return services;
        }

        public static IServiceCollection AddCouponLibraryDapper(this IServiceCollection services, Func<IServiceProvider, IDbConnection> connectionFactory)
        {
            services.AddScoped(connectionFactory);
            services.AddScoped<ICouponRepository, DapperCouponRepository>();
            return services;
        }
    }
}
