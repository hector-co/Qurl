using Microsoft.Extensions.DependencyInjection;

namespace Qurl.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        public static void AddQurlModelBinder(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddMvcCore(o =>
            {
                o.ModelBinderProviders.Insert(0, new QueryModelBinderProvider());
            });
        }
    }
}
