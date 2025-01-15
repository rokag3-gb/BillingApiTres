using System.Linq.Expressions;

namespace Billing.EF
{
    /// <summary>
    /// bool을 반환하는 함수 대리자 T에 대한 두 표현식을 지정한 조건에 맞게 invoke합니다.
    /// 여러 조건식을 동적으로 and 또는 or 를 지정하여 연결합니다.
    /// https://www.albahari.com/nutshell/predicatebuilder.aspx
    /// </summary>
    public static class PredicateBuilder
    {
        /// <summary>
        /// True 및 False 메서드는 특별한 작업을 수행하지 않습니다. 처음에 true 또는 false로 평가되는 Expression을 만들기 위한 편리한 바로 가기일 뿐입니다.
        /// </summary>
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        /// <summary>
        /// True 및 False 메서드는 특별한 작업을 수행하지 않습니다. 처음에 true 또는 false로 평가되는 Expression을 만들기 위한 편리한 바로 가기일 뿐입니다.
        /// </summary>
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
                                                            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
                                                             Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}
