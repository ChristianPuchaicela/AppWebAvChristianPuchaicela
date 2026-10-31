using System;

namespace EntityLayer.PoliMusicV3
{
    public class Song
    {
        public Song()
        {
        }

        public Song(int id, string songName, string songPath, int? plays, string artistLabel = null, string albumTitle = null)
        {
            ID = id;
            SongName = songName;
            SongPath = songPath;
            Plays = plays;
            ArtistLabel = artistLabel;
            AlbumTitle = albumTitle;
        }

        public int ID { get; set; }
        public string SongName { get; set; }
        public string SongPath { get; set; }
        public int? Plays { get; set; }
        public string ArtistLabel { get; set; }
        public string AlbumTitle { get; set; }

        public int PlayCount
        {
            get { return Plays ?? 0; }
        }
    }
}
