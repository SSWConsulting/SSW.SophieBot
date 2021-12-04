using Microsoft.Extensions.DependencyInjection;
using SSW.SophieBot.Entities;
using SSW.SophieBot.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Extensions.DependencyInjection
{
    public static class LuisModelServiceCollectionExtensions
    {
        public static IServiceCollection AddEntities<T>(this IServiceCollection services)
        {
            var entityTypes = AssemblyHelper
                .GetAllTypes(typeof(T).Assembly)
                .Where(
                    type => type != null &&
                            type.IsClass &&
                            !type.IsAbstract &&
                            !type.IsGenericType &&
                            typeof(EntityBase).IsAssignableFrom(type)
                ).ToList();

            if (!entityTypes.IsNullOrEmpty())
            {
                entityTypes.ForEach(entityType => services.AddTransient(entityType));
            }

            return services;
        }
    }
}
