using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Qurl
{
    public static class SwaggerExtensions
    {
        public static void AddQurlDefinitions(this SwaggerGenOptions options)
        {
            options.ParameterFilter<QurlParameterFilter>();
        }
    }
}
