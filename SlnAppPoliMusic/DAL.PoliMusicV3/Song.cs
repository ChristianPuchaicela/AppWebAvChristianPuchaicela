using System;
using System.Data;
using System.Data.SqlClient;

namespace DAL.PoliMusicV3
{
    public class Song
    {
        private string strConnectionString;

        public Song(string strConnString)
        {
            strConnectionString = strConnString;
        }

        public DataTable Read()
        {
            DataTable dtSong = new DataTable();
            using (SqlConnection con = new SqlConnection(strConnectionString))
            {
                string query = @"SELECT s.ID_SONG, s.SONG_NAME, s.SONG_PATH, ISNULL(s.PLAYS, 0) AS PLAYS,
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
                                  ORDER BY s.SONG_NAME";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        sda.Fill(dtSong);
                        return dtSong;
                    }
                }
            }
        }

        public DataTable ReadById(int idSong)
        {
            DataTable dtSong = new DataTable();
            using (SqlConnection con = new SqlConnection(strConnectionString))
            {
                string query = @"SELECT s.ID_SONG, s.SONG_NAME, s.SONG_PATH, ISNULL(s.PLAYS, 0) AS PLAYS,
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
                                  WHERE s.ID_SONG = @ID_SONG";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ID_SONG", idSong);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        sda.Fill(dtSong);
                        return dtSong;
                    }
                }
            }
        }

        public int IncrementPlays(int idSong)
        {
            using (SqlConnection con = new SqlConnection(strConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("UPDATE TBL_SONG SET PLAYS = ISNULL(PLAYS, 0) + 1 WHERE ID_SONG = @ID_SONG", con))
                {
                    cmd.Parameters.AddWithValue("@ID_SONG", idSong);
                    con.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public int Save(EntityLayer.PoliMusicV3.Song song)
        {
            using (SqlConnection con = new SqlConnection(strConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("INSERT INTO TBL_SONG (SONG_NAME, SONG_PATH, PLAYS) VALUES (@SONG_NAME, @SONG_PATH, @PLAYS)", con))
                {
                    cmd.Parameters.AddWithValue("@SONG_NAME", song.SongName);
                    cmd.Parameters.AddWithValue("@SONG_PATH", song.SongPath);
                    cmd.Parameters.AddWithValue("@PLAYS", song.Plays ?? 0);
                    con.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public int Update(EntityLayer.PoliMusicV3.Song song)
        {
            using (SqlConnection con = new SqlConnection(strConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("UPDATE TBL_SONG SET SONG_NAME = @SONG_NAME, SONG_PATH = @SONG_PATH, PLAYS = @PLAYS WHERE ID_SONG = @ID_SONG", con))
                {
                    cmd.Parameters.AddWithValue("@ID_SONG", song.ID);
                    cmd.Parameters.AddWithValue("@SONG_NAME", song.SongName);
                    cmd.Parameters.AddWithValue("@SONG_PATH", song.SongPath);
                    cmd.Parameters.AddWithValue("@PLAYS", song.Plays ?? 0);
                    con.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public int Delete(int idSong)
        {
            using (SqlConnection con = new SqlConnection(strConnectionString))
            {
                con.Open();
                using (SqlTransaction trans = con.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = con;
                            cmd.Transaction = trans;
                            cmd.Parameters.AddWithValue("@ID_SONG", idSong);

                            cmd.CommandText = "DELETE FROM TBL_PLAYLIST_SONG WHERE ID_SONG = @ID_SONG";
                            cmd.ExecuteNonQuery();

                            cmd.CommandText = "DELETE FROM TBL_TRACKLIST WHERE ID_SONG = @ID_SONG";
                            cmd.ExecuteNonQuery();

                            cmd.CommandText = "DELETE FROM TBL_PLAYER WHERE ID_SONG = @ID_SONG";
                            cmd.ExecuteNonQuery();

                            cmd.CommandText = "DELETE FROM TBL_SONG WHERE ID_SONG = @ID_SONG";
                            int affected = cmd.ExecuteNonQuery();

                            trans.Commit();
                            return affected;
                        }
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
