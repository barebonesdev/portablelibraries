using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace ToolsPortable
{
    [DataContract]
    public abstract class Clonable : BindableBase
    {
        public T ShallowClone<T>() where T : new()
        {
            return (T)ShallowClone<T>(new T());
        }

        public K ShallowClone<K>(K newItem)
        {
            return (K)ShallowCloneObject(newItem);
        }

        public static void ShallowCloneObject(object oldItem, object newItem)
        {
            IEnumerable<PropertyInfo> srcFields = oldItem.GetType().GetRuntimeProperties();

            IEnumerable<PropertyInfo> destFields = newItem.GetType().GetRuntimeProperties();

            foreach (var property in srcFields)
            {
                //we're going to clone null values now
                //if (property.GetValue(this, null) != null)
                //{
                var dest = destFields.FirstOrDefault(x => x.Name == property.Name);
                if (dest != null)
                {
                    if (dest.CanWrite)
                        dest.SetValue(newItem, property.GetValue(oldItem, null), null);
                }
                //}
            }
        }

        /// <summary>
        /// It DOES clone null values.
        /// </summary>
        /// <param name="newItem"></param>
        /// <returns></returns>
        public object ShallowCloneObject(object newItem)
        {
            ShallowCloneObject(this, newItem);

            //PropertyInfo[] srcFields = this.GetType().GetProperties(
            //BindingFlags.Instance | BindingFlags.Public);

            //PropertyInfo[] destFields = newItem.GetType().GetProperties(
            //    BindingFlags.Instance | BindingFlags.Public);

            //foreach (var property in srcFields)
            //{
            //    //we're going to clone null values now
            //    //if (property.GetValue(this, null) != null)
            //    //{
            //        var dest = destFields.FirstOrDefault(x => x.Name == property.Name);
            //        if (dest != null)
            //        {
            //            if (dest.CanWrite)
            //                dest.SetValue(newItem, property.GetValue(this, null), null);
            //        }
            //    //}
            //}

            return newItem;
        }
    }

    /// <summary>
    /// Don't use this class. Use the clonable without generics.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    public abstract class Clonable<T> : BindableBase
    {
        public T ShallowClone()
        {
            return (T)this.MemberwiseClone();
        }

        public K ShallowClone<K>(K newItem) where K : T
        {
            IEnumerable<PropertyInfo> srcFields = this.GetType().GetRuntimeProperties();

            IEnumerable<PropertyInfo> destFields = newItem.GetType().GetRuntimeProperties();

            foreach (var property in srcFields)
            {
                var dest = destFields.FirstOrDefault(x => x.Name == property.Name);
                if (dest != null)
                {
                    if (!dest.CanWrite)
                    {
                        throw new Exception("Cannot write destination");
                    }
                    
                    dest.SetValue(newItem, property.GetValue(this, null), null);
                }
            }

            return newItem;
        }
    }
}
