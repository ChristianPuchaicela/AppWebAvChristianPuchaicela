using System;

namespace WebAppPoliMusicV1.Models
{
    /// <summary>
    /// Refleja exactamente [dbo].[TBL_PLAYLIST].
    /// La relacion con canciones vive en TBL_PLAYLIST_SONG (tabla intermedia).
    /// </summary>
    public class Playlist
    {
        public int IdPlaylist { get; set; }
        public int IdUser { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
        public int Type { get; set; }
        public string Photo { get; set; }

        /// <summary>Nombre del dueno, resuelto por JOIN con TBL_USER.</summary>
        public string OwnerUsername { get; set; }

        /// <summary>Cantidad de canciones, resuelta por TBL_PLAYLIST_SONG.</summary>
        public int SongCount { get; set; }

        public bool IsPrivate
        {
            get { return Type == PlaylistTypes.Private; }
        }

        public string TypeName
        {
            get { return PlaylistTypes.Name(Type); }
        }
    }
}
