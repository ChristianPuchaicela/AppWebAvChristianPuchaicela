using System;
using System.Collections.Generic;
using System.Text;
using WebAppPoliMusicV2.BLL;
using WebAppPoliMusicV2.Entities;
using WebAppPoliMusicV2.Util;

namespace WebAppPoliMusicV2
{
    /// <summary>
    /// Listado de artistas y detalle de un artista con sus canciones.
    /// El detalle se resuelve con ?id=N en la misma pagina.
    /// </summary>
    public partial class Artistas : PageBase
    {
        protected bool IsDetail;
        protected string Subtitle;
        protected string SearchTerm;
        protected string GridHtml;
        protected string RowsHtml;

        protected int ArtistId;
        protected string ArtistName;
        protected string ArtistArt;
        protected int SongCount;

        public Artistas()
        {
            ActiveSection = "artists";
            GridHtml = string.Empty;
            RowsHtml = string.Empty;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            int id = QueryInt("id");
            if (id > 0)
            {
                LoadDetail(id);
                return;
            }
            LoadGrid();
        }

        private void LoadGrid()
        {
            SearchTerm = QueryStr("q");
            List<Artist> artists = new ArtistBll().Read(SearchTerm);

            Subtitle = artists.Count == 1
                ? "1 artista en el catalogo"
                : artists.Count + " artistas en el catalogo";

            StringBuilder sb = new StringBuilder();
            foreach (Artist artist in artists)
            {
                string art = MediaResolver.Resolve(artist.ArtistImage);
                string url = ResolveUrl("~/Artistas.aspx?id=" + artist.IdArtist);
                sb.Append(Ui.ArtistCard(artist, art, url));
            }
            GridHtml = sb.ToString();
        }

        private void LoadDetail(int id)
        {
            Artist artist = new ArtistBll().ReadById(id);
            if (artist == null)
            {
                Response.StatusCode = 404;
                IsDetail = false;
                Subtitle = "El artista solicitado no existe.";
                LoadGrid();
                return;
            }

            IsDetail = true;
            ArtistId = artist.IdArtist;
            ArtistName = artist.FullName;
            ArtistArt = MediaResolver.Resolve(artist.ArtistImage);
            SongCount = artist.SongCount;

            List<Song> songs = new SongBll().ReadByArtist(id);
            SongCount = songs.Count;
            string queue = Ui.PlayQueue(songs, ArtistArt);

            StringBuilder sb = new StringBuilder();
            int index = 1;
            foreach (Song song in songs)
            {
                sb.Append(Ui.SongRow(song, index, queue, string.Empty));
                index++;
            }
            RowsHtml = sb.ToString();
        }
    }
}
