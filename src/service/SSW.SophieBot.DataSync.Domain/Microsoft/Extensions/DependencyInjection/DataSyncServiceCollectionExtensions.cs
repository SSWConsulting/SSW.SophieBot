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
            services.TryAddTransient(typeof(IPagedSyncService<>), typeof(NullPagedSyncService<>));
            services.TryAddTransient<ISyncVersionGenerator, DefaultSyncVersionGenerator>();

            return services;
        }
    }
}
