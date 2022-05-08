using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Qurl
{
    public class QurlParameterFilter : IParameterFilter
    {
        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            if (IsQurlType(context.PropertyInfo))
            {
                parameter.Name = FormatName(parameter.Name);
                if (parameter.Schema?.Reference != null)
                {
                    context.SchemaRepository.Schemas.Remove(parameter.Schema.Reference.Id);
                }
                if (parameter.Name.ToUpper() == QueryBuilder.FieldsQueryField || parameter.Name.ToUpper() == QueryBuilder.SortQueryField)
                {
                    parameter.Schema.Format = null;
                    parameter.Schema.Reference = null;
                    parameter.Schema.Type = "string";
                    parameter.Schema.Items = null;
                    context.SchemaRepository.Schemas.Remove(nameof(SortDirection));
                    context.SchemaRepository.Schemas.Remove(nameof(SortValue));
                }
                else if (parameter.Name.ToUpper() != QueryBuilder.OffsetQueryField && parameter.Name.ToUpper() != QueryBuilder.LimitQueryField)
                {
                    parameter.Schema.Format = null;
                    parameter.Schema.Reference = null;
                    parameter.Schema.Type = "string";
                }
            }
        }

        private static string FormatName(string name)
        {
            if (name.Contains("."))
            {
                var nn = name.Split('.');
                if (nn.Length > 1)
                {
                    return $"{ToCamelCase(nn[0])}.{ToCamelCase(nn[1])}";
                }
            }
            return ToCamelCase(name);

            string ToCamelCase(string value)
            {
                return value.Substring(0, 1).ToLower() + value.Substring(1, value.Length - 1);
            }
        }

        private static bool IsQurlType(PropertyInfo propInfo)
        {
            if (propInfo == null) return false;
            return propInfo.PropertyType.Namespace == nameof(Qurl) || propInfo.ReflectedType.Namespace == nameof(Qurl)
                || propInfo.DeclaringType.Namespace == nameof(Qurl);
        }
    }
}
