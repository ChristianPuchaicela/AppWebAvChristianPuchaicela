namespace WebAppPoliMusicV1.Models
{
    /// <summary>
    /// Refleja exactamente [dbo].[TBL_SONG].
    /// </summary>
    public class Song
    {
        public int IdSong { get; set; }
        public string SongName { get; set; }
        public string SongPath { get; set; }
        public int? Plays { get; set; }
    }
}
