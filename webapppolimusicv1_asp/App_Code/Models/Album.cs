namespace WebAppPoliMusicV1.Models
{
    /// <summary>
    /// Refleja exactamente [dbo].[TBL_ALBUM].
    /// </summary>
    public class Album
    {
        public int IdAlbum { get; set; }
        public string TitleAlbum { get; set; }
        public int? ReleaseYear { get; set; }
        public string AlbumImagePath { get; set; }

        /// <summary>Cantidad de canciones del album (TBL_TRACKLIST).</summary>
        public int SongCount { get; set; }
    }
}
