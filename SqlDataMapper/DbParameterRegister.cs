using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDataMapper
{
    /// <summary>
    /// 注册数据参数
    /// </summary>
    public sealed class DbParameterRegister
    {
        DbParameterRegister(string name, object value, bool output)
        {
            this.Name = name;
            this.Value = value;
            this.Output = output;
        }
        public string Name { get; private set; }

        public object Value { get; set; }

        public bool Output { get; private set; }

        public static DbParameterRegister Create(string name, object value)
        {
            return new DbParameterRegister(name, value, false);
        }

        public static DbParameterRegister Create(string name, object value, bool output)
        {
            return new DbParameterRegister(name, value, output);
        }

    }
}
