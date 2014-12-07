using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace Ultrasonic.DownloadManager.Core.Utils
{
    public static class ExpressionHelper
    {
        public static string GetPropertyName(LambdaExpression expression)
        {
            MemberExpression memExpression;
            return GetPropertyName(expression, out memExpression);
        }

        public static string GetPropertyName<TSource, TResult>(Expression<Func<TSource, TResult>> propertyExpression)
        {
            MemberExpression memExpression;
            return GetPropertyName(propertyExpression, out memExpression);
        }

        public static string GetPropertyName(LambdaExpression expression, out MemberExpression bodyMemberExpression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            bodyMemberExpression = expression.Body as MemberExpression;
            if (bodyMemberExpression == null)
            {
                var convertExpr = expression.Body as UnaryExpression;
                if (convertExpr != null)
                {
                    bodyMemberExpression = convertExpr.Operand as MemberExpression;
                }
            }
            if (bodyMemberExpression == null || bodyMemberExpression.Expression != expression.Parameters[0] || bodyMemberExpression.Member.MemberType != MemberTypes.Property)
            {
                throw new ArgumentException("Invalid property expression.", "propertyExpression");
            }

            return bodyMemberExpression.Member.Name;
        }
    }
}
