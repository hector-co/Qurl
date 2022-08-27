﻿using System.Collections.Generic;
using System.Linq.Expressions;

namespace Qurl.Filters
{
    public abstract class FilterPropertyBase<TValue> : IFilterProperty
    {
        private string _modelPropertyName = string.Empty;
        public string PropertyName { get; internal set; } = string.Empty;
        public abstract IEnumerable<TValue> Values { get; }

        public string ModelPropertyName
        {
            get
            {
                return string.IsNullOrEmpty(_modelPropertyName)
                    ? PropertyName
                    : _modelPropertyName;
            }
            internal set { _modelPropertyName = value; }
        }
        public bool CustomFiltering { get; internal set; }

        public abstract void SetValues(params object?[] values);

        public virtual Expression? GetFilterExpression<TModel>(ParameterExpression modelParameter)
        {
            var propExp = ModelPropertyName.GetPropertyExpression<TModel>(modelParameter);
            if (propExp == null)
                return null;

            return GetExpression(propExp);
        }

        protected abstract Expression GetExpression(Expression property);

        public void SetOptions(string propertyName, string modelPropertyName, bool customFiltering)
        {
            PropertyName = propertyName;
            ModelPropertyName = modelPropertyName;
            CustomFiltering = customFiltering;
        }
    }
}