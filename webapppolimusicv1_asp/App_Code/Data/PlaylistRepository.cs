using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using WebAppPoliMusicV1.Models;

namespace WebAppPoliMusicV1.Data
{
    /// <summary>
    /// Acceso a datos de [dbo].[TBL_PLAYLIST] y su tabla intermedia
    /// [dbo].[TBL_PLAYLIST_SONG]. Cada metodo que recibe un ID_PLAYLIST
    /// puede filtrar por ID_USER para respetar la propiedad del dato.
    /// </summary>
    public static class PlaylistRepository
    {
        private const string SelectBase =
            @"SELECT p.ID_PLAYLIST, p.ID_USER, p.TITLE, p.CREATION_DATE, p.TYPE, p.PHOTO,
                     u.USERNAME AS OWNER,
                     (SELECT COUNT(1) FROM TBL_PLAYLIST_SONG ps WHERE ps.ID_PLAYLIST = p.ID_PLAYLIST) AS SONG_COUNT
                FROM TBL_PLAYLIST p
                INNER JOIN TBL_USER u ON u.ID_USER = p.ID_USER ";

        private static Playlist Map(SqlDataReader r)
        {
            return new Playlist
            {
                IdPlaylist = Db.ToInt(r, "ID_PLAYLIST"),
                IdUser = Db.ToInt(r, "ID_USER"),
                Title = Db.ToStr(r, "TITLE"),
                CreationDate = Db.ToDate(r, "CREATION_DATE"),
                Type = Db.ToInt(r, "TYPE"),
                Photo = Db.ToStr(r, "PHOTO"),
                OwnerUsername = Db.ToStr(r, "OWNER"),
                SongCount = Db.ToInt(r, "SONG_COUNT")
            };
        }

        /// <summary>Todas las playlists (vista de administrador).</summary>
        public static List<Playlist> GetAll()
        {
            return Db.Query(SelectBase + "ORDER BY p.CREATION_DATE DESC, p.TITLE", Map);
        }

        /// <summary>
        /// Playlists visibles para un usuario: las propias completas mas las
        /// publicas de otros usuarios (TYPE = 0). Sujeto a ownership en el servidor.
        /// </summary>
        public static List<Playlist> GetVisibleFor(int userId)
        {
            return Db.Query(
                SelectBase +
                "WHERE p.ID_USER = @uid OR p.TYPE = @pub " +
                "ORDER BY p.CREATION_DATE DESC, p.TITLE",
                Map, Db.P("@uid", userId), Db.P("@pub", PlaylistTypes.Public));
        }

        /// <summary>Playlists propias de un usuario (todas, publicas y privadas).</summary>
        public static List<Playlist> GetOwnedBy(int userId)
        {
            return Db.Query(
                SelectBase + "WHERE p.ID_USER = @uid ORDER BY p.CREATION_DATE DESC, p.TITLE",
                Map, Db.P("@uid", userId));
        }

        /// <summary>Playlists publicas de un usuario (usado desde el perfil).</summary>
        public static List<Playlist> GetPublicBy(int userId)
        {
            return Db.Query(
                SelectBase + "WHERE p.ID_USER = @uid AND p.TYPE = @pub ORDER BY p.CREATION_DATE DESC",
                Map, Db.P("@uid", userId), Db.P("@pub", PlaylistTypes.Public));
        }

        /// <summary>
        /// Devuelve la playlist solo si pertenece al usuario indicado.
        /// Es la base del control de propiedad: nunca se confia en el ID de la URL.
        /// </summary>
        public static Playlist GetOwned(int playlistId, int userId)
        {
            return Db.QuerySingle(
                SelectBase + "WHERE p.ID_PLAYLIST = @pid AND p.ID_USER = @uid",
                Map, Db.P("@pid", playlistId), Db.P("@uid", userId));
        }

        /// <summary>Devuelve la playlist si es propia o si es publica (lectura permitida).</summary>
        public static Playlist GetReadable(int playlistId, int userId)
        {
            return Db.QuerySingle(
                SelectBase + "WHERE p.ID_PLAYLIST = @pid AND (p.ID_USER = @uid OR p.TYPE = @pub)",
                Map, Db.P("@pid", playlistId), Db.P("@uid", userId), Db.P("@pub", PlaylistTypes.Public));
        }

        /// <summary>Sin filtro de propiedad: solo para el panel de administracion.</summary>
        public static Playlist GetById(int playlistId)
        {
            return Db.QuerySingle(SelectBase + "WHERE p.ID_PLAYLIST = @pid", Map, Db.P("@pid", playlistId));
        }

        public static int Create(int userId, string title, DateTime creationDate, int type, string photo)
        {
            return Db.InsertReturningId(
                @"INSERT INTO TBL_PLAYLIST (ID_USER, TITLE, CREATION_DATE, TYPE, PHOTO)
                  VALUES (@u, @t, @d, @ty, @ph)",
                Db.P("@u", userId), Db.P("@t", title), Db.P("@d", creationDate),
                Db.P("@ty", type), Db.P("@ph", photo));
        }

        public static int Update(int playlistId, string title, int type, string photo)
        {
            return Db.ExecuteNonQuery(
                "UPDATE TBL_PLAYLIST SET TITLE=@t, TYPE=@ty, PHOTO=@ph WHERE ID_PLAYLIST=@id",
                Db.P("@t", title), Db.P("@ty", type), Db.P("@ph", photo), Db.P("@id", playlistId));
        }

        /// <summary>Actualiza solo si la playlist pertenece al usuario (control de propiedad).</summary>
        public static int UpdateOwned(int playlistId, int userId, string title, int type, string photo)
        {
            return Db.ExecuteNonQuery(
                "UPDATE TBL_PLAYLIST SET TITLE=@t, TYPE=@ty, PHOTO=@ph WHERE ID_PLAYLIST=@id AND ID_USER=@u",
                Db.P("@t", title), Db.P("@ty", type), Db.P("@ph", photo),
                Db.P("@id", playlistId), Db.P("@u", userId));
        }

        /// <summary>Elimina la playlist y sus relaciones en TBL_PLAYLIST_SONG.</summary>
        public static void DeleteCascade(int playlistId)
        {
            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    using (SqlCommand c1 = Db.Command(cn, "DELETE FROM TBL_PLAYLIST_SONG WHERE ID_PLAYLIST = @id", Db.P("@id", playlistId)))
                    { c1.Transaction = tx; c1.ExecuteNonQuery(); }
                    using (SqlCommand c2 = Db.Command(cn, "DELETE FROM TBL_PLAYLIST WHERE ID_PLAYLIST = @id", Db.P("@id", playlistId)))
                    { c2.Transaction = tx; c2.ExecuteNonQuery(); }
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        /// <summary>Elimina solo si es del usuario; devuelve true si elimino algo.</summary>
        public static bool DeleteOwned(int playlistId, int userId)
        {
            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    int affected;
                    using (SqlCommand c2 = Db.Command(cn,
                        "DELETE FROM TBL_PLAYLIST WHERE ID_PLAYLIST = @id AND ID_USER = @u",
                        Db.P("@id", playlistId), Db.P("@u", userId)))
                    {
                        c2.Transaction = tx;
                        affected = c2.ExecuteNonQuery();
                    }
                    if (affected == 0)
                    {
                        tx.Rollback();
                        return false;
                    }
                    using (SqlCommand c1 = Db.Command(cn, "DELETE FROM TBL_PLAYLIST_SONG WHERE ID_PLAYLIST = @id", Db.P("@id", playlistId)))
                    { c1.Transaction = tx; c1.ExecuteNonQuery(); }
                    tx.Commit();
                    return true;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public static bool ContainsSong(int playlistId, int songId)
        {
            object o = Db.ExecuteScalar(
                "SELECT COUNT(1) FROM TBL_PLAYLIST_SONG WHERE ID_PLAYLIST=@p AND ID_SONG=@s",
                Db.P("@p", playlistId), Db.P("@s", songId));
            return Convert.ToInt32(o) > 0;
        }

        public static int AddSong(int playlistId, int songId)
        {
            return Db.ExecuteNonQuery(
                "IF NOT EXISTS (SELECT 1 FROM TBL_PLAYLIST_SONG WHERE ID_PLAYLIST=@p AND ID_SONG=@s) " +
                "INSERT INTO TBL_PLAYLIST_SONG (ID_PLAYLIST, ID_SONG) VALUES (@p, @s)",
                Db.P("@p", playlistId), Db.P("@s", songId));
        }

        public static int RemoveSong(int playlistId, int songId)
        {
            return Db.ExecuteNonQuery(
                "DELETE FROM TBL_PLAYLIST_SONG WHERE ID_PLAYLIST=@p AND ID_SONG=@s",
                Db.P("@p", playlistId), Db.P("@s", songId));
        }

        public static int CountAll()
        {
            object o = Db.ExecuteScalar("SELECT COUNT(1) FROM TBL_PLAYLIST");
            return Convert.ToInt32(o);
        }
    }
}
