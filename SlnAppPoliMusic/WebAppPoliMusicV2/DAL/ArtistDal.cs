using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using WebAppPoliMusicV2.Entities;

namespace WebAppPoliMusicV2.DAL
{
    /// <summary>Acceso a datos de [dbo].[TBL_ARTIST].</summary>
    public class ArtistDal
    {
        private const string SelectColumns =
            @"SELECT a.ID_ARTIST, a.ARTIST_NAME, a.ARTIST_LASTNAME, a.ARTIST_IMAGE,
                     (SELECT COUNT(1) FROM TBL_PLAYER p WHERE p.ID_ARTIST = a.ID_ARTIST) AS SONG_COUNT
                FROM TBL_ARTIST a ";

        private static Artist Map(SqlDataReader r)
        {
            return new Artist
            {
                IdArtist = Db.ToInt(r, "ID_ARTIST"),
                ArtistName = Db.ToStr(r, "ARTIST_NAME"),
                ArtistLastname = Db.ToStr(r, "ARTIST_LASTNAME"),
                ArtistImage = Db.ToStr(r, "ARTIST_IMAGE"),
                SongCount = Db.ToInt(r, "SONG_COUNT")
            };
        }

        public List<Artist> Read(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return Db.Query(SelectColumns + "ORDER BY a.ARTIST_NAME, a.ARTIST_LASTNAME", Map);
            }
            return Db.Query(
                SelectColumns +
                "WHERE a.ARTIST_NAME LIKE @q OR ISNULL(a.ARTIST_LASTNAME, '') LIKE @q " +
                "ORDER BY a.ARTIST_NAME, a.ARTIST_LASTNAME",
                Map, Db.P("@q", "%" + search + "%"));
        }

        public Artist ReadById(int idArtist)
        {
            return Db.QuerySingle(SelectColumns + "WHERE a.ID_ARTIST = @id", Map, Db.P("@id", idArtist));
        }

        public int Create(Artist artist)
        {
            return Db.InsertReturningId(
                @"INSERT INTO TBL_ARTIST (ARTIST_NAME, ARTIST_LASTNAME, ARTIST_IMAGE)
                  VALUES (@n, @l, @img)",
                Db.P("@n", artist.ArtistName),
                Db.P("@l", artist.ArtistLastname),
                Db.P("@img", artist.ArtistImage));
        }

        public int Update(Artist artist)
        {
            return Db.ExecuteNonQuery(
                @"UPDATE TBL_ARTIST
                     SET ARTIST_NAME = @n, ARTIST_LASTNAME = @l, ARTIST_IMAGE = @img
                   WHERE ID_ARTIST = @id",
                Db.P("@n", artist.ArtistName),
                Db.P("@l", artist.ArtistLastname),
                Db.P("@img", artist.ArtistImage),
                Db.P("@id", artist.IdArtist));
        }

        /// <summary>Elimina el artista y sus vinculos en TBL_PLAYER.</summary>
        public void DeleteCascade(int idArtist)
        {
            Db.ExecuteTransaction(
                new[] { Db.P("@id", idArtist) },
                "DELETE FROM TBL_PLAYER WHERE ID_ARTIST = @id",
                "DELETE FROM TBL_ARTIST WHERE ID_ARTIST = @id");
        }

        public int CountAll()
        {
            return Db.ExecuteScalarInt("SELECT COUNT(1) FROM TBL_ARTIST");
        }
    }
}
