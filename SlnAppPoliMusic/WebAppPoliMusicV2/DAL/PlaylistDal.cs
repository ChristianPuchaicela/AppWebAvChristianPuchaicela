using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using WebAppPoliMusicV2.Entities;

namespace WebAppPoliMusicV2.DAL
{
    /// <summary>
    /// Acceso a datos de [dbo].[TBL_PLAYLIST] y de la tabla intermedia
    /// [dbo].[TBL_PLAYLIST_SONG]. Cada metodo que recibe un ID_PLAYLIST puede
    /// filtrar por ID_USER para respetar la propiedad del dato.
    /// </summary>
    public class PlaylistDal
    {
        private const string SelectBase =
            @"SELECT pl.ID_PLAYLIST, pl.ID_USER, pl.TITLE, pl.CREATION_DATE, pl.TYPE, pl.PHOTO,
                     u.USERNAME AS OWNER,
                     (SELECT COUNT(1) FROM TBL_PLAYLIST_SONG ps WHERE ps.ID_PLAYLIST = pl.ID_PLAYLIST) AS SONG_COUNT
                FROM TBL_PLAYLIST pl
                INNER JOIN TBL_USER u ON u.ID_USER = pl.ID_USER ";

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

        /// <summary>Todas las playlists (vista de administracion).</summary>
        public List<Playlist> Read()
        {
            return Db.Query(SelectBase + "ORDER BY pl.CREATION_DATE DESC, pl.TITLE", Map);
        }

        /// <summary>
        /// Playlists visibles para un usuario: las propias (publicas o
        /// privadas) mas las publicas de los demas.
        /// </summary>
        public List<Playlist> ReadVisibleFor(int idUser)
        {
            return Db.Query(
                SelectBase + "WHERE pl.ID_USER = @uid OR pl.TYPE = @pub " +
                "ORDER BY pl.CREATION_DATE DESC, pl.TITLE",
                Map, Db.P("@uid", idUser), Db.P("@pub", Util.Constants.ID_PLAYLIST_PUBLIC));
        }

        public List<Playlist> ReadOwnedBy(int idUser)
        {
            return Db.Query(
                SelectBase + "WHERE pl.ID_USER = @uid ORDER BY pl.CREATION_DATE DESC, pl.TITLE",
                Map, Db.P("@uid", idUser));
        }

        /// <summary>Playlist solo si pertenece al usuario indicado.</summary>
        public Playlist ReadOwned(int idPlaylist, int idUser)
        {
            return Db.QuerySingle(
                SelectBase + "WHERE pl.ID_PLAYLIST = @pid AND pl.ID_USER = @uid",
                Map, Db.P("@pid", idPlaylist), Db.P("@uid", idUser));
        }

        /// <summary>Playlist propia o publica (lectura permitida).</summary>
        public Playlist ReadReadable(int idPlaylist, int idUser)
        {
            return Db.QuerySingle(
                SelectBase + "WHERE pl.ID_PLAYLIST = @pid AND (pl.ID_USER = @uid OR pl.TYPE = @pub)",
                Map, Db.P("@pid", idPlaylist), Db.P("@uid", idUser),
                Db.P("@pub", Util.Constants.ID_PLAYLIST_PUBLIC));
        }

        /// <summary>Sin filtro de propiedad: solo para el panel de administracion.</summary>
        public Playlist ReadById(int idPlaylist)
        {
            return Db.QuerySingle(SelectBase + "WHERE pl.ID_PLAYLIST = @pid",
                Map, Db.P("@pid", idPlaylist));
        }

        public int Create(int idUser, string title, DateTime creationDate, int type, string photo)
        {
            return Db.InsertReturningId(
                @"INSERT INTO TBL_PLAYLIST (ID_USER, TITLE, CREATION_DATE, TYPE, PHOTO)
                  VALUES (@u, @t, @d, @ty, @ph)",
                Db.P("@u", idUser), Db.P("@t", title), Db.P("@d", creationDate),
                Db.P("@ty", type), Db.P("@ph", photo));
        }

        public int Update(int idPlaylist, string title, int type, string photo)
        {
            return Db.ExecuteNonQuery(
                "UPDATE TBL_PLAYLIST SET TITLE = @t, TYPE = @ty, PHOTO = @ph WHERE ID_PLAYLIST = @id",
                Db.P("@t", title), Db.P("@ty", type), Db.P("@ph", photo), Db.P("@id", idPlaylist));
        }

        /// <summary>Actualiza solo si la playlist pertenece al usuario.</summary>
        public int UpdateOwned(int idPlaylist, int idUser, string title, int type, string photo)
        {
            return Db.ExecuteNonQuery(
                "UPDATE TBL_PLAYLIST SET TITLE = @t, TYPE = @ty, PHOTO = @ph " +
                "WHERE ID_PLAYLIST = @id AND ID_USER = @u",
                Db.P("@t", title), Db.P("@ty", type), Db.P("@ph", photo),
                Db.P("@id", idPlaylist), Db.P("@u", idUser));
        }

        public void DeleteCascade(int idPlaylist)
        {
            Db.ExecuteTransaction(
                new[] { Db.P("@id", idPlaylist) },
                "DELETE FROM TBL_PLAYLIST_SONG WHERE ID_PLAYLIST = @id",
                "DELETE FROM TBL_PLAYLIST WHERE ID_PLAYLIST = @id");
        }

        /// <summary>Elimina solo si es del usuario; devuelve false si no borro nada.</summary>
        public bool DeleteOwned(int idPlaylist, int idUser)
        {
            Playlist owned = ReadOwned(idPlaylist, idUser);
            if (owned == null)
            {
                return false;
            }
            DeleteCascade(idPlaylist);
            return true;
        }

        public bool ContainsSong(int idPlaylist, int idSong)
        {
            return Db.ExecuteScalarInt(
                "SELECT COUNT(1) FROM TBL_PLAYLIST_SONG WHERE ID_PLAYLIST = @p AND ID_SONG = @s",
                Db.P("@p", idPlaylist), Db.P("@s", idSong)) > 0;
        }

        public int AddSong(int idPlaylist, int idSong)
        {
            return Db.ExecuteNonQuery(
                "IF NOT EXISTS (SELECT 1 FROM TBL_PLAYLIST_SONG WHERE ID_PLAYLIST = @p AND ID_SONG = @s) " +
                "INSERT INTO TBL_PLAYLIST_SONG (ID_PLAYLIST, ID_SONG) VALUES (@p, @s)",
                Db.P("@p", idPlaylist), Db.P("@s", idSong));
        }

        public int RemoveSong(int idPlaylist, int idSong)
        {
            return Db.ExecuteNonQuery(
                "DELETE FROM TBL_PLAYLIST_SONG WHERE ID_PLAYLIST = @p AND ID_SONG = @s",
                Db.P("@p", idPlaylist), Db.P("@s", idSong));
        }

        public int CountAll()
        {
            return Db.ExecuteScalarInt("SELECT COUNT(1) FROM TBL_PLAYLIST");
        }
    }
}
