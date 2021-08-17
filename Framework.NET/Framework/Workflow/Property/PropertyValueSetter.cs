﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Framework.Workflow.Property
{
    internal class PropertyValueSetter<TContext, TProperty>
    {
        public static async Task SetAsync(TContext context, Func<TContext, Task<TProperty>> actionReturn,
            Expression<Func<TContext, TProperty>> propertyPicker)
        {
            var prop = (PropertyInfo) ((MemberExpression) propertyPicker.Body).Member;
            prop.SetValue(context, await actionReturn(context).ConfigureAwait(false), null);
        }
    }
}