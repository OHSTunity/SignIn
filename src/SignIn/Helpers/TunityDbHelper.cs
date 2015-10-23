using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Starcounter;
using Concepts.Ring1;
using Concepts.Ring8.Tunity;
using System.Web;

namespace SignIn
{
    public static class TunityDbHelper
    {
        /// <summary>
        ///  A more "forgiving" function than DBHelper.fromIDString
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objId"></param>
        /// <returns></returns>
        public static T FromIDString<T>(String objectID) where T : class
        {
            try
            {
                return Db.SQL("SELECT a FROM Concepts.Ring1.Something a WHERE a.ObjectID=?",objectID).First as T;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Returns all instances of Type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> AllInstances<T>() where T: Something
        {
            string query = string.Format("SELECT a FROM {0} a", typeof(T).FullName);
            return Db.SQL<T>(query);
        }

        /// <summary>
        /// Returns all instances of Type T with ObjectState os
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> AllInstances<T>(ObjectState os) where T : Something, IObjectState
        {
            string query = string.Format("SELECT a FROM {0} a WHERE a.ObjectState=?", typeof(T).FullName);
            return Db.SQL<T>(query, os);
        }

        /// <summary>
        /// Returns all instances of Type T with Role R
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> AllInstances<T, R>() where T : Something where R : Role 
        {
            string query = string.Format("SELECT a FROM {0} a JOIN {1} b ON b.Value=a",
                typeof(T).FullName, typeof(R).FullName);
            return Db.SQL<T>(query);
        }
        
        /// <summary>
        ///  Return true if relation exist
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whatIs"></param>
        /// <param name="toWhat"></param>
        /// <returns></returns>
        public static Boolean HaveRelation<T>(Something whatIs, Something toWhat) where T: Relation
        {
            string query = string.Format("SELECT a FROM {0} a WHERE a.WhatIs=? AND a.ToWhat=?", typeof(T).FullName);
            return Db.SQL<T>(query, whatIs, toWhat).First != null;
        }

        /// <summary>
        /// Returns all latest versions of VersionSomething objects, Type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetLatestVersions<T>()
            where T : VersionedSomething
        {
            string query = string.Format("SELECT a FROM {0} a WHERE a.Latest=true",
                typeof(T).FullName);
            return Db.SQL<T>(query);
        }


        public static T FromName<T>(String name) where T : class
        {
            string query = string.Format("SELECT a FROM {0} a WHERE a.Name=?", typeof(T).FullName);
            return Db.SQL<T>(query, name).First;
        }
        
        public static T FromDescription<T>(String name) where T : class
        {
            string query = string.Format("SELECT a FROM {0} a WHERE a.Description=?", typeof(T).FullName);
            return Db.SQL<T>(query, name).First;
        }

        public static IEnumerable<T> Empty<T>()
        {
            return new List<T>();
        }

    }
}
