using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Rewriter.Core
{
    public class BusyContext : IDisposable
    {
        private readonly PropertyInfo _property;
        private readonly object _target;

        public BusyContext(Expression<Func<bool>> expression)
        {
            var memberExpression = (MemberExpression)expression.Body;
            _property = (PropertyInfo)memberExpression.Member;
            _target = ((ConstantExpression) memberExpression.Expression).Value;

            _property.SetValue(_target, true);
        }

        public void Dispose() => _property.SetValue(_target, false);
    }
}