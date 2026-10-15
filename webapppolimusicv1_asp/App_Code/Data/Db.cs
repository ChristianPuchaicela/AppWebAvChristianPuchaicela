using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace WebAppPoliMusicV1.Data
{
    /// <summary>
    /// Punto unico de acceso a SQL Server.
    /// La cadena de conexion vive en Web.config (connectionStrings/PoliMusicConnection)
    /// y nunca se repite ni se expone en las paginas.
    /// </summary>
    public static class Db
    {
        public static string ConnectionString
        {
            get
            {
                ConnectionStringSettings cs = ConfigurationManager.ConnectionStrings["PoliMusicConnection"];
                if (cs == null || string.IsNullOrEmpty(cs.ConnectionString))
                {
                    throw new ConfigurationErrorsException(
                        "No se encontro la cadena de conexion 'PoliMusicConnection' en Web.config.");
                }
                return cs.ConnectionString;
            }
        }

        /// <summary>Configuracion auxiliar de la seccion &lt;polimusic&gt;.</summary>
        public static string Setting(string key, string fallback)
        {
            try
            {
                var section = ConfigurationManager.GetSection("polimusic") as System.Collections.Specialized.NameValueCollection;
                if (section != null && !string.IsNullOrEmpty(section[key]))
                {
                    return section[key];
                }
            }
            catch (Exception)
            {
                // Seccion ausente o mal formada: se usa el valor por defecto.
            }
            return fallback;
        }

        public static SqlConnection OpenConnection()
        {
            SqlConnection cn = new SqlConnection(ConnectionString);
            cn.Open();
            return cn;
        }

        /// <summary>
        /// Crea un comando con parametros. Toda consulta de la aplicacion pasa
        /// por aqui para garantizar el uso de parametros y evitar inyeccion SQL.
        /// </summary>
        public static SqlCommand Command(SqlConnection cn, string sql, params SqlParameter[] parameters)
        {
            SqlCommand cmd = new SqlCommand(sql, cn);
            cmd.CommandType = CommandType.Text;
            if (parameters != null && parameters.Length > 0)
            {
                cmd.Parameters.AddRange(parameters);
            }
            return cmd;
        }

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

        /// <summary>
        /// Ejecuta el lector y materializa las filas mediante el proyector indicado,
        /// de modo que la conexion se cierre siempre antes de devolver el resultado.
        /// </summary>
        public static System.Collections.Generic.List<T> Query<T>(
            string sql, Func<SqlDataReader, T> projector, params SqlParameter[] parameters)
        {
            var list = new System.Collections.Generic.List<T>();
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

        /// <summary>Devuelve la primera fila o null.</summary>
        public static T QuerySingle<T>(
            string sql, Func<SqlDataReader, T> projector, params SqlParameter[] parameters) where T : class
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
        /// Ejecuta un INSERT y devuelve la identidad generada.
        /// </summary>
        public static int InsertReturningId(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection cn = OpenConnection())
            using (SqlCommand cmd = Command(cn, sql + "; SELECT CAST(SCOPE_IDENTITY() AS int);", parameters))
            {
                object id = cmd.ExecuteScalar();
                return id == null || id == DBNull.Value ? 0 : Convert.ToInt32(id);
            }
        }

        public static int ToInt(SqlDataReader r, string column)
        {
            int i = r.GetOrdinal(column);
            return r.IsDBNull(i) ? 0 : r.GetInt32(i);
        }

        public static int? ToNullableInt(SqlDataReader r, string column)
        {
            int i = r.GetOrdinal(column);
            return r.IsDBNull(i) ? (int?)null : r.GetInt32(i);
        }

        public static string ToStr(SqlDataReader r, string column)
        {
            int i = r.GetOrdinal(column);
            return r.IsDBNull(i) ? null : r.GetString(i);
        }

        public static DateTime ToDate(SqlDataReader r, string column)
        {
            int i = r.GetOrdinal(column);
            return r.IsDBNull(i) ? DateTime.MinValue : r.GetDateTime(i);
        }

        public static DateTime? ToNullableDate(SqlDataReader r, string column)
        {
            int i = r.GetOrdinal(column);
            return r.IsDBNull(i) ? (DateTime?)null : r.GetDateTime(i);
        }
    }
}
