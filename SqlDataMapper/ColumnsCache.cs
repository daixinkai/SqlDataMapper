using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDataMapper
{
    internal static class ColumnsCache
    {
        static readonly Cache<Type, string> _columnsCache = new Cache<Type, string>();

        public static string GetColumns(Type type)
        {
            return _columnsCache.Get(type, () =>
            {
                StringBuilder sb = new StringBuilder(); 
                foreach (var item in type.GetProperties())
                {
                    if (item.GetCustomAttributes(typeof(Attribute), true).Length > 0)
                    {
                        continue;
                    }
                    sb.Append(type.Name + "." + item.Name + ",");
                    //sb.Append("[" + item.Name + "],");
                }
                return sb.ToString().TrimEnd(',');
            });
        }

        public static string BuildSelectColumns(Type type)
        {
            return string.Format("Select {0} From {1}", GetColumns(type), type.Name); ;
        }

    }
}
