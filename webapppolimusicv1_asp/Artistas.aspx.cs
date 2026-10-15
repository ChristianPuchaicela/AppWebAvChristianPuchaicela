using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using WebAppPoliMusicV1.Data;
using WebAppPoliMusicV1.Models;
using WebAppPoliMusicV1.Services;

namespace WebAppPoliMusicV1
{
    /// <summary>
    /// Catalogo de artistas con vista de detalle. La relacion artista-cancion
    /// proviene de TBL_PLAYER; los albumes se deducen de TBL_TRACKLIST.
    /// </summary>
    public partial class Artistas : BasePage
    {
        protected List<Artist> Artists = new List<Artist>();
        protected List<string> Cards = new List<string>();

        protected Artist SelectedArtist { get; set; }
        protected string SelectedArtistArt { get; set; }
        protected List<Song> ArtistSongs = new List<Song>();
        protected List<string> Rows = new List<string>();
        protected int ArtistSongCount { get; set; }
        protected string AlbumNames { get; set; }
        protected string ArtistQueueJson { get; set; }
        protected string ArtistFirstPlayJson { get; set; }

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

            ((SiteMaster)Master).ActiveSection = "artists";

            int artistId = QueryInt("id");
            if (artistId > 0)
            {
                LoadArtistDetail(artistId);
            }
            else
            {
                LoadArtistList();
            }
        }

        private void LoadArtistList()
        {
            Artists = ArtistRepository.GetAll();
            foreach (Artist a in Artists)
            {
                string art = MediaResolver.Resolve(a.ArtistImage);
                Cards.Add(Ui.ArtistCard(a, art, ResolveUrl("~/Artistas.aspx?id=" + a.IdArtist)));
            }
        }

        private void LoadArtistDetail(int artistId)
        {
            SelectedArtist = ArtistRepository.GetById(artistId);
            if (SelectedArtist == null)
            {
                Response.Redirect("~/Artistas.aspx", true);
                return;
            }

            SelectedArtistArt = MediaResolver.Resolve(SelectedArtist.ArtistImage);

            ArtistSongs = SongRepository.GetByArtist(artistId);
            ArtistSongCount = ArtistSongs.Count;

            // Albumes en los que aparece el artista, resuelto en una consulta.
            var albumSet = new List<string>();
            foreach (Album a in AlbumRepository.GetByArtist(artistId))
            {
                albumSet.Add(a.TitleAlbum);
            }
            AlbumNames = string.Join(", ", albumSet.ToArray());

            string label = SelectedArtist.FullName;

            var playable = new List<Song>();
            foreach (Song s in ArtistSongs)
            {
                if (MediaResolver.Resolve(s.SongPath) != null)
                {
                    playable.Add(s);
                }
            }

            ArtistQueueJson = Ui.PlayQueue(playable, s => label, SelectedArtistArt);
            ArtistFirstPlayJson = playable.Count > 0
                ? Ui.PlayPayload(playable[0], label, SelectedArtistArt)
                : "{}";

            int i = 1;
            foreach (Song s in ArtistSongs)
            {
                Rows.Add(Ui.SongRow(s, label, SelectedArtistArt, ArtistQueueJson, i++));
            }
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
