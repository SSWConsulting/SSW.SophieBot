using Microsoft.Extensions.DependencyInjection.Extensions;
using SSW.SophieBot.Persistence;
using SSW.SophieBot.Sync;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DataSyncServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultDataSync(this IServiceCollection services)
        {
            services.TryAddTransient(typeof(IPersistenceMigrator<,>), typeof(NullPersistenceMigrator<,>));

            services.TryAddTransient(typeof(IOdataSyncService<>), typeof(NullOdataSyncService<>));
            services.TryAddTransient(typeof(IPagedOdataSyncService<>), typeof(NullPagedOdataSyncService<>));

            services.TryAddTransient<ISyncVersionGenerator, DefaultSyncVersionGenerator>();

            return services;
        }
    }
}
