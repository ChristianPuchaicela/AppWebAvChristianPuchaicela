using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using WebAppPoliMusicV1.Models;

namespace WebAppPoliMusicV1.Data
{
    /// <summary>Acceso a datos de [dbo].[TBL_ALBUM] y su lista TBL_TRACKLIST.</summary>
    public static class AlbumRepository
    {
        private const string SelectColumns =
            "SELECT ID_ALBUM, TITLE_ALBUM, RELEASE_YEAR, ALBUM_IMAGE_PATH FROM TBL_ALBUM ";

        private static Album Map(SqlDataReader r)
        {
            return new Album
            {
                IdAlbum = Db.ToInt(r, "ID_ALBUM"),
                TitleAlbum = Db.ToStr(r, "TITLE_ALBUM"),
                ReleaseYear = Db.ToNullableInt(r, "RELEASE_YEAR"),
                AlbumImagePath = Db.ToStr(r, "ALBUM_IMAGE_PATH")
            };
        }

        public static List<Album> GetAll(string search = null)
        {
            if (string.IsNullOrEmpty(search))
            {
                return Db.Query(SelectColumns + "ORDER BY TITLE_ALBUM", Map);
            }
            return Db.Query(SelectColumns + "WHERE TITLE_ALBUM LIKE @q ORDER BY TITLE_ALBUM",
                Map, Db.P("@q", "%" + search + "%"));
        }

        /// <summary>Albumes con la cantidad de canciones de su tracklist.</summary>
        public static List<Album> GetAllWithCounts()
        {
            return Db.Query(
                @"SELECT a.ID_ALBUM, a.TITLE_ALBUM, a.RELEASE_YEAR, a.ALBUM_IMAGE_PATH,
                         (SELECT COUNT(1) FROM TBL_TRACKLIST t WHERE t.ID_ALBUM = a.ID_ALBUM) AS SONG_COUNT
                    FROM TBL_ALBUM a
                   ORDER BY a.TITLE_ALBUM",
                r =>
                {
                    Album a = Map(r);
                    a.SongCount = Db.ToInt(r, "SONG_COUNT");
                    return a;
                });
        }

        public static Album GetById(int id)
        {
            return Db.QuerySingle(SelectColumns + "WHERE ID_ALBUM = @id", Map, Db.P("@id", id));
        }

        /// <summary>
        /// Albumes en los que participa un artista, cruzando TBL_PLAYER con
        /// TBL_TRACKLIST. Se resuelve en una sola consulta.
        /// </summary>
        public static List<Album> GetByArtist(int artistId)
        {
            return Db.Query(
                @"SELECT DISTINCT a.ID_ALBUM, a.TITLE_ALBUM, a.RELEASE_YEAR, a.ALBUM_IMAGE_PATH
                    FROM TBL_ALBUM a
                    INNER JOIN TBL_TRACKLIST t ON t.ID_ALBUM = a.ID_ALBUM
                    INNER JOIN TBL_PLAYER p ON p.ID_SONG = t.ID_SONG
                   WHERE p.ID_ARTIST = @id
                   ORDER BY a.TITLE_ALBUM",
                Map, Db.P("@id", artistId));
        }

        public static int Create(string title, int? year, string image)
        {
            return Db.InsertReturningId(
                "INSERT INTO TBL_ALBUM (TITLE_ALBUM, RELEASE_YEAR, ALBUM_IMAGE_PATH) VALUES (@t, @y, @i)",
                Db.P("@t", title), Db.P("@y", year.HasValue ? (object)year.Value : null), Db.P("@i", image));
        }

        public static int Update(int id, string title, int? year, string image)
        {
            return Db.ExecuteNonQuery(
                "UPDATE TBL_ALBUM SET TITLE_ALBUM=@t, RELEASE_YEAR=@y, ALBUM_IMAGE_PATH=@i WHERE ID_ALBUM=@id",
                Db.P("@t", title), Db.P("@y", year.HasValue ? (object)year.Value : null),
                Db.P("@i", image), Db.P("@id", id));
        }

        public static int CountSongs(int albumId)
        {
            object o = Db.ExecuteScalar(
                "SELECT COUNT(1) FROM TBL_TRACKLIST WHERE ID_ALBUM = @id", Db.P("@id", albumId));
            return Convert.ToInt32(o);
        }

        /// <summary>Elimina el album y las filas de TBL_TRACKLIST asociadas.</summary>
        public static void DeleteCascade(int albumId)
        {
            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    using (SqlCommand c1 = Db.Command(cn, "DELETE FROM TBL_TRACKLIST WHERE ID_ALBUM = @id", Db.P("@id", albumId)))
                    { c1.Transaction = tx; c1.ExecuteNonQuery(); }
                    using (SqlCommand c2 = Db.Command(cn, "DELETE FROM TBL_ALBUM WHERE ID_ALBUM = @id", Db.P("@id", albumId)))
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
    }
}
