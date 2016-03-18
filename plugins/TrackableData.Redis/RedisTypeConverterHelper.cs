using System;
using StackExchange.Redis;
using System.Linq.Expressions;

namespace TrackableData.Redis
{
    internal static class RedisTypeConverterHelper
    {
        public static Func<object, RedisValue> Convert<T>(Func<T, RedisValue> func)
        {
            return o => func((T)o);
        }

        public static Func<RedisValue, object> Convert<T>(Func<RedisValue, T> func)
        {
            return o => func(o);
        }

        public static Delegate Convert(Type type, Func<object, RedisValue> func)
        {
            var v = Expression.Parameter(type, "v");
            if (func.Target != null)
            {
                return Expression.Lambda(
                    Expression.Call(
                        Expression.Constant(func.Target),
                        func.Method,
                        Expression.Convert(v, typeof(object))),
                    v).Compile();
            }
            else
            {
                return Expression.Lambda(
                    Expression.Call(
                        func.Method,
                        Expression.Convert(v, typeof(object))),
                    v).Compile();
            }
        }

        public static Delegate Convert(Type type, Func<RedisValue, object> func)
        {
            var v = Expression.Parameter(typeof(RedisValue), "v");
            if (func.Target != null)
            {
                return Expression.Lambda(
                    Expression.Convert(
                        Expression.Call(
                            Expression.Constant(func.Target),
                            func.Method, v), type),
                    v).Compile();
            }
            else
            {
                return Expression.Lambda(
                    Expression.Convert(
                        Expression.Call(func.Method, v), type),
                    v).Compile();
            }
        }
    }
}
