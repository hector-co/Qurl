using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Qurl
{
    public class QueryModelBinderProvider : IModelBinderProvider
    {
        private readonly FilterMode _filterMode;

        public QueryModelBinderProvider(FilterMode filterMode)
        {
            _filterMode = filterMode;
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType.IsValidQueryType())
                return new QueryModelBinder(_filterMode);
            return null;
        }
    }
}

