using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using WebAppPoliMusicV1.Models;

namespace WebAppPoliMusicV1.Data
{
    /// <summary>Acceso a datos de [dbo].[TBL_ARTIST].</summary>
    public static class ArtistRepository
    {
        private const string SelectColumns =
            "SELECT ID_ARTIST, ARTIST_NAME, ARTIST_LASTNAME, ARTIST_IMAGE FROM TBL_ARTIST ";

        private static Artist Map(SqlDataReader r)
        {
            return new Artist
            {
                IdArtist = Db.ToInt(r, "ID_ARTIST"),
                ArtistName = Db.ToStr(r, "ARTIST_NAME"),
                ArtistLastname = Db.ToStr(r, "ARTIST_LASTNAME"),
                ArtistImage = Db.ToStr(r, "ARTIST_IMAGE")
            };
        }

        public static List<Artist> GetAll(string search = null)
        {
            if (string.IsNullOrEmpty(search))
            {
                return Db.Query(SelectColumns + "ORDER BY ARTIST_NAME, ARTIST_LASTNAME", Map);
            }
            return Db.Query(
                SelectColumns + "WHERE ARTIST_NAME LIKE @q OR ARTIST_LASTNAME LIKE @q " +
                "ORDER BY ARTIST_NAME, ARTIST_LASTNAME",
                Map, Db.P("@q", "%" + search + "%"));
        }

        /// <summary>Artistas con cantidad de canciones, para las tarjetas del panel admin.</summary>
        public static List<Artist> GetAllWithCounts()
        {
            return Db.Query(
                @"SELECT a.ID_ARTIST, a.ARTIST_NAME, a.ARTIST_LASTNAME, a.ARTIST_IMAGE,
                         (SELECT COUNT(1) FROM TBL_PLAYER p WHERE p.ID_ARTIST = a.ID_ARTIST) AS SONG_COUNT
                    FROM TBL_ARTIST a
                   ORDER BY a.ARTIST_NAME, a.ARTIST_LASTNAME",
                r =>
                {
                    Artist a = Map(r);
                    return a;
                });
        }

        public static Dictionary<int, int> GetSongCounts()
        {
            var map = new Dictionary<int, int>();
            foreach (var row in Db.Query(
                "SELECT ID_ARTIST, COUNT(1) AS C FROM TBL_PLAYER GROUP BY ID_ARTIST",
                r => new { Id = Db.ToInt(r, "ID_ARTIST"), C = Db.ToInt(r, "C") }))
            {
                map[row.Id] = row.C;
            }
            return map;
        }

        public static Artist GetById(int id)
        {
            return Db.QuerySingle(SelectColumns + "WHERE ID_ARTIST = @id", Map, Db.P("@id", id));
        }

        public static int Create(string name, string lastname, string image)
        {
            return Db.InsertReturningId(
                "INSERT INTO TBL_ARTIST (ARTIST_NAME, ARTIST_LASTNAME, ARTIST_IMAGE) VALUES (@n, @l, @i)",
                Db.P("@n", name), Db.P("@l", lastname), Db.P("@i", image));
        }

        public static int Update(int id, string name, string lastname, string image)
        {
            return Db.ExecuteNonQuery(
                "UPDATE TBL_ARTIST SET ARTIST_NAME=@n, ARTIST_LASTNAME=@l, ARTIST_IMAGE=@i WHERE ID_ARTIST=@id",
                Db.P("@n", name), Db.P("@l", lastname), Db.P("@i", image), Db.P("@id", id));
        }

        /// <summary>Canciones vinculadas via TBL_PLAYER (FK_TBL_SONG_ARTIST_TBL_ARTIST).</summary>
        public static int CountSongs(int artistId)
        {
            object o = Db.ExecuteScalar(
                "SELECT COUNT(1) FROM TBL_PLAYER WHERE ID_ARTIST = @id", Db.P("@id", artistId));
            return Convert.ToInt32(o);
        }

        /// <summary>Elimina el artista y sus vinculos en TBL_PLAYER (las canciones se conservan).</summary>
        public static void DeleteCascade(int artistId)
        {
            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    using (SqlCommand c1 = Db.Command(cn, "DELETE FROM TBL_PLAYER WHERE ID_ARTIST = @id", Db.P("@id", artistId)))
                    { c1.Transaction = tx; c1.ExecuteNonQuery(); }
                    using (SqlCommand c2 = Db.Command(cn, "DELETE FROM TBL_ARTIST WHERE ID_ARTIST = @id", Db.P("@id", artistId)))
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
