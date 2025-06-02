using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using CampaignModel = Domain.Models.CampaignModel;

namespace Utility
{
    public static class ExpressionBuilders
    {
        public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            var param = Expression.Parameter(typeof(T), "o");
            var body = Expression.AndAlso( Expression.Invoke(left, param), Expression.Invoke(right, param));
            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            return lambda;
        }
        public static Expression And(Expression first, Expression second)
        {
            return Expression.And(first, second);
        }        
    }
}
