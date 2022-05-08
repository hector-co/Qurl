using Microsoft.Extensions.DependencyInjection;
using System;

namespace Qurl
{
    public static class ServiceCollectionExtensions
    {
        public static void AddQurlModelBinder(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddMvcCore(o =>
            {
                o.ModelBinderProviders.Insert(0, new QueryModelBinderProvider(FilterMode.LHS));
            });
        }

        public static void AddQurlModelBinder(this IServiceCollection serviceCollection, Action<QurlOptions> options)
        {
            options(new QurlOptions(serviceCollection));
        }
    }
}
