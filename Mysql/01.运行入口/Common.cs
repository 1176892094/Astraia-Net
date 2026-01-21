// *********************************************************************************
// # Project: SQLServer
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-11-25 04:11:05
// # Recently: 2025-02-11 00:02:29
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Astraia.Net
{
    internal static class Common
    {
        public static int Insert<T>(Command proxy, T entity)
        {
            var parameters = new Dictionary<string, object>();
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                if (property.GetCustomAttribute<ColumnAttribute>() != null)
                {
                    parameters.Add(property.Name, property.GetValue(entity));
                }
            }

            return Insert<T>(proxy, parameters);
        }

        public static int Update<T>(Command proxy, T entity)
        {
            var parameter = new KeyValuePair<string, object>();
            var parameters = new Dictionary<string, object>();
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                if (property.GetCustomAttribute<ColumnAttribute>() != null)
                {
                    if (property.GetCustomAttribute<KeyAttribute>() != null)
                    {
                        parameter = new KeyValuePair<string, object>(property.Name, property.GetValue(entity));
                        continue;
                    }

                    parameters.Add(property.Name, property.GetValue(entity));
                }
            }

            return Update<T>(proxy, parameter, parameters);
        }

        public static int Delete<T>(Command proxy, object primaryValue)
        {
            var primaryKey = string.Empty;
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                if (property.GetCustomAttribute<KeyAttribute>() != null)
                {
                    if (property.GetCustomAttribute<ColumnAttribute>() != null)
                    {
                        primaryKey = property.Name;
                    }

                    break;
                }
            }

            var query = "DELETE FROM {0} WHERE {1} = @{1}".Format(typeof(T).Name, primaryKey);
            var parameters = new Dictionary<string, object>
            {
                { "@{0}".Format(primaryKey), primaryValue }
            };

            return proxy.ExecuteNonQuery(query, parameters);
        }

        public static List<T> Select<T>(Command proxy, string clause = "", Dictionary<string, object> parameters = null) where T : new()
        {
            var properties = typeof(T).GetProperties();

            var columns = new HashSet<string>();
            foreach (var property in properties)
            {
                if (property.GetCustomAttribute<ColumnAttribute>() != null)
                {
                    columns.Add(property.Name);
                }
            }

            var query = "SELECT {0} FROM {1}".Format(string.Join(", ", columns), typeof(T).Name);
            if (!string.IsNullOrEmpty(clause))
            {
                query += " WHERE {0}".Format(clause);
            }

            var results = new List<T>();
            var dataTable = proxy.ExecuteQuery(query, parameters);

            foreach (DataRow row in dataTable.Rows)
            {
                var entity = new T();
                foreach (var property in properties)
                {
                    if (property.GetCustomAttribute<ColumnAttribute>() != null)
                    {
                        var value = row[property.Name];
                        property.SetValue(entity, value != DBNull.Value ? value : null);
                    }
                }

                results.Add(entity);
            }

            return results;
        }

        public static int Insert<T>(Command proxy, Dictionary<string, object> parameters)
        {
            var inserts = parameters.Keys.Select(field => "@{0}".Format(field)).ToList();
            var query = "INSERT INTO {0} ({1}) VALUES ({2})".Format(typeof(T).Name, string.Join(", ", parameters.Keys), string.Join(", ", inserts));
            parameters = parameters.ToDictionary(pair => "@{0}".Format(pair.Key), pair => pair.Value);
            return proxy.ExecuteNonQuery(query, parameters);
        }

        public static int Update<T>(Command proxy, KeyValuePair<string, object> parameter, Dictionary<string, object> parameters)
        {
            var updates = parameters.Keys.Select(field => "{0} = @{0}".Format(field)).ToList();
            var query = "UPDATE {0} SET {1} WHERE {2} = @{2}".Format(typeof(T).Name, string.Join(", ", updates), parameter.Key);
            parameters.Add(parameter.Key, parameter.Value);
            parameters = parameters.ToDictionary(pair => "@{0}".Format(pair.Key), pair => pair.Value);
            return proxy.ExecuteNonQuery(query, parameters);
        }
    }
}