using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Starcounter;
using Concepts.Ring1;

namespace SignIn
{
    public static class AssureHelper
    {
        public static T Assure<T>(String name) where T : Something, new()
        {
            Boolean dummy;
            return Assure<T>(name, out dummy);
        }

        public static T Assure<T>(String name, out Boolean isNew) where T : Something, new()
        {
            string query = string.Format("SELECT a FROM {0} a WHERE a.Name=?",typeof(T).FullName);
            T result = Db.SQL<T>(query, name).First;
            if (result != null)
            {
                isNew = false;
                return result;
            }
            else
            {
                isNew = true;
                result = new T() { Name = name };
                if (result is Something)
                {
                    (result as Something).Name = name;
                }
                return result;
            }
        }

                /// <summary>
        /// For desciption of BuildOn part see private method below. This one only adds authentication to that one.
        /// </summary>
        /// <returns></returns>
        public static dynamic Assure<T>(String name, Action<T> func) where T : Something, new()
        {
            string query = string.Format("SELECT a FROM {0} a WHERE a.Name=?", typeof(T).FullName);
            T result = Db.SQL<T>(query, name).First;
            if (result == null)
            {
                result = new T() { Name = name };
                func(result);
                return result;
            }
            return result;
        }

        public static dynamic AssureTunityUser<T>(String name, Action<T> func) where T : Concepts.Ring8.Tunity.TunityUser, new()
        {
            string query = string.Format("SELECT a FROM {0} a WHERE a.Name=?", typeof(T).FullName);
            T result = Db.SQL<T>(query, name).First;
            if (result == null)
            {
                result = new T() { Name = name };
                func(result);
                return result;
            }
            else if (String.IsNullOrWhiteSpace(result.Password))
            {
                func(result);
                return result;
            }
            return result;
        }

        /// For desciption of BuildOn part see private method below. This one only adds authentication to that one.
        /// </summary>
        /// <returns></returns>
        public static dynamic FullAssure<T>(String name, Action<T> func) where T : Something, new()
        {
            string query = string.Format("SELECT a FROM {0} a WHERE a.Name=?", typeof(T).FullName);
            T result = Db.SQL<T>(query, name).First;
            if (result == null)
            {
                result = new T() { Name = name };
            }
            func(result);
            return result;
        }
    }
}
