using System;
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
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    internal static class ExpressionExtensions
    {

        public static Action<T, object> GetSetPKFunc<T>()
        {
            //Expression.NewArrayInit
            Type sourceType = typeof(T);
            ParameterExpression sourceInstance = Expression.Parameter(sourceType, "SourceInstance");
            var valueInstance = Expression.Parameter(typeof(object), "Value");
            PropertyInfo pi = null;
            foreach (var item in sourceType.GetProperties())
            {
                var cAtt = item.GetCustomAttribute<ColumnAttribute>();
                if (cAtt != null)
                {
                    if (cAtt.IsPrimaryKey)
                    {
                        pi = item;
                        break;
                    }
                }
            }
            var body = Expression.Call(sourceInstance, pi.SetMethod, Expression.Convert(valueInstance, pi.PropertyType));
            return Expression.Lambda<Action<T, object>>(body, sourceInstance, valueInstance).Compile();
        }
        public static Func<T, object[]> GetPropertyFunc<T>()
        {
            Type sourceType = typeof(T);
            ParameterExpression sourceInstance = Expression.Parameter(sourceType, "SourceInstance");
            List<Expression> list = new List<Expression>();
            foreach (var item in sourceType.GetProperties())
            {
                var cAtt = item.GetCustomAttribute<ColumnAttribute>();
                if (cAtt != null)
                {
                    if (cAtt.IsDbGenerated)
                    {
                        continue;
                    }
                }
                var valueExpression = Expression.Call(sourceInstance, item.GetMethod);
                list.Add(Expression.Convert(valueExpression, typeof(object)));
            }
            var array = Expression.NewArrayInit(typeof(object), list);
            return Expression.Lambda<Func<T, object[]>>(array, sourceInstance).Compile();
        }

        public static Func<T, Tuple<string, object>[]> GetPropertyFuncTuple<T>()
        {
            Type sourceType = typeof(T);
            ParameterExpression sourceInstance = Expression.Parameter(sourceType, "SourceInstance");
            List<Expression> list = new List<Expression>();
            var constructor = typeof(Tuple<string, object>).GetConstructor(new Type[] { typeof(string), typeof(object) });
            foreach (var item in sourceType.GetProperties())
            {
                var cAtt = item.GetCustomAttribute<ColumnAttribute>();
                if (cAtt != null)
                {
                    if (cAtt.IsDbGenerated)
                    {
                        continue;
                    }
                }
                var valueExpression = Expression.Call(sourceInstance, item.GetMethod);

                var nameExp = Expression.Constant(item.Name);

                list.Add(Expression.New(constructor, nameExp, Expression.Convert(valueExpression, typeof(object))));
            }
            var array = Expression.NewArrayInit(typeof(Tuple<string, object>), list);
            return Expression.Lambda<Func<T, Tuple<string, object>[]>>(array, sourceInstance).Compile();
        }

        /// <summary>
        /// 从提供的 DataRecord 对象创建新委托实例。
        /// </summary>
        /// <param name="RecordInstance">表示一个 DataRecord 实例</param>
        /// <returns>从提供的 DataRecord 对象创建新委托实例。</returns>
        /// <remarks></remarks>
        public static Func<IDataReader, T> GetInstanceCreator<T>(IDataReader reader)
        {
            List<MemberBinding> bindings = new List<MemberBinding>();
            Type targetType = typeof(T);
            Type sourceType = typeof(IDataRecord);
            ParameterExpression sourceInstance = Expression.Parameter(sourceType, "SourceInstance");
            //MethodInfo getSourcePropertyMethodExpression = sourceType.GetProperty("Item", new Type[] { typeof(int) }).GetGetMethod();
            MethodInfo getSourcePropertyMethodExpression = sourceType.GetMethod("GetValue", new Type[] { typeof(int) });
            DataTable schemaTable = reader.GetSchemaTable();
            //通过在目标属性和字段在记录中的循环检查哪些是匹配的
            for (int i = 0; i <= reader.FieldCount - 1; i++)
            {
                string name = reader.GetName(i);
                var targetProperty = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (targetProperty == null)
                {
                    continue;
                }

                //是否是Column
                if (targetProperty.GetCustomAttribute<ColumnAttribute>() == null)
                {
                    continue;
                }

                //获取 RecordField 的类型
                Type recordFieldType = reader.GetFieldType(i);
                //RecordField 可空类型检查
                if ((bool)(schemaTable.Rows[i]["AllowDBNull"]) == true && recordFieldType.IsValueType)
                {
                    recordFieldType = typeof(Nullable<>).MakeGenericType(recordFieldType);
                }
                //为 RecordField 创建一个表达式
                Expression recordFieldExpression = Expression.Call(sourceInstance, getSourcePropertyMethodExpression, Expression.Constant(i, typeof(int)));

                //获取一个表示 SourceValue 的表达式
                Expression sourceValueExpression = GetSourceValueExpression(recordFieldType, recordFieldExpression);

                Type targetPropertyType = targetProperty.PropertyType;
                //从 RecordField 到 TargetProperty 类型的值转换
                Expression convertedRecordFieldExpression = GetConvertedRecordFieldExpression(recordFieldType, sourceValueExpression, targetPropertyType);

                //MethodInfo TargetPropertySetter = TargetProperty.GetSetMethod();
                //为属性创建绑定
                var bindExpression = Expression.Bind(targetProperty, convertedRecordFieldExpression);
                //将绑定添加到绑定列表
                bindings.Add(bindExpression);
            }
            //创建 Target 的新实例并绑定到 DataRecord
            MemberInitExpression body = Expression.MemberInit(Expression.New(targetType), bindings);
            return Expression.Lambda<Func<IDataReader, T>>(body, sourceInstance).Compile();
        }

        /// <summary>
        /// 从提供的 DataRow 对象创建新委托实例。
        /// </summary>
        /// <param name="RecordInstance">表示一个 DataRow 实例</param>
        /// <returns>从提供的 DataRow 对象创建新委托实例。</returns>
        /// <remarks></remarks>
        public static Func<DataRow, T> GetInstanceCreator<T>(DataRow row)
        {
            List<MemberBinding> bindings = new List<MemberBinding>();
            Type targetType = typeof(T);
            Type sourceType = typeof(DataRow);

            ParameterExpression sourceInstance = Expression.Parameter(sourceType, "SourceInstance");
            MethodInfo getSourcePropertyMethodExpression = sourceType.GetProperty("Item", new Type[] { typeof(int) }).GetGetMethod();

            var table = row.Table;
            //通过在目标属性和字段在记录中的循环检查哪些是匹配的
            for (int i = 0; i <= table.Columns.Count - 1; i++)
            {
                var column = table.Columns[i];

                string name = column.ColumnName;
                var targetProperty = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (targetProperty == null)
                {
                    continue;
                }
                //获取 RecordField 的类型
                Type recordFieldType = column.DataType;
                //RecordField 可空类型检查
                if (column.AllowDBNull && recordFieldType.IsValueType)
                {
                    recordFieldType = typeof(Nullable<>).MakeGenericType(recordFieldType);
                }

                //MethodInfo getSourcePropertyMethodExpression=Expression.Property(sourceInstance,)

                //为 RecordField 创建一个表达式
                Expression recordFieldExpression = Expression.Call(sourceInstance, getSourcePropertyMethodExpression, Expression.Constant(i, typeof(int)));

                //获取一个表示 SourceValue 的表达式
                Expression sourceValueExpression = GetSourceValueExpression(recordFieldType, recordFieldExpression);

                Type targetPropertyType = targetProperty.PropertyType;
                //从 RecordField 到 TargetProperty 类型的值转换
                Expression convertedRecordFieldExpression = GetConvertedRecordFieldExpression(recordFieldType, sourceValueExpression, targetPropertyType);

                //MethodInfo TargetPropertySetter = TargetProperty.GetSetMethod();
                //为属性创建绑定
                var bindExpression = Expression.Bind(targetProperty, convertedRecordFieldExpression);
                //将绑定添加到绑定列表
                bindings.Add(bindExpression);
            }
            //创建 Target 的新实例并绑定到 DataRecord
            MemberInitExpression body = Expression.MemberInit(Expression.New(targetType), bindings);
            return Expression.Lambda<Func<DataRow, T>>(body, sourceInstance).Compile();
        }

        public static Delegate GetInstanceCreator<TResult>(Type sourceType)
        {
            List<MemberBinding> bindings = new List<MemberBinding>();
            Type targetType = typeof(TResult);
            ParameterExpression sourceInstance = Expression.Parameter(sourceType, "SourceInstance");

            foreach (var item in sourceType.GetProperties())
            {
                var targetProperty = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(o => o.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));
                if (targetProperty == null)
                {
                    continue;
                }
                if (targetProperty.SetMethod == null)
                {
                    continue;
                }

                var valueExpression = Expression.Call(sourceInstance, item.GetMethod);


                Expression convertedRecordFieldExpression = GetConvertedRecordFieldExpression(item.PropertyType, valueExpression, targetProperty.PropertyType);

                var bindExpression = Expression.Bind(targetProperty, convertedRecordFieldExpression);
                bindings.Add(bindExpression);
            }
            //创建 Target 的新实例并绑定
            MemberInitExpression body = Expression.MemberInit(Expression.New(targetType), bindings);

            return Expression.Lambda(body, sourceInstance).Compile();

            var tt = Expression.Convert(sourceInstance, typeof(object));

            var ttt = Expression.Assign(tt, sourceInstance);

            return Expression.Lambda<Func<object, TResult>>(body, sourceInstance).Compile();

            var t = typeof(Func<,>);
            var funcType = t.MakeGenericType(sourceType, targetType);
            var exp = Expression.Lambda(funcType, body, sourceInstance);


            return exp.Compile() as Func<object, TResult>;

            //return Expression.Lambda<Func<object, TResult>>(exp.Body, exp1);
            //return Expression.Lambda<Func<object, TResult>>(body, sourceInstance).Compile();

            //return Expression.Lambda<Func<object, TResult>>(body, sourceInstance).Compile();
            //return Expression.Lambda(body, sourceInstance).Compile();

        }
        [Obsolete()]
        public static Delegate GetInstanceCreatorOld<TResult>(Type sourceType)
        {
            List<MemberBinding> bindings = new List<MemberBinding>();
            Type targetType = typeof(TResult);
            ParameterExpression sourceInstance = Expression.Parameter(sourceType, "SourceInstance");

            foreach (var item in sourceType.GetProperties())
            {
                var targetProperty = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(o => o.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));
                if (targetProperty == null)
                {
                    continue;
                }
                if (targetProperty.SetMethod == null)
                {
                    continue;
                }

                var valueExpression = Expression.Call(sourceInstance, item.GetMethod);

                var bindExpression = Expression.Bind(targetProperty, valueExpression);
                bindings.Add(bindExpression);
            }
            //创建 Target 的新实例并绑定
            MemberInitExpression body = Expression.MemberInit(Expression.New(targetType), bindings);

            return Expression.Lambda(body, sourceInstance).Compile();

            var tt = Expression.Convert(sourceInstance, typeof(object));

            var ttt = Expression.Assign(tt, sourceInstance);

            return Expression.Lambda<Func<object, TResult>>(body, sourceInstance).Compile();

            var t = typeof(Func<,>);
            var funcType = t.MakeGenericType(sourceType, targetType);
            var exp = Expression.Lambda(funcType, body, sourceInstance);


            return exp.Compile() as Func<object, TResult>;

            //return Expression.Lambda<Func<object, TResult>>(exp.Body, exp1);
            //return Expression.Lambda<Func<object, TResult>>(body, sourceInstance).Compile();

            //return Expression.Lambda<Func<object, TResult>>(body, sourceInstance).Compile();
            //return Expression.Lambda(body, sourceInstance).Compile();

        }

        /// <summary>
        /// 获取表示 RecordField 真实值的表达式。
        /// </summary>
        /// <param name="RecordFieldType">表示 RecordField 的类型。</param>
        /// <param name="RecordFieldExpression">表示 RecordField 的表达式。</param>
        /// <returns>表示 SourceValue 的表达式。</returns>
        private static Expression GetSourceValueExpression(Type recordFieldType, Expression recordFieldExpression)
        {
            //首先从 RecordField 取消装箱值，以便我们可以使用它
            UnaryExpression unboxedRecordFieldExpression = Expression.Convert(recordFieldExpression, recordFieldType);

            //获取一个检查 SourceField 为 null 值的表达式
            UnaryExpression nullCheckExpression = Expression.IsFalse(Expression.TypeIs(recordFieldExpression, typeof(DBNull)));

            ParameterExpression value = Expression.Variable(recordFieldType, "Value");
            //获取一个设置 TargetProperty 值的表达式
            Expression sourceValueExpression = Expression.Block(
                new ParameterExpression[] { value },
                Expression.IfThenElse(
                    nullCheckExpression,
                    Expression.Assign(value, unboxedRecordFieldExpression),
                    Expression.Assign(value, Expression.Constant(GetDefaultValue(recordFieldType), recordFieldType))),
                    Expression.Convert(value, recordFieldType));
            return sourceValueExpression;
        }

        /// <summary>
        /// Gets an expression representing the recordField converted to the TargetPropertyType
        /// </summary>
        /// <param name="RecordFieldType">The Type of the RecordField</param>
        /// <param name="UnboxedRecordFieldExpression">An Expression representing the Unboxed RecordField value</param>
        /// <param name="TargetPropertyType">The Type of the TargetProperty</param>
        /// <returns></returns>
        private static Expression GetConvertedRecordFieldExpression(Type recordFieldType, Expression unboxedRecordFieldExpression, Type targetPropertyType)
        {
            Expression convertedRecordFieldExpression = default(Expression);
            if (object.ReferenceEquals(targetPropertyType, recordFieldType))
            {
                //Just assign the unboxed expression
                convertedRecordFieldExpression = unboxedRecordFieldExpression;

            }
            else if (object.ReferenceEquals(targetPropertyType, typeof(string)))
            {
                //There are no casts from primitive types to String.
                //And Expression.Convert Method (Expression, Type, MethodInfo) only works with static methods.
                convertedRecordFieldExpression = Expression.Call(unboxedRecordFieldExpression, recordFieldType.GetMethod("ToString", Type.EmptyTypes));
            }
            else
            {
                //Using Expression.Convert works wherever you can make an explicit or implicit cast.
                //But it casts OR unboxes an object, therefore the double cast. First unbox to the SourceType and then cast to the TargetType
                //It also doesn't convert a numerical type to a String or date, this will throw an exception.
                convertedRecordFieldExpression = Expression.Convert(unboxedRecordFieldExpression, targetPropertyType);
            }
            return convertedRecordFieldExpression;
        }



        static object GetDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}
