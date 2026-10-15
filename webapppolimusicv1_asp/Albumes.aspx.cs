using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using WebAppPoliMusicV1.Data;
using WebAppPoliMusicV1.Models;
using WebAppPoliMusicV1.Services;

namespace WebAppPoliMusicV1
{
    /// <summary>
    /// Catalogo de albumes. Las canciones de cada album provienen de
    /// TBL_TRACKLIST y el artista de cada pista de TBL_PLAYER.
    /// </summary>
    public partial class Albumes : BasePage
    {
        protected List<Album> Albums = new List<Album>();
        protected List<string> Cards = new List<string>();

        protected Album SelectedAlbum { get; set; }
        protected string SelectedAlbumArt { get; set; }
        protected List<Song> AlbumSongs = new List<Song>();
        protected List<string> Rows = new List<string>();
        protected string ArtistNames { get; set; }
        protected string AlbumQueueJson { get; set; }
        protected string AlbumFirstPlayJson { get; set; }

        protected bool IsAdmin
        {
            get { return UserContext.IsAdmin; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UserContext.IsAuthenticated)
            {
                Response.Redirect("~/Login.aspx", true);
                return;
            }

            ((SiteMaster)Master).ActiveSection = "albums";

            int albumId = QueryInt("id");
            if (albumId > 0)
            {
                LoadAlbumDetail(albumId);
            }
            else
            {
                LoadAlbumList();
            }
        }

        private void LoadAlbumList()
        {
            Albums = AlbumRepository.GetAllWithCounts();
            foreach (Album a in Albums)
            {
                string art = MediaResolver.Resolve(a.AlbumImagePath);
                Cards.Add(Ui.AlbumCard(a, art, ResolveUrl("~/Albumes.aspx?id=" + a.IdAlbum)));
            }
        }

        private void LoadAlbumDetail(int albumId)
        {
            SelectedAlbum = AlbumRepository.GetById(albumId);
            if (SelectedAlbum == null)
            {
                Response.Redirect("~/Albumes.aspx", true);
                return;
            }

            SelectedAlbumArt = MediaResolver.Resolve(SelectedAlbum.AlbumImagePath);
            AlbumSongs = SongRepository.GetByAlbum(albumId);

            var artistList = new List<string>();
            var artistOf = new Dictionary<int, string>();

            foreach (Song s in AlbumSongs)
            {
                string label = ArtistLabel(s.IdSong);
                artistOf[s.IdSong] = label;

                foreach (Artist a in SongRepository.GetArtistsOfSong(s.IdSong))
                {
                    if (!artistList.Contains(a.FullName))
                    {
                        artistList.Add(a.FullName);
                    }
                }
            }
            ArtistNames = string.Join(", ", artistList.ToArray());

            var playable = new List<Song>();
            foreach (Song s in AlbumSongs)
            {
                if (MediaResolver.Resolve(s.SongPath) != null)
                {
                    playable.Add(s);
                }
            }

            AlbumQueueJson = Ui.PlayQueue(playable, s => artistOf[s.IdSong], SelectedAlbumArt);
            AlbumFirstPlayJson = playable.Count > 0
                ? Ui.PlayPayload(playable[0], artistOf[playable[0].IdSong], SelectedAlbumArt)
                : "{}";

            int i = 1;
            foreach (Song s in AlbumSongs)
            {
                Rows.Add(Ui.SongRow(s, artistOf[s.IdSong], SelectedAlbumArt, AlbumQueueJson, i++));
            }
        }

        private static string ArtistLabel(int songId)
        {
            List<Artist> artists = SongRepository.GetArtistsOfSong(songId);
            if (artists.Count == 0)
            {
                return "Sin artista";
            }
            var names = new List<string>();
            foreach (Artist a in artists)
            {
                names.Add(a.FullName);
            }
            return string.Join(", ", names.ToArray());
        }

        protected static string H(string value)
        {
            return HttpUtility.HtmlEncode(value ?? string.Empty);
        }

        protected static string Url(string value)
        {
            return HttpUtility.HtmlAttributeEncode(value ?? string.Empty);
        }
    }
}
