using Microsoft.Extensions.DependencyInjection;
using System;

namespace Qurl.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        public static void AddQurlModelBinder(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddMvcCore(o =>
            {
                o.ModelBinderProviders.Insert(0, new QueryModelBinderProvider(FilterMode.RHS));
            });
        }

        public static void AddQurlModelBinder(this IServiceCollection serviceCollection, Action<QurlOptions> options)
        {
            options(new QurlOptions(serviceCollection));
        }
    }
}
