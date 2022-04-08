using System;
using System.Linq;
using System.Linq.Expressions;

namespace Mensch.Id.API.Helpers
{
    public static class SearchExpressionBuilder
    {
        public static Expression<Func<T, bool>> ContainsAll<T>(
            Expression<Func<T, string>> selector,
            params string[] searchTerms)
        {
            if (searchTerms.Length == 0)
                throw new ArgumentException("At least one search term must be passed to " + nameof(ContainsAll));
            var containsMethod = typeof(string).GetMethod("Contains", new[] {typeof(string)});
            Expression expression = Expression.Call(selector.Body, containsMethod, Expression.Constant(searchTerms[0], typeof(string)));
            foreach (var searchTerm in searchTerms.Skip(1))
            {
                expression = Expression.AndAlso(
                    expression, 
                    Expression.Call(selector.Body, containsMethod, Expression.Constant(searchTerm, typeof(string))));
            }
            var result = Expression.Lambda<Func<T,bool>>(expression, selector.Parameters[0]);
            return result;
        }

        public static Expression<Func<T, bool>> ContainsAny<T>(
            Expression<Func<T, string>> selector,
            params string[] searchTerms)
        {
            if (searchTerms.Length == 0)
                throw new ArgumentException("At least one search term must be passed to " + nameof(ContainsAny));
            var containsMethod = typeof(string).GetMethod("Contains", new[] {typeof(string)});
            Expression expression = Expression.Call(selector.Body, containsMethod, Expression.Constant(searchTerms[0], typeof(string)));
            foreach (var searchTerm in searchTerms.Skip(1))
            {
                expression = Expression.OrElse(
                    expression, 
                    Expression.Call(selector.Body, containsMethod, Expression.Constant(searchTerm, typeof(string))));
            }
            var result = Expression.Lambda<Func<T,bool>>(expression, selector.Parameters[0]);
            return result;
        }

        public static Expression<Func<T, bool>> And<T>(params Expression<Func<T, bool>>[] expressions)
        {
            if (!expressions.Any())
                throw new ArgumentException("At least one expressions must be passed to OR");
            if (expressions.Length == 1)
                return expressions[0];
            Expression<Func<T, bool>> left = expressions[0];
            for (int i = 1; i < expressions.Length; i++)
            {
                var right = expressions[i];
                var parameterReplacedRight = new ExpressionParameterReplacer(right.Parameters, left.Parameters)
                    .Visit(right.Body);
                var andExpression = Expression.AndAlso(left.Body, parameterReplacedRight);
                left = Expression.Lambda<Func<T,bool>>(andExpression, left.Parameters);
            }
            return left;
        }

        public static Expression<Func<T, bool>> Or<T>(params Expression<Func<T, bool>>[] expressions)
        {
            if (!expressions.Any())
                throw new ArgumentException("At least one expressions must be passed to OR");
            if (expressions.Length == 1)
                return expressions[0];
            Expression<Func<T, bool>> left = expressions[0];
            for (int i = 1; i < expressions.Length; i++)
            {
                var right = expressions[i];
                var parameterReplacedRight = new ExpressionParameterReplacer(right.Parameters, left.Parameters)
                    .Visit(right.Body);
                var andExpression = Expression.OrElse(left.Body, parameterReplacedRight);
                left = Expression.Lambda<Func<T,bool>>(andExpression, left.Parameters);
            }
            return left;
        }
    }
}