using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace WebAppPoliMusicV2.DAL
{
    /// <summary>
    /// Punto unico de acceso a SQL Server para toda la capa DAL.
    ///
    /// La cadena de conexion se lee de Web.config
    /// (connectionStrings/BDD_PoliMusicConnectionString), que es la que ya
    /// existia en el proyecto y apunta a la base BDD_PoliMusic. Ninguna pagina
    /// ni capa superior construye SQL por su cuenta: todo pasa por aqui y
    /// siempre con parametros, para evitar inyeccion SQL.
    /// </summary>
    public static class Db
    {
        public const string ConnectionStringName = "BDD_PoliMusicConnectionString";

        public static string ConnectionString
        {
            get
            {
                ConnectionStringSettings cs = ConfigurationManager.ConnectionStrings[ConnectionStringName];
                if (cs == null || string.IsNullOrEmpty(cs.ConnectionString))
                {
                    throw new ConfigurationErrorsException(
                        "No se encontro la cadena de conexion '" + ConnectionStringName +
                        "' en Web.config. Revise la seccion <connectionStrings>.");
                }
                return cs.ConnectionString;
            }
        }

        /// <summary>Valor de &lt;appSettings&gt; o el valor por defecto indicado.</summary>
        public static string Setting(string key, string fallback)
        {
            string value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrEmpty(value) ? fallback : value;
        }

        public static SqlConnection OpenConnection()
        {
            SqlConnection cn = new SqlConnection(ConnectionString);
            cn.Open();
            return cn;
        }

        public static SqlCommand Command(SqlConnection cn, string sql, SqlParameter[] parameters)
        {
            SqlCommand cmd = new SqlCommand(sql, cn);
            cmd.CommandType = CommandType.Text;
            if (parameters != null && parameters.Length > 0)
            {
                cmd.Parameters.AddRange(parameters);
            }
            return cmd;
        }

        /// <summary>Crea un parametro tratando null como DBNull.Value.</summary>
        public static SqlParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection cn = OpenConnection())
            using (SqlCommand cmd = Command(cn, sql, parameters))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection cn = OpenConnection())
            using (SqlCommand cmd = Command(cn, sql, parameters))
            {
                object result = cmd.ExecuteScalar();
                return result == DBNull.Value ? null : result;
            }
        }

        public static int ExecuteScalarInt(string sql, params SqlParameter[] parameters)
        {
            object result = ExecuteScalar(sql, parameters);
            return result == null ? 0 : Convert.ToInt32(result);
        }

        /// <summary>Ejecuta un INSERT y devuelve el IDENTITY generado.</summary>
        public static int InsertReturningId(string sql, params SqlParameter[] parameters)
        {
            return ExecuteScalarInt(sql + "; SELECT CAST(SCOPE_IDENTITY() AS int);", parameters);
        }

        /// <summary>
        /// Materializa las filas con el proyector indicado. La conexion y el
        /// lector se cierran antes de devolver la lista.
        /// </summary>
        public static List<T> Query<T>(string sql, Func<SqlDataReader, T> projector,
                                       params SqlParameter[] parameters)
        {
            List<T> list = new List<T>();
            using (SqlConnection cn = OpenConnection())
            using (SqlCommand cmd = Command(cn, sql, parameters))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(projector(reader));
                }
            }
            return list;
        }

        /// <summary>Primera fila o null.</summary>
        public static T QuerySingle<T>(string sql, Func<SqlDataReader, T> projector,
                                       params SqlParameter[] parameters) where T : class
        {
            using (SqlConnection cn = OpenConnection())
            using (SqlCommand cmd = Command(cn, sql, parameters))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    return projector(reader);
                }
                return null;
            }
        }

        /// <summary>
        /// Ejecuta varias sentencias dentro de una transaccion. Se usa para
        /// borrados en cascada sobre tablas intermedias.
        /// </summary>
        public static void ExecuteTransaction(params string[] statements)
        {
            ExecuteTransaction(null, statements);
        }

        public static void ExecuteTransaction(SqlParameter[] parameters, params string[] statements)
        {
            if (statements == null || statements.Length == 0)
            {
                return;
            }
            using (SqlConnection cn = OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    foreach (string sql in statements)
                    {
                        using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
                        {
                            if (parameters != null)
                            {
                                foreach (SqlParameter p in parameters)
                                {
                                    cmd.Parameters.Add(new SqlParameter(p.ParameterName, p.Value ?? DBNull.Value));
                                }
                            }
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
                catch
                {
                    try { tx.Rollback(); }
                    catch (Exception) { /* la transaccion ya estaba cerrada */ }
                    throw;
                }
            }
        }

        // ------------------------------------------------------- lectura ---

        public static int ToInt(SqlDataReader r, string column)
        {
            int i = r.GetOrdinal(column);
            return r.IsDBNull(i) ? 0 : Convert.ToInt32(r.GetValue(i));
        }

        public static int? ToNullableInt(SqlDataReader r, string column)
        {
            int i = r.GetOrdinal(column);
            return r.IsDBNull(i) ? (int?)null : Convert.ToInt32(r.GetValue(i));
        }

        public static string ToStr(SqlDataReader r, string column)
        {
            int i = r.GetOrdinal(column);
            return r.IsDBNull(i) ? null : Convert.ToString(r.GetValue(i));
        }

        public static DateTime ToDate(SqlDataReader r, string column)
        {
            int i = r.GetOrdinal(column);
            return r.IsDBNull(i) ? DateTime.MinValue : Convert.ToDateTime(r.GetValue(i));
        }

        public static DateTime? ToNullableDate(SqlDataReader r, string column)
        {
            int i = r.GetOrdinal(column);
            return r.IsDBNull(i) ? (DateTime?)null : Convert.ToDateTime(r.GetValue(i));
        }
    }
}
