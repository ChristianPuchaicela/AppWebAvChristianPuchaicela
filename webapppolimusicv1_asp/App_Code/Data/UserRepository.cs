using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using WebAppPoliMusicV1.Models;

namespace WebAppPoliMusicV1.Data
{
    /// <summary>Acceso a datos de [dbo].[TBL_USER]. Consultas siempre parametrizadas.</summary>
    public static class UserRepository
    {
        private const string SelectColumns =
            "SELECT ID_USER, USERNAME, PASSWORD, EMAIL, BIRTHDAY, USER_PHOTO, USER_TYPE FROM TBL_USER ";

        private static User Map(SqlDataReader r)
        {
            return new User
            {
                IdUser = Db.ToInt(r, "ID_USER"),
                Username = Db.ToStr(r, "USERNAME"),
                Password = Db.ToStr(r, "PASSWORD"),
                Email = Db.ToStr(r, "EMAIL"),
                Birthday = Db.ToNullableDate(r, "BIRTHDAY"),
                UserPhoto = Db.ToStr(r, "USER_PHOTO"),
                UserType = Db.ToInt(r, "USER_TYPE")
            };
        }

        public static List<User> GetAll()
        {
            return Db.Query(SelectColumns + "ORDER BY USERNAME", Map);
        }

        public static User GetById(int id)
        {
            return Db.QuerySingle(SelectColumns + "WHERE ID_USER = @id",
                Map, Db.P("@id", id));
        }

        /// <summary>Busca por nombre de usuario (el login tambien permite email).</summary>
        public static User GetByUsernameOrEmail(string login)
        {
            return Db.QuerySingle(
                SelectColumns + "WHERE USERNAME = @login OR EMAIL = @login",
                Map, Db.P("@login", login));
        }

        public static User GetByUsername(string username)
        {
            return Db.QuerySingle(SelectColumns + "WHERE USERNAME = @u", Map, Db.P("@u", username));
        }

        public static User GetByEmail(string email)
        {
            return Db.QuerySingle(SelectColumns + "WHERE EMAIL = @e", Map, Db.P("@e", email));
        }

        public static bool UsernameExists(string username, int excludeId)
        {
            object o = Db.ExecuteScalar(
                "SELECT COUNT(1) FROM TBL_USER WHERE USERNAME = @u AND ID_USER <> @id",
                Db.P("@u", username), Db.P("@id", excludeId));
            return Convert.ToInt32(o) > 0;
        }

        public static bool EmailExists(string email, int excludeId)
        {
            object o = Db.ExecuteScalar(
                "SELECT COUNT(1) FROM TBL_USER WHERE EMAIL = @e AND ID_USER <> @id",
                Db.P("@e", email), Db.P("@id", excludeId));
            return Convert.ToInt32(o) > 0;
        }

        public static int Create(string username, string passwordHash, string email,
                                 DateTime? birthday, string photo, int userType)
        {
            return Db.InsertReturningId(
                @"INSERT INTO TBL_USER (USERNAME, PASSWORD, EMAIL, BIRTHDAY, USER_PHOTO, USER_TYPE)
                  VALUES (@u, @p, @e, @b, @ph, @t)",
                Db.P("@u", username), Db.P("@p", passwordHash), Db.P("@e", email),
                Db.P("@b", birthday.HasValue ? (object)birthday.Value : null),
                Db.P("@ph", photo), Db.P("@t", userType));
        }

        public static int Update(int id, string username, string email,
                                 DateTime? birthday, string photo, int userType)
        {
            return Db.ExecuteNonQuery(
                @"UPDATE TBL_USER
                     SET USERNAME = @u, EMAIL = @e, BIRTHDAY = @b, USER_PHOTO = @ph, USER_TYPE = @t
                   WHERE ID_USER = @id",
                Db.P("@u", username), Db.P("@e", email),
                Db.P("@b", birthday.HasValue ? (object)birthday.Value : null),
                Db.P("@ph", photo), Db.P("@t", userType), Db.P("@id", id));
        }

        /// <summary>Cambia unicamente la contrasena (ya cifrada).</summary>
        public static int UpdatePassword(int id, string passwordHash)
        {
            return Db.ExecuteNonQuery(
                "UPDATE TBL_USER SET PASSWORD = @p WHERE ID_USER = @id",
                Db.P("@p", passwordHash), Db.P("@id", id));
        }

        public static int UpdatePhoto(int id, string photo)
        {
            return Db.ExecuteNonQuery(
                "UPDATE TBL_USER SET USER_PHOTO = @ph WHERE ID_USER = @id",
                Db.P("@ph", photo), Db.P("@id", id));
        }

        /// <summary>Cantidad de playlists que dependen del usuario (FK_TBL_PLAYLIST_TBL_USER).</summary>
        public static int CountPlaylists(int userId)
        {
            object o = Db.ExecuteScalar(
                "SELECT COUNT(1) FROM TBL_PLAYLIST WHERE ID_USER = @id", Db.P("@id", userId));
            return Convert.ToInt32(o);
        }

        public static int CountAdmins()
        {
            object o = Db.ExecuteScalar(
                "SELECT COUNT(1) FROM TBL_USER WHERE USER_TYPE = @t", Db.P("@t", UserRoles.Admin));
            return Convert.ToInt32(o);
        }

        public static int Delete(int id)
        {
            return Db.ExecuteNonQuery("DELETE FROM TBL_USER WHERE ID_USER = @id", Db.P("@id", id));
        }
    }
}
