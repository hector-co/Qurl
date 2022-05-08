using Microsoft.Extensions.DependencyInjection;

namespace Qurl
{
    public class QurlOptions
    {
        private readonly IServiceCollection _serviceCollection;

        public QurlOptions(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public void UseRhsMode()
        {
            _serviceCollection.AddMvcCore(o =>
            {
                o.ModelBinderProviders.Insert(0, new QueryModelBinderProvider(FilterMode.RHS));
            });
        }

        public void UseLhsMode()
        {
            _serviceCollection.AddMvcCore(o =>
            {
                o.ModelBinderProviders.Insert(0, new QueryModelBinderProvider(FilterMode.LHS));
            });
        }
    }
}
