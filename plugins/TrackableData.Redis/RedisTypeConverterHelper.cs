using System;
using System.Linq.Expressions;
using StackExchange.Redis;

namespace TrackableData.Redis
{
    internal static class RedisTypeConverterHelper
    {
        // Func<T, RedisValue> -> Func<object, RedisValue>
        public static Func<object, RedisValue> ConvertToObjectToFunc<T>(Func<T, RedisValue> func)
        {
            return o => func((T)o);
        }

        // Func<RedisValue, T> -> Func<RedisValue, object>
        public static Func<RedisValue, object> ConvertToObjectFromFunc<T>(Func<RedisValue, T> func)
        {
            return o => func(o);
        }

        // Func<object, RedisValue> -> Func<T, RedisValue>
        public static Delegate ConvertToToFunc(Type type, Func<object, RedisValue> func)
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

        // Func<RedisValue, object> -> Func<RedisValue, T>
        public static Delegate ConvertToFromFunc(Type type, Func<RedisValue, object> func)
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
