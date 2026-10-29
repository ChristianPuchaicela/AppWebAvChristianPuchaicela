using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using WebAppPoliMusicV2.Entities;

namespace WebAppPoliMusicV2.DAL
{
    /// <summary>
    /// Acceso a datos de [dbo].[TBL_SONG] y de sus relaciones:
    /// TBL_PLAYER (cancion - artista), TBL_TRACKLIST (album - cancion) y
    /// TBL_PLAYLIST_SONG (playlist - cancion).
    /// </summary>
    public class SongDal
    {
        /// <summary>
        /// Columnas de TBL_SONG mas dos datos derivados: la etiqueta de
        /// artistas y el primer album. Se resuelven en SQL para no repetir
        /// consultas por fila.
        /// </summary>
        private const string SelectColumns =
            @"SELECT s.ID_SONG, s.SONG_NAME, s.SONG_PATH, s.PLAYS,
                     STUFF((SELECT DISTINCT ', ' + a.ARTIST_NAME
                              FROM TBL_PLAYER p
                              INNER JOIN TBL_ARTIST a ON a.ID_ARTIST = p.ID_ARTIST
                             WHERE p.ID_SONG = s.ID_SONG
                             FOR XML PATH('')), 1, 2, '') AS ARTIST_LABEL,
                     (SELECT TOP 1 al.TITLE_ALBUM
                        FROM TBL_TRACKLIST t
                        INNER JOIN TBL_ALBUM al ON al.ID_ALBUM = t.ID_ALBUM
                       WHERE t.ID_SONG = s.ID_SONG) AS ALBUM_TITLE
                FROM TBL_SONG s ";

        private static Song Map(SqlDataReader r)
        {
            return new Song
            {
                IdSong = Db.ToInt(r, "ID_SONG"),
                SongName = Db.ToStr(r, "SONG_NAME"),
                SongPath = Db.ToStr(r, "SONG_PATH"),
                Plays = Db.ToNullableInt(r, "PLAYS"),
                ArtistLabel = Db.ToStr(r, "ARTIST_LABEL"),
                AlbumTitle = Db.ToStr(r, "ALBUM_TITLE")
            };
        }

        public List<Song> Read(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return Db.Query(SelectColumns + "ORDER BY s.SONG_NAME", Map);
            }
            return Db.Query(SelectColumns + "WHERE s.SONG_NAME LIKE @q ORDER BY s.SONG_NAME",
                Map, Db.P("@q", "%" + search + "%"));
        }

        public Song ReadById(int idSong)
        {
            return Db.QuerySingle(SelectColumns + "WHERE s.ID_SONG = @id", Map, Db.P("@id", idSong));
        }

        public List<Song> ReadMostPlayed(int top)
        {
            if (top <= 0)
            {
                top = 5;
            }
            return Db.Query(
                "SELECT TOP (@top) s.ID_SONG, s.SONG_NAME, s.SONG_PATH, s.PLAYS, " +
                "       STUFF((SELECT DISTINCT ', ' + a.ARTIST_NAME " +
                "                FROM TBL_PLAYER p " +
                "                INNER JOIN TBL_ARTIST a ON a.ID_ARTIST = p.ID_ARTIST " +
                "               WHERE p.ID_SONG = s.ID_SONG " +
                "               FOR XML PATH('')), 1, 2, '') AS ARTIST_LABEL, " +
                "       (SELECT TOP 1 al.TITLE_ALBUM " +
                "          FROM TBL_TRACKLIST t " +
                "          INNER JOIN TBL_ALBUM al ON al.ID_ALBUM = t.ID_ALBUM " +
                "         WHERE t.ID_SONG = s.ID_SONG) AS ALBUM_TITLE " +
                "  FROM TBL_SONG s " +
                " ORDER BY ISNULL(s.PLAYS, 0) DESC, s.SONG_NAME",
                Map, Db.P("@top", top));
        }

        /// <summary>Canciones de un album (TBL_TRACKLIST).</summary>
        public List<Song> ReadByAlbum(int idAlbum)
        {
            return Db.Query(
                @"SELECT s.ID_SONG, s.SONG_NAME, s.SONG_PATH, s.PLAYS,
                         STUFF((SELECT DISTINCT ', ' + a.ARTIST_NAME
                                  FROM TBL_PLAYER p
                                  INNER JOIN TBL_ARTIST a ON a.ID_ARTIST = p.ID_ARTIST
                                 WHERE p.ID_SONG = s.ID_SONG
                                 FOR XML PATH('')), 1, 2, '') AS ARTIST_LABEL,
                         CAST(NULL AS varchar(50)) AS ALBUM_TITLE
                    FROM TBL_SONG s
                    INNER JOIN TBL_TRACKLIST t ON t.ID_SONG = s.ID_SONG
                   WHERE t.ID_ALBUM = @id
                   ORDER BY s.SONG_NAME",
                Map, Db.P("@id", idAlbum));
        }

        /// <summary>Canciones de un artista (TBL_PLAYER).</summary>
        public List<Song> ReadByArtist(int idArtist)
        {
            return Db.Query(
                @"SELECT s.ID_SONG, s.SONG_NAME, s.SONG_PATH, s.PLAYS,
                         (SELECT DISTINCT a.ARTIST_NAME
                            FROM TBL_ARTIST a WHERE a.ID_ARTIST = @id) AS ARTIST_LABEL,
                         (SELECT TOP 1 al.TITLE_ALBUM
                            FROM TBL_TRACKLIST t
                            INNER JOIN TBL_ALBUM al ON al.ID_ALBUM = t.ID_ALBUM
                           WHERE t.ID_SONG = s.ID_SONG) AS ALBUM_TITLE
                    FROM TBL_SONG s
                    INNER JOIN TBL_PLAYER p ON p.ID_SONG = s.ID_SONG
                   WHERE p.ID_ARTIST = @id
                   ORDER BY s.SONG_NAME",
                Map, Db.P("@id", idArtist));
        }

        /// <summary>Canciones de una playlist (TBL_PLAYLIST_SONG).</summary>
        public List<Song> ReadByPlaylist(int idPlaylist)
        {
            return Db.Query(
                @"SELECT s.ID_SONG, s.SONG_NAME, s.SONG_PATH, s.PLAYS,
                         STUFF((SELECT DISTINCT ', ' + a.ARTIST_NAME
                                  FROM TBL_PLAYER p
                                  INNER JOIN TBL_ARTIST a ON a.ID_ARTIST = p.ID_ARTIST
                                 WHERE p.ID_SONG = s.ID_SONG
                                 FOR XML PATH('')), 1, 2, '') AS ARTIST_LABEL,
                         (SELECT TOP 1 al.TITLE_ALBUM
                            FROM TBL_TRACKLIST t
                            INNER JOIN TBL_ALBUM al ON al.ID_ALBUM = t.ID_ALBUM
                           WHERE t.ID_SONG = s.ID_SONG) AS ALBUM_TITLE
                    FROM TBL_SONG s
                    INNER JOIN TBL_PLAYLIST_SONG ps ON ps.ID_SONG = s.ID_SONG
                   WHERE ps.ID_PLAYLIST = @id
                   ORDER BY s.SONG_NAME",
                Map, Db.P("@id", idPlaylist));
        }

        public int Create(string songName, string songPath, int plays)
        {
            return Db.InsertReturningId(
                "INSERT INTO TBL_SONG (SONG_NAME, SONG_PATH, PLAYS) VALUES (@n, @p, @pl)",
                Db.P("@n", songName), Db.P("@p", songPath), Db.P("@pl", plays));
        }

        public int Update(int idSong, string songName, string songPath, int plays)
        {
            return Db.ExecuteNonQuery(
                "UPDATE TBL_SONG SET SONG_NAME = @n, SONG_PATH = @p, PLAYS = @pl WHERE ID_SONG = @id",
                Db.P("@n", songName), Db.P("@p", songPath), Db.P("@pl", plays), Db.P("@id", idSong));
        }

        /// <summary>Incremento atomico del contador PLAYS.</summary>
        public int IncrementPlays(int idSong)
        {
            return Db.ExecuteNonQuery(
                "UPDATE TBL_SONG SET PLAYS = ISNULL(PLAYS, 0) + 1 WHERE ID_SONG = @id",
                Db.P("@id", idSong));
        }

        /// <summary>Elimina la cancion y sus filas dependientes en una transaccion.</summary>
        public void DeleteCascade(int idSong)
        {
            Db.ExecuteTransaction(
                new[] { Db.P("@id", idSong) },
                "DELETE FROM TBL_PLAYLIST_SONG WHERE ID_SONG = @id",
                "DELETE FROM TBL_TRACKLIST WHERE ID_SONG = @id",
                "DELETE FROM TBL_PLAYER WHERE ID_SONG = @id",
                "DELETE FROM TBL_SONG WHERE ID_SONG = @id");
        }

        // ------------------------------- TBL_PLAYER (cancion - artista) ---

        public void SetArtist(int idSong, int idArtist)
        {
            Db.ExecuteNonQuery(
                "IF NOT EXISTS (SELECT 1 FROM TBL_PLAYER WHERE ID_SONG = @s AND ID_ARTIST = @a) " +
                "INSERT INTO TBL_PLAYER (ID_SONG, ID_ARTIST) VALUES (@s, @a)",
                Db.P("@s", idSong), Db.P("@a", idArtist));
        }

        public void RemoveArtist(int idSong, int idArtist)
        {
            Db.ExecuteNonQuery(
                "DELETE FROM TBL_PLAYER WHERE ID_SONG = @s AND ID_ARTIST = @a",
                Db.P("@s", idSong), Db.P("@a", idArtist));
        }

        // ------------------------------ TBL_TRACKLIST (album - cancion) ---

        public void SetAlbum(int idSong, int idAlbum)
        {
            Db.ExecuteNonQuery(
                "IF NOT EXISTS (SELECT 1 FROM TBL_TRACKLIST WHERE ID_SONG = @s AND ID_ALBUM = @a) " +
                "INSERT INTO TBL_TRACKLIST (ID_ALBUM, ID_SONG) VALUES (@a, @s)",
                Db.P("@s", idSong), Db.P("@a", idAlbum));
        }

        public void RemoveAlbum(int idSong, int idAlbum)
        {
            Db.ExecuteNonQuery(
                "DELETE FROM TBL_TRACKLIST WHERE ID_SONG = @s AND ID_ALBUM = @a",
                Db.P("@s", idSong), Db.P("@a", idAlbum));
        }

        public int CountAll()
        {
            return Db.ExecuteScalarInt("SELECT COUNT(1) FROM TBL_SONG");
        }

        /// <summary>Primer artista asociado a la cancion (0 si no tiene).</summary>
        public int ReadArtistIdOfSong(int idSong)
        {
            return Db.ExecuteScalarInt(
                "SELECT TOP 1 ID_ARTIST FROM TBL_PLAYER WHERE ID_SONG = @id ORDER BY ID_ARTIST",
                Db.P("@id", idSong));
        }

        /// <summary>Primer album asociado a la cancion (0 si no tiene).</summary>
        public int ReadAlbumIdOfSong(int idSong)
        {
            return Db.ExecuteScalarInt(
                "SELECT TOP 1 ID_ALBUM FROM TBL_TRACKLIST WHERE ID_SONG = @id ORDER BY ID_ALBUM",
                Db.P("@id", idSong));
        }

        public int CountPlaylistsUsing(int idSong)
        {
            return Db.ExecuteScalarInt("SELECT COUNT(1) FROM TBL_PLAYLIST_SONG WHERE ID_SONG = @id",
                Db.P("@id", idSong));
        }
    }
}
