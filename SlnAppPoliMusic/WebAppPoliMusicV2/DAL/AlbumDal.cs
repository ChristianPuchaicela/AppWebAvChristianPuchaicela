using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using WebAppPoliMusicV2.Entities;

namespace WebAppPoliMusicV2.DAL
{
    /// <summary>Acceso a datos de [dbo].[TBL_ALBUM] y [dbo].[TBL_TRACKLIST].</summary>
    public class AlbumDal
    {
        private const string SelectColumns =
            @"SELECT al.ID_ALBUM, al.TITLE_ALBUM, al.RELEASE_YEAR, al.ALBUM_IMAGE_PATH,
                     (SELECT COUNT(1) FROM TBL_TRACKLIST t WHERE t.ID_ALBUM = al.ID_ALBUM) AS SONG_COUNT
                FROM TBL_ALBUM al ";

        private static Album Map(SqlDataReader r)
        {
            return new Album
            {
                IdAlbum = Db.ToInt(r, "ID_ALBUM"),
                TitleAlbum = Db.ToStr(r, "TITLE_ALBUM"),
                ReleaseYear = Db.ToNullableInt(r, "RELEASE_YEAR"),
                AlbumImagePath = Db.ToStr(r, "ALBUM_IMAGE_PATH"),
                SongCount = Db.ToInt(r, "SONG_COUNT")
            };
        }

        public List<Album> Read(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return Db.Query(SelectColumns + "ORDER BY al.TITLE_ALBUM", Map);
            }
            return Db.Query(SelectColumns + "WHERE al.TITLE_ALBUM LIKE @q ORDER BY al.TITLE_ALBUM",
                Map, Db.P("@q", "%" + search + "%"));
        }

        public Album ReadById(int idAlbum)
        {
            return Db.QuerySingle(SelectColumns + "WHERE al.ID_ALBUM = @id", Map, Db.P("@id", idAlbum));
        }

        public int Create(Album album)
        {
            return Db.InsertReturningId(
                @"INSERT INTO TBL_ALBUM (TITLE_ALBUM, RELEASE_YEAR, ALBUM_IMAGE_PATH)
                  VALUES (@t, @y, @img)",
                Db.P("@t", album.TitleAlbum),
                Db.P("@y", album.ReleaseYear),
                Db.P("@img", album.AlbumImagePath));
        }

        public int Update(Album album)
        {
            return Db.ExecuteNonQuery(
                @"UPDATE TBL_ALBUM
                     SET TITLE_ALBUM = @t, RELEASE_YEAR = @y, ALBUM_IMAGE_PATH = @img
                   WHERE ID_ALBUM = @id",
                Db.P("@t", album.TitleAlbum),
                Db.P("@y", album.ReleaseYear),
                Db.P("@img", album.AlbumImagePath),
                Db.P("@id", album.IdAlbum));
        }

        /// <summary>Elimina el album y sus vinculos en TBL_TRACKLIST.</summary>
        public void DeleteCascade(int idAlbum)
        {
            Db.ExecuteTransaction(
                new[] { Db.P("@id", idAlbum) },
                "DELETE FROM TBL_TRACKLIST WHERE ID_ALBUM = @id",
                "DELETE FROM TBL_ALBUM WHERE ID_ALBUM = @id");
        }

        public int CountAll()
        {
            return Db.ExecuteScalarInt("SELECT COUNT(1) FROM TBL_ALBUM");
        }
    }
}
