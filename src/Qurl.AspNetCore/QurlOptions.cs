using Microsoft.Extensions.DependencyInjection;
using System;

namespace Qurl.AspNetCore
{
    public class QurlOptions
    {
        private readonly QueryHelper _queryHelper;

        public QurlOptions(IServiceCollection services)
        {
            _queryHelper = new QueryHelper();
            
            services.AddSingleton(_queryHelper);
            services.AddSingleton<FilterFactory>();
            services.AddSingleton<QueryBuilder>();
        }

        public void SetDateTimeConverter(Func<DateTime, DateTime> dateTimeConverter)
        {
            _queryHelper.SetDateTimeConverter(dateTimeConverter);
        }

        public void SetDateTimeOffsetConverter(Func<DateTimeOffset, DateTimeOffset> dateTimeOffsetConverter)
        {
            _queryHelper.SetDateTimeOffsetConverter(dateTimeOffsetConverter);
        }
    }
}
