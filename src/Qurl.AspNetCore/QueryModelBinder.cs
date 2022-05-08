using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Threading.Tasks;

namespace Qurl
{
    public class QueryModelBinder : IModelBinder
    {
        private readonly FilterMode _filterMode;

        public QueryModelBinder(FilterMode filterMode)
        {
            _filterMode = filterMode;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var query = QueryBuilder.FromQueryString(bindingContext.ModelType, bindingContext.HttpContext.Request.QueryString.Value, _filterMode);
            bindingContext.Result = ModelBindingResult.Success(query);
            return Task.CompletedTask;
        }
    }
}
