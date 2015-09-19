using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDataMapper
{
    internal static class MapperFactory
    {
        static readonly Cache<Tuple<Type, int>, Delegate> _facoties = new Cache<Tuple<Type, int>, Delegate>();
        /// <summary>
        /// 得到映射委托
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Delegate GetFactory<T>(IDataReader reader)
        {
            return GetFactory(reader, typeof(T));
        }
        /// <summary>
        /// 得到映射委托
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Delegate GetFactory(IDataReader reader, Type type)
        {
            Tuple<Type, int> key = new Tuple<Type, int>(type, GetColumnHash(reader));
            return _facoties.Get(key, () =>
            {
                return null;
            });
        }

        static int GetColumnHash(IDataRecord record)
        {
            unchecked
            {
                int colCount = record.FieldCount, hash = colCount;
                for (int i = 0; i < colCount; i++)
                {   // binding code is only interested in names - not types
                    object tmp = record.GetName(i);
                    hash = (hash * 31) + (tmp == null ? 0 : tmp.GetHashCode());
                }
                return hash;
            }
        }

    }
}
