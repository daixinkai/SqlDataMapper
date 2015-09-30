using SqlDataMapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    internal static class FactoryCache
    {

        static readonly Cache<Tuple<Type, string>, Delegate> _factories = new Cache<Tuple<Type, string>, Delegate>();

        static readonly Cache<Tuple<Type, string>, Delegate> _tableFactories = new Cache<Tuple<Type, string>, Delegate>();


        static readonly Cache<Type, Tuple<string, Delegate>> _insertFactories = new Cache<Type, Tuple<string, Delegate>>();

        static readonly Cache<Type, Delegate> _setPKFactories = new Cache<Type, Delegate>();


        static readonly Cache<Type, Type> _pKTypeFactories = new Cache<Type, Type>();

        public static Delegate GetFactory<T>(string sql, IDataReader reader)
        {
            var key = new Tuple<Type, string>(typeof(T), sql);
            return _factories.Get(key, () =>
               {
                   return GetExpression<T>(reader);
               });
        }


        public static Delegate GetFactory<T>(IDataReader reader)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var type = reader.GetFieldType(i);
                var name = reader.GetName(i);
                string keyItem = name + "_" + type.Name;
                sb.Append(keyItem + "_");
            }
            var key = new Tuple<Type, string>(typeof(T), sb.ToString());
            return _factories.Get(key, () =>
            {
                return GetExpression<T>(reader);
            });

        }

        public static Action<T, object> GetSetPKFactory<T>()
        {
            return _setPKFactories.Get(typeof(T), () =>
            {
                return ExpressionExtensions.GetSetPKFunc<T>();
            }) as Action<T, object>;
        }

        public static Type GetPKType<T>()
        {
            return _pKTypeFactories.Get(typeof(T), () =>
            {
                foreach (var item in typeof(T).GetProperties())
                {
                    var att = item.GetCustomAttribute<ColumnAttribute>();
                    if (att != null && att.IsPrimaryKey)
                    {
                        return item.PropertyType;
                    }
                }
                return null;
            });
        }


        public static Tuple<string, Tuple<string, object>[]> GetInsertSql<T>(T entity)
        {
            var factory = _insertFactories.Get(typeof(T), () =>
            {
                StringBuilder args = new StringBuilder();
                StringBuilder values = new StringBuilder();
                foreach (var item in typeof(T).GetProperties())
                {
                    var cAtt = item.GetCustomAttribute<ColumnAttribute>();
                    if (cAtt != null)
                    {
                        //这里这样做
                        continue;
                        if (cAtt.IsDbGenerated)
                        {
                            continue;
                        }
                    }
                    args.Append(item.Name + ",");
                    values.Append("@" + item.Name + ",");
                }
                string sql = "Insert Into {0} ({1}) Values ({2});Select @@identity;";
                sql = string.Format(sql, typeof(T).Name, args.ToString().TrimEnd(','), values.ToString().TrimEnd(','));
                return new Tuple<string, Delegate>(sql, ExpressionExtensions.GetPropertyFuncTuple<T>());
            });

            var func = factory.Item2 as Func<T, Tuple<string, object>[]>;

            return new Tuple<string, Tuple<string, object>[]>(factory.Item1, func(entity));
        }

        #region DataTable

        public static Delegate GetFactory<T>(DataTable table)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];

                var type = column.DataType;
                var name = column.ColumnName;
                string keyItem = name + "_" + type.Name;
                sb.Append(keyItem + "_");
            }
            var key = new Tuple<Type, string>(typeof(T), sb.ToString());
            return _tableFactories.Get(key, () =>
            {
                return GetExpression<T>(table);
            });

        }
        #endregion


        static Func<IDataReader, T> GetExpression<T>(IDataReader reader)
        {
            return ExpressionExtensions.GetInstanceCreator<T>(reader);
        }

        static Func<DataRow, T> GetExpression<T>(DataTable table)
        {
            return ExpressionExtensions.GetInstanceCreator<T>(table.Rows[0]);
        }

        static Expression<Func<IDataRecord, T>> GetExpression1<T>(IDataRecord reader)
        {
            var type = typeof(T);
            var ps = type.GetProperties();
            NewExpression newObj = Expression.New(type);
            ParameterExpression readerPar = Expression.Parameter(typeof(IDataRecord), "reader");
            int count = reader.FieldCount;
            List<MemberBinding> temp = new List<MemberBinding>();
            Dictionary<MemberInfo, ParameterExpression> pars = new Dictionary<MemberInfo, ParameterExpression>();

            for (int i = 0; i < count; i++)
            {

                var srcType = reader.GetFieldType(i);
                var name = reader.GetName(i);
                PropertyInfo pi = ps.FirstOrDefault(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (pi == null)
                {
                    continue;
                }

                //ParameterExpression pa = Expression.Parameter(srcType, "arg" + i);
                //pars.Add(pi, pa);

                //常数表达式
                ConstantExpression cons = Expression.Constant(i);

                var propertyExp = Expression.Property(newObj, pi);

                //是否是DBNull.Value
                MethodCallExpression isDBNull = Expression.Call(readerPar, typeof(IDataRecord).GetMethod("IsDBNull"), cons);
                //调用方法
                MethodCallExpression call = Expression.Call(readerPar, typeof(IDataRecord).GetMethod("GetValue"), cons);


                var isTrue = Expression.Convert(Expression.Call(readerPar, typeof(IDataRecord).GetMethod("GetValue"), cons), pi.PropertyType);

                var isFalse = Expression.Default(pi.PropertyType);

                var tttt = Expression.IfThenElse(isDBNull, Expression.Assign(propertyExp, isTrue), Expression.Assign(propertyExp, isFalse));

                var bind = Expression.Bind(pi, propertyExp);

                temp.Add(bind);


            }

            var member = Expression.MemberInit(newObj, pars.Select(o => Expression.Bind(o.Key, o.Value)));
            var exp = Expression.Lambda<Func<IDataRecord, T>>(member, readerPar);
            return exp;
            //return null;
        }

        static IEnumerable<ParameterExpression> BuildExpression(Expression exp, IDataReader reader, Type type)
        {
            yield break;
            //var ps = type.GetProperties();
            //int count = reader.FieldCount;
            //for (int i = 0; i < count; i++)
            //{
            //    var name = reader.GetName(i);
            //    PropertyInfo pi = ps.FirstOrDefault(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            //    if (pi == null)
            //    {
            //        continue;
            //    }
            //    var value = reader.GetValue(i);
            //    if (value == null || value == DBNull.Value)
            //    {
            //        continue;
            //    }
            //    var body = Expression.Call(exp, pi.GetGetMethod());
            //    yield return body;
            //}
        }

    }


}
