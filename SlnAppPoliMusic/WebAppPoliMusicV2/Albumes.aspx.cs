using System;
using System.Collections.Generic;
using System.Text;
using WebAppPoliMusicV2.BLL;
using WebAppPoliMusicV2.Entities;
using WebAppPoliMusicV2.Util;

namespace WebAppPoliMusicV2
{
    /// <summary>
    /// Listado de albumes y detalle de un album con su lista de canciones.
    /// </summary>
    public partial class Albumes : PageBase
    {
        protected bool IsDetail;
        protected string Subtitle;
        protected string SearchTerm;
        protected string GridHtml;
        protected string RowsHtml;

        protected int AlbumId;
        protected string AlbumTitle;
        protected string AlbumArt;
        protected string ReleaseLabel;
        protected int SongCount;

        public Albumes()
        {
            ActiveSection = "albums";
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
            List<Album> albums = new AlbumBll().Read(SearchTerm);

            Subtitle = albums.Count == 1
                ? "1 album en el catalogo"
                : albums.Count + " albumes en el catalogo";

            StringBuilder sb = new StringBuilder();
            foreach (Album album in albums)
            {
                string art = MediaResolver.Resolve(album.AlbumImagePath);
                string url = ResolveUrl("~/Albumes.aspx?id=" + album.IdAlbum);
                sb.Append(Ui.AlbumCard(album, art, url));
            }
            GridHtml = sb.ToString();
        }

        private void LoadDetail(int id)
        {
            Album album = new AlbumBll().ReadById(id);
            if (album == null)
            {
                Response.StatusCode = 404;
                IsDetail = false;
                Subtitle = "El album solicitado no existe.";
                LoadGrid();
                return;
            }

            IsDetail = true;
            AlbumId = album.IdAlbum;
            AlbumTitle = album.TitleAlbum;
            AlbumArt = MediaResolver.Resolve(album.AlbumImagePath);
            ReleaseLabel = album.ReleaseLabel;

            List<Song> songs = new SongBll().ReadByAlbum(id);
            SongCount = songs.Count;
            string queue = Ui.PlayQueue(songs, AlbumArt);

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
