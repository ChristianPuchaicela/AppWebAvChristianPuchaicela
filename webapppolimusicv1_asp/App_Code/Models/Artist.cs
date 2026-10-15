namespace WebAppPoliMusicV1.Models
{
    /// <summary>
    /// Refleja exactamente [dbo].[TBL_ARTIST].
    /// </summary>
    public class Artist
    {
        public int IdArtist { get; set; }
        public string ArtistName { get; set; }
        public string ArtistLastname { get; set; }
        public string ArtistImage { get; set; }

        public string FullName
        {
            get
            {
                string n = (ArtistName ?? string.Empty).Trim();
                string l = (ArtistLastname ?? string.Empty).Trim();
                return string.IsNullOrEmpty(l) ? n : (n + " " + l);
            }
        }
    }
}
