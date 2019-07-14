using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ToolsPortable.Sql
{
    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
    public class Column : System.Attribute
    {
        public string ColumnName { get; private set; }

        public Column(string columnName)
        {
            ColumnName = columnName;
        }
    }

    //public class ImagesColumn : Column
    //{
    //    public ImagesColumn() : base("VarChar600_1") { }
    //}

    public static class Columns
    {
        public static List<ColumnAndValue> GetData(object item)
        {
            List<ColumnAndValue> answer = new List<ColumnAndValue>();

            if (item == null)
                return answer;

            Type t = item.GetType();

            foreach (PropertyInfo property in t.GetRuntimeProperties().Where(i => i.CanRead))
            {
                Column c = property.GetCustomAttributes(typeof(Column), true).FirstOrDefault() as Column;

                if (c != null)
                {
                    answer.Add(new ColumnAndValue(c.ColumnName, property.GetValue(item, null)));
                }
            }

            foreach (FieldInfo field in t.GetRuntimeFields().Where(i => i.IsPublic))
            {
                Column c = field.GetCustomAttributes(typeof(Column), true).FirstOrDefault() as Column;

                if (c != null)
                    answer.Add(new ColumnAndValue(c.ColumnName, field.GetValue(item)));
            }

            return answer;
        }
    }

    public class ColumnAndValue
    {
        public string Name { get; private set; }
        public object Value { get; private set; }

        internal ColumnAndValue(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}
