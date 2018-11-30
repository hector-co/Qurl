﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using Qurl;

namespace Qurl.AspNetCore
{
    public class QueryModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType.IsValidQueryType())
                return new QueryModelBinder();
            return null;
        }
    }
}

