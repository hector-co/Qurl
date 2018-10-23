using Microsoft.AspNetCore.Mvc.ModelBinding;
using Qurl.WebUtilities;
using System;
using System.Threading.Tasks;

namespace Qurl.AspNetCore
{
    public class QueryModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var query = QueryBuilder.FromQueryString(bindingContext.ModelType, bindingContext.HttpContext.Request.QueryString.Value);
            bindingContext.Result = ModelBindingResult.Success(query);
            return Task.CompletedTask;
        }
    }
}
