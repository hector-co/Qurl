using Microsoft.Extensions.DependencyInjection;

namespace Qurl
{
    public static class ServiceCollectionExtensions
    {
        public static void AddQurl(this IServiceCollection services)
        {
            services.AddSingleton<FilterFactory>();
            services.AddSingleton<QueryBuilder>();
        }
    }
}
