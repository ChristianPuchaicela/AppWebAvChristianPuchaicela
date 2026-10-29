namespace WebAppPoliMusicV2.Entities
{
    /// <summary>Fila de [dbo].[TBL_ALBUM].</summary>
    public class Album
    {
        public int IdAlbum { get; set; }
        public string TitleAlbum { get; set; }
        public int? ReleaseYear { get; set; }
        public string AlbumImagePath { get; set; }

        /// <summary>Numero de canciones asociadas en TBL_TRACKLIST.</summary>
        public int SongCount { get; set; }

        public string ReleaseLabel
        {
            get { return ReleaseYear.HasValue ? ReleaseYear.Value.ToString() : "A\u00f1o desconocido"; }
        }
    }
}
