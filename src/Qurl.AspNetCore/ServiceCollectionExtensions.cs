using Microsoft.Extensions.DependencyInjection;
using Qurl.AspNetCore;
using System;

namespace Qurl
{
    public static class ServiceCollectionExtensions
    {
        public static void AddQurl(this IServiceCollection services)
        {
            services.AddSingleton<QueryHelper>();
            services.AddSingleton<FilterFactory>();
            services.AddSingleton<QueryBuilder>();
        }

        public static void AddQurl(this IServiceCollection services, Action<QurlOptions> options)
        {
            options(new QurlOptions(services));
        }
    }
}
