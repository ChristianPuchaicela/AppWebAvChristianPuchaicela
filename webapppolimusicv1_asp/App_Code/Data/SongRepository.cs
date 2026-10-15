using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using WebAppPoliMusicV1.Models;

namespace WebAppPoliMusicV1.Data
{
    /// <summary>
    /// Acceso a datos de [dbo].[TBL_SONG].
    /// Relaciones: TBL_PLAYER (cancion-artista), TBL_TRACKLIST (album-cancion),
    /// TBL_PLAYLIST_SONG (playlist-cancion).
    /// </summary>
    public static class SongRepository
    {
        private const string SelectColumns =
            "SELECT ID_SONG, SONG_NAME, SONG_PATH, PLAYS FROM TBL_SONG ";

        private static Song Map(SqlDataReader r)
        {
            return new Song
            {
                IdSong = Db.ToInt(r, "ID_SONG"),
                SongName = Db.ToStr(r, "SONG_NAME"),
                SongPath = Db.ToStr(r, "SONG_PATH"),
                Plays = Db.ToNullableInt(r, "PLAYS")
            };
        }

        public static List<Song> GetAll(string search = null)
        {
            if (string.IsNullOrEmpty(search))
            {
                return Db.Query(SelectColumns + "ORDER BY SONG_NAME", Map);
            }
            return Db.Query(SelectColumns + "WHERE SONG_NAME LIKE @q ORDER BY SONG_NAME",
                Map, Db.P("@q", "%" + search + "%"));
        }

        public static Song GetById(int id)
        {
            return Db.QuerySingle(SelectColumns + "WHERE ID_SONG = @id", Map, Db.P("@id", id));
        }

        public static List<Song> GetMostPlayed(int top)
        {
            return Db.Query(
                "SELECT TOP (@top) ID_SONG, SONG_NAME, SONG_PATH, PLAYS FROM TBL_SONG " +
                "ORDER BY ISNULL(PLAYS,0) DESC, SONG_NAME",
                Map, Db.P("@top", top));
        }

        /// <summary>Canciones de un album a traves de TBL_TRACKLIST.</summary>
        public static List<Song> GetByAlbum(int albumId)
        {
            return Db.Query(
                @"SELECT s.ID_SONG, s.SONG_NAME, s.SONG_PATH, s.PLAYS
                    FROM TBL_SONG s
                    INNER JOIN TBL_TRACKLIST t ON t.ID_SONG = s.ID_SONG
                   WHERE t.ID_ALBUM = @id
                   ORDER BY s.SONG_NAME",
                Map, Db.P("@id", albumId));
        }

        /// <summary>Canciones de un artista a traves de TBL_PLAYER.</summary>
        public static List<Song> GetByArtist(int artistId)
        {
            return Db.Query(
                @"SELECT s.ID_SONG, s.SONG_NAME, s.SONG_PATH, s.PLAYS
                    FROM TBL_SONG s
                    INNER JOIN TBL_PLAYER p ON p.ID_SONG = s.ID_SONG
                   WHERE p.ID_ARTIST = @id
                   ORDER BY s.SONG_NAME",
                Map, Db.P("@id", artistId));
        }

        /// <summary>Canciones de una playlist a traves de TBL_PLAYLIST_SONG.</summary>
        public static List<Song> GetByPlaylist(int playlistId)
        {
            return Db.Query(
                @"SELECT s.ID_SONG, s.SONG_NAME, s.SONG_PATH, s.PLAYS
                    FROM TBL_SONG s
                    INNER JOIN TBL_PLAYLIST_SONG ps ON ps.ID_SONG = s.ID_SONG
                   WHERE ps.ID_PLAYLIST = @id
                   ORDER BY s.SONG_NAME",
                Map, Db.P("@id", playlistId));
        }

        public static int Create(string name, string path, int plays)
        {
            return Db.InsertReturningId(
                "INSERT INTO TBL_SONG (SONG_NAME, SONG_PATH, PLAYS) VALUES (@n, @p, @pl)",
                Db.P("@n", name), Db.P("@p", path), Db.P("@pl", plays));
        }

        public static int Update(int id, string name, string path, int plays)
        {
            return Db.ExecuteNonQuery(
                "UPDATE TBL_SONG SET SONG_NAME = @n, SONG_PATH = @p, PLAYS = @pl WHERE ID_SONG = @id",
                Db.P("@n", name), Db.P("@p", path), Db.P("@pl", plays), Db.P("@id", id));
        }

        /// <summary>Incrementa el contador de reproducciones de forma atomica.</summary>
        public static int IncrementPlays(int id)
        {
            return Db.ExecuteNonQuery(
                "UPDATE TBL_SONG SET PLAYS = ISNULL(PLAYS,0) + 1 WHERE ID_SONG = @id",
                Db.P("@id", id));
        }

        public static int CountPlaylistsUsing(int songId)
        {
            object o = Db.ExecuteScalar(
                "SELECT COUNT(1) FROM TBL_PLAYLIST_SONG WHERE ID_SONG = @id", Db.P("@id", songId));
            return Convert.ToInt32(o);
        }

        public static int CountAlbumsUsing(int songId)
        {
            object o = Db.ExecuteScalar(
                "SELECT COUNT(1) FROM TBL_TRACKLIST WHERE ID_SONG = @id", Db.P("@id", songId));
            return Convert.ToInt32(o);
        }

        public static int CountArtistsUsing(int songId)
        {
            object o = Db.ExecuteScalar(
                "SELECT COUNT(1) FROM TBL_PLAYER WHERE ID_SONG = @id", Db.P("@id", songId));
            return Convert.ToInt32(o);
        }

        /// <summary>Elimina una cancion junto con sus filas dependientes en una transaccion.</summary>
        public static void DeleteCascade(int songId)
        {
            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    using (SqlCommand c1 = Db.Command(cn, "DELETE FROM TBL_PLAYLIST_SONG WHERE ID_SONG = @id", Db.P("@id", songId)))
                    { c1.Transaction = tx; c1.ExecuteNonQuery(); }
                    using (SqlCommand c2 = Db.Command(cn, "DELETE FROM TBL_TRACKLIST WHERE ID_SONG = @id", Db.P("@id", songId)))
                    { c2.Transaction = tx; c2.ExecuteNonQuery(); }
                    using (SqlCommand c3 = Db.Command(cn, "DELETE FROM TBL_PLAYER WHERE ID_SONG = @id", Db.P("@id", songId)))
                    { c3.Transaction = tx; c3.ExecuteNonQuery(); }
                    using (SqlCommand c4 = Db.Command(cn, "DELETE FROM TBL_SONG WHERE ID_SONG = @id", Db.P("@id", songId)))
                    { c4.Transaction = tx; c4.ExecuteNonQuery(); }
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        // ----- Relacion TBL_PLAYER (cancion <-> artista) -----

        public static List<Artist> GetArtistsOfSong(int songId)
        {
            return Db.Query(
                @"SELECT a.ID_ARTIST, a.ARTIST_NAME, a.ARTIST_LASTNAME, a.ARTIST_IMAGE
                    FROM TBL_ARTIST a
                    INNER JOIN TBL_PLAYER p ON p.ID_ARTIST = a.ID_ARTIST
                   WHERE p.ID_SONG = @id
                   ORDER BY a.ARTIST_NAME",
                r => new Artist
                {
                    IdArtist = Db.ToInt(r, "ID_ARTIST"),
                    ArtistName = Db.ToStr(r, "ARTIST_NAME"),
                    ArtistLastname = Db.ToStr(r, "ARTIST_LASTNAME"),
                    ArtistImage = Db.ToStr(r, "ARTIST_IMAGE")
                },
                Db.P("@id", songId));
        }

        public static void SetArtist(int songId, int artistId)
        {
            Db.ExecuteNonQuery(
                "IF NOT EXISTS (SELECT 1 FROM TBL_PLAYER WHERE ID_SONG=@s AND ID_ARTIST=@a) " +
                "INSERT INTO TBL_PLAYER (ID_SONG, ID_ARTIST) VALUES (@s, @a)",
                Db.P("@s", songId), Db.P("@a", artistId));
        }

        public static void RemoveArtist(int songId, int artistId)
        {
            Db.ExecuteNonQuery(
                "DELETE FROM TBL_PLAYER WHERE ID_SONG = @s AND ID_ARTIST = @a",
                Db.P("@s", songId), Db.P("@a", artistId));
        }

        // ----- Relacion TBL_TRACKLIST (album <-> cancion) -----

        public static void SetAlbum(int songId, int albumId)
        {
            Db.ExecuteNonQuery(
                "IF NOT EXISTS (SELECT 1 FROM TBL_TRACKLIST WHERE ID_SONG=@s AND ID_ALBUM=@a) " +
                "INSERT INTO TBL_TRACKLIST (ID_ALBUM, ID_SONG) VALUES (@a, @s)",
                Db.P("@s", songId), Db.P("@a", albumId));
        }

        public static void RemoveAlbum(int songId, int albumId)
        {
            Db.ExecuteNonQuery(
                "DELETE FROM TBL_TRACKLIST WHERE ID_SONG = @s AND ID_ALBUM = @a",
                Db.P("@s", songId), Db.P("@a", albumId));
        }
    }
}
