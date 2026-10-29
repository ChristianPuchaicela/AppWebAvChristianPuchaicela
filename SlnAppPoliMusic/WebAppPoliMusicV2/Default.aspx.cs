using System;
using System.Collections.Generic;
using System.Text;
using WebAppPoliMusicV2.BLL;
using WebAppPoliMusicV2.Entities;
using WebAppPoliMusicV2.Util;

namespace WebAppPoliMusicV2
{
    /// <summary>
    /// Pagina de inicio: resumen del catalogo y accesos rapidos.
    /// Reemplaza al Default.aspx que solo redirigia a Song.aspx.
    /// </summary>
    public partial class Default : PageBase
    {
        protected string DisplayName;
        protected string StatsHtml;
        protected string TopSongsHtml;
        protected string AlbumsHtml;
        protected string ArtistsHtml;
        protected string PlaylistsHtml;

        public Default()
        {
            ActiveSection = "home";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            DisplayName = UserSession.CurrentUsername;

            LoadStats();
            LoadTopSongs();
            LoadAlbums();
            LoadArtists();
            LoadPlaylists();
        }

        private void LoadStats()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Ui.StatCard(new SongBll().CountAll().ToString(), "Canciones", "TBL_SONG"));
            sb.Append(Ui.StatCard(new ArtistBll().CountAll().ToString(), "Artistas", "TBL_ARTIST"));
            sb.Append(Ui.StatCard(new AlbumBll().CountAll().ToString(), "\u00C1lbumes", "TBL_ALBUM"));
            sb.Append(Ui.StatCard(new PlaylistBll().CountAll().ToString(), "Playlists", "TBL_PLAYLIST"));

            if (IsAdmin)
            {
                sb.Append(Ui.StatCard(new UserBll().CountAll().ToString(), "Usuarios", "TBL_USER"));
            }
            StatsHtml = sb.ToString();
        }

        private void LoadTopSongs()
        {
            List<Song> songs = new SongBll().ReadMostPlayed(5);
            string queue = Ui.PlayQueue(songs, null);

            StringBuilder sb = new StringBuilder();
            int index = 1;
            foreach (Song song in songs)
            {
                sb.Append(Ui.SongRow(song, index, queue, string.Empty));
                index++;
            }
            TopSongsHtml = sb.ToString();
        }

        private void LoadAlbums()
        {
            List<Album> albums = new AlbumBll().Read(null);
            StringBuilder sb = new StringBuilder();
            int count = 0;
            foreach (Album album in albums)
            {
                if (count >= 6)
                {
                    break;
                }
                string art = MediaResolver.Resolve(album.AlbumImagePath);
                string url = ResolveUrl("~/Albumes.aspx?id=" + album.IdAlbum);
                sb.Append(Ui.AlbumCard(album, art, url));
                count++;
            }
            AlbumsHtml = sb.ToString();
        }

        private void LoadArtists()
        {
            List<Artist> artists = new ArtistBll().Read(null);
            StringBuilder sb = new StringBuilder();
            int count = 0;
            foreach (Artist artist in artists)
            {
                if (count >= 6)
                {
                    break;
                }
                string art = MediaResolver.Resolve(artist.ArtistImage);
                string url = ResolveUrl("~/Artistas.aspx?id=" + artist.IdArtist);
                sb.Append(Ui.ArtistCard(artist, art, url));
                count++;
            }
            ArtistsHtml = sb.ToString();
        }

        private void LoadPlaylists()
        {
            List<Playlist> playlists = new PlaylistBll().ReadOwnedBy(CurrentUserId);
            StringBuilder sb = new StringBuilder();
            int count = 0;
            foreach (Playlist playlist in playlists)
            {
                if (count >= 6)
                {
                    break;
                }
                string art = MediaResolver.ResolveWithFallback(
                    playlist.Photo, Constants.DEFAULT_PLAYLIST_PHOTO);
                string url = ResolveUrl("~/PlaylistDetalle.aspx?id=" + playlist.IdPlaylist);
                sb.Append(Ui.PlaylistCard(playlist, art, url));
                count++;
            }
            PlaylistsHtml = sb.ToString();
        }
    }
}
