using System;

namespace WebAppPoliMusicV2.Entities
{
    /// <summary>Fila de [dbo].[TBL_PLAYLIST] con datos de apoyo para la vista.</summary>
    public class Playlist
    {
        public int IdPlaylist { get; set; }
        public int IdUser { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
        public int Type { get; set; }
        public string Photo { get; set; }

        /// <summary>USERNAME del propietario (JOIN con TBL_USER).</summary>
        public string OwnerUsername { get; set; }

        /// <summary>Cantidad de canciones en TBL_PLAYLIST_SONG.</summary>
        public int SongCount { get; set; }

        public bool IsPrivate
        {
            get { return Type == Util.Constants.ID_PLAYLIST_PRIVATE; }
        }

        public string VisibilityName
        {
            get { return IsPrivate ? "Privada" : "P\u00fablica"; }
        }
    }
}
