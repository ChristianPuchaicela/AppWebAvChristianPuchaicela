namespace WebAppPoliMusicV2.Entities
{
    /// <summary>Fila de [dbo].[TBL_SONG].</summary>
    public class Song
    {
        public int IdSong { get; set; }
        public string SongName { get; set; }
        public string SongPath { get; set; }
        public int? Plays { get; set; }

        /// <summary>
        /// Etiqueta de artistas separada por comas. Se rellena en las consultas
        /// de lectura (JOIN con TBL_PLAYER) y no se persiste en TBL_SONG.
        /// </summary>
        public string ArtistLabel { get; set; }

        /// <summary>Titulo del primer album asociado, si existe.</summary>
        public string AlbumTitle { get; set; }

        public int PlayCount
        {
            get { return Plays ?? 0; }
        }
    }
}
