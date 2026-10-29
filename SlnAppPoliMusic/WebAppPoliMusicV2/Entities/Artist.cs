namespace WebAppPoliMusicV2.Entities
{
    /// <summary>Fila de [dbo].[TBL_ARTIST].</summary>
    public class Artist
    {
        public int IdArtist { get; set; }
        public string ArtistName { get; set; }
        public string ArtistLastname { get; set; }
        public string ArtistImage { get; set; }

        /// <summary>Numero de canciones asociadas en TBL_PLAYER.</summary>
        public int SongCount { get; set; }

        public string FullName
        {
            get
            {
                string name = (ArtistName ?? string.Empty).Trim();
                string lastname = (ArtistLastname ?? string.Empty).Trim();
                if (lastname.Length == 0)
                {
                    return name;
                }
                if (name.Length == 0)
                {
                    return lastname;
                }
                return name + " " + lastname;
            }
        }
    }
}
