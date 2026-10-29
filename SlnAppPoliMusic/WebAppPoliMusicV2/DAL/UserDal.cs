using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using WebAppPoliMusicV2.Entities;

namespace WebAppPoliMusicV2.DAL
{
    /// <summary>
    /// Acceso a datos de [dbo].[TBL_USER]. Todas las consultas estan
    /// parametrizadas. El rol (USER_TYPE) se lee siempre de la base y nunca
    /// se acepta como dato del formulario.
    /// </summary>
    public class UserDal
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

        public List<User> Read()
        {
            return Db.Query(SelectColumns + "ORDER BY USERNAME", Map);
        }

        public User ReadById(int idUser)
        {
            return Db.QuerySingle(SelectColumns + "WHERE ID_USER = @id", Map, Db.P("@id", idUser));
        }

        /// <summary>El inicio de sesion admite nombre de usuario o correo.</summary>
        public User ReadByLogin(string login)
        {
            return Db.QuerySingle(SelectColumns + "WHERE USERNAME = @login OR EMAIL = @login",
                Map, Db.P("@login", login));
        }

        public bool UsernameExists(string username, int excludeId)
        {
            return Db.ExecuteScalarInt(
                "SELECT COUNT(1) FROM TBL_USER WHERE USERNAME = @u AND ID_USER <> @id",
                Db.P("@u", username), Db.P("@id", excludeId)) > 0;
        }

        public bool EmailExists(string email, int excludeId)
        {
            return Db.ExecuteScalarInt(
                "SELECT COUNT(1) FROM TBL_USER WHERE EMAIL = @e AND ID_USER <> @id",
                Db.P("@e", email), Db.P("@id", excludeId)) > 0;
        }

        public int Create(User user)
        {
            return Db.InsertReturningId(
                @"INSERT INTO TBL_USER (USERNAME, PASSWORD, EMAIL, BIRTHDAY, USER_PHOTO, USER_TYPE)
                  VALUES (@u, @p, @e, @b, @ph, @t)",
                Db.P("@u", user.Username),
                Db.P("@p", user.Password),
                Db.P("@e", user.Email),
                Db.P("@b", user.Birthday),
                Db.P("@ph", user.UserPhoto),
                Db.P("@t", user.UserType));
        }

        public int Update(User user)
        {
            return Db.ExecuteNonQuery(
                @"UPDATE TBL_USER
                     SET USERNAME = @u, EMAIL = @e, BIRTHDAY = @b, USER_PHOTO = @ph, USER_TYPE = @t
                   WHERE ID_USER = @id",
                Db.P("@u", user.Username),
                Db.P("@e", user.Email),
                Db.P("@b", user.Birthday),
                Db.P("@ph", user.UserPhoto),
                Db.P("@t", user.UserType),
                Db.P("@id", user.IdUser));
        }

        /// <summary>Cambia solo la contrasena (ya cifrada).</summary>
        public int UpdatePassword(int idUser, string storedPassword)
        {
            return Db.ExecuteNonQuery(
                "UPDATE TBL_USER SET PASSWORD = @p WHERE ID_USER = @id",
                Db.P("@p", storedPassword), Db.P("@id", idUser));
        }

        public int UpdatePhoto(int idUser, string photo)
        {
            return Db.ExecuteNonQuery(
                "UPDATE TBL_USER SET USER_PHOTO = @ph WHERE ID_USER = @id",
                Db.P("@ph", photo), Db.P("@id", idUser));
        }

        public int Delete(int idUser)
        {
            return Db.ExecuteNonQuery("DELETE FROM TBL_USER WHERE ID_USER = @id", Db.P("@id", idUser));
        }

        public int CountAll()
        {
            return Db.ExecuteScalarInt("SELECT COUNT(1) FROM TBL_USER");
        }

        public int CountAdmins()
        {
            return Db.ExecuteScalarInt("SELECT COUNT(1) FROM TBL_USER WHERE USER_TYPE = @t",
                Db.P("@t", Util.Constants.ID_USER_ADMIN));
        }

        /// <summary>Playlists que dependen del usuario (FK_TBL_PLAYLIST_TBL_USER).</summary>
        public int CountPlaylists(int idUser)
        {
            return Db.ExecuteScalarInt("SELECT COUNT(1) FROM TBL_PLAYLIST WHERE ID_USER = @id",
                Db.P("@id", idUser));
        }
    }
}
