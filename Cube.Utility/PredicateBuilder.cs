using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Cube.Utility
{
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        public static Expression<Func<T, T2, bool>> True<T, T2>() { return (f, f2) => true; }
        public static Expression<Func<T, T2, bool>> False<T, T2>() { return (f, f2) => false; }

        public static Expression<Func<T, T2, T3, bool>> True<T, T2, T3>() { return (f, f2, f3) => true; }
        public static Expression<Func<T, T2, T3, bool>> False<T, T2, T3>() { return (f, f2, f3) => false; }

        public static Expression<Func<T, T2, T3, T4, bool>> True<T, T2, T3, T4>() { return (f, f2, f3, f4) => true; }
        public static Expression<Func<T, T2, T3, T4, bool>> False<T, T2, T3, T4>() { return (f, f2, f3, f4) => false; }

        public static Expression<Func<T, T2, T3, T4, T5, bool>> True<T, T2, T3, T4, T5>() { return (f, f2, f3, f4, f5) => true; }
        public static Expression<Func<T, T2, T3, T4, T5, bool>> False<T, T2, T3, T4, T5>() { return (f, f2, f3, f4, f5) => false; }

        public static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {
            // build parameter map (from parameters of second to parameters of first)
            var map = first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] }).ToDictionary(p => p.s, p => p.f);

            // replace parameters in the second lambda expression with parameters from the first
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);

            // apply composition of lambda expression bodies to parameters from the first expression 
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.AndAlso);
        }

        public static Expression<Func<T, T2, bool>> And<T, T2>(this Expression<Func<T, T2, bool>> first, Expression<Func<T, T2, bool>> second)
        {
            return first.Compose(second, Expression.AndAlso);
        }

        public static Expression<Func<T, T2, T3, bool>> And<T, T2, T3>(this Expression<Func<T, T2, T3, bool>> first, Expression<Func<T, T2, T3, bool>> second)
        {
            return first.Compose(second, Expression.AndAlso);
        }


        public static Expression<Func<T, T2, T3, T4, bool>> And<T, T2, T3, T4>(this Expression<Func<T, T2, T3, T4, bool>> first, Expression<Func<T, T2, T3, T4, bool>> second)
        {
            return first.Compose(second, Expression.AndAlso);
        }


        public static Expression<Func<T, T2, T3, T4, T5, bool>> And<T, T2, T3, T4, T5>(this Expression<Func<T, T2, T3, T4, T5, bool>> first, Expression<Func<T, T2, T3, T4, T5, bool>> second)
        {
            return first.Compose(second, Expression.AndAlso);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.OrElse);
        }

        public static Expression<Func<T, T2, bool>> Or<T, T2>(this Expression<Func<T, T2, bool>> first, Expression<Func<T, T2, bool>> second)
        {
            return first.Compose(second, Expression.OrElse);
        }
        public static Expression<Func<T, T2, T3, bool>> Or<T, T2, T3>(this Expression<Func<T, T2, T3, bool>> first, Expression<Func<T, T2, T3, bool>> second)
        {
            return first.Compose(second, Expression.OrElse);
        }

        public static Expression<Func<T, T2, T3, T4, bool>> Or<T, T2, T3, T4>(this Expression<Func<T, T2, T3, T4, bool>> first, Expression<Func<T, T2, T3, T4, bool>> second)
        {
            return first.Compose(second, Expression.OrElse);
        }

        public static Expression<Func<T, T2, T3, T4, T5, bool>> Or<T, T2, T3, T4, T5>(this Expression<Func<T, T2, T3, T4, T5, bool>> first, Expression<Func<T, T2, T3, T4, T5, bool>> second)
        {
            return first.Compose(second, Expression.OrElse);
        }


    }


    public class ParameterRebinder : ExpressionVisitor
    {
        private readonly Dictionary<ParameterExpression, ParameterExpression> map;

        public ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
        {
            this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }

        public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
        {
            return new ParameterRebinder(map).Visit(exp);
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            if (map.TryGetValue(p, out ParameterExpression replacement))
            {
                p = replacement;
            }
            return base.VisitParameter(p);
        }

    }

}
