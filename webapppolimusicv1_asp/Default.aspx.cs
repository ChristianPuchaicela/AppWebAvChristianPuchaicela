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
    /// Pantalla principal. Todos los datos provienen de SQL Server; las
    /// secciones vacias muestran un estado util en lugar de contenido ficticio.
    /// </summary>
    public partial class Default : BasePage
    {
        protected List<Song> FeaturedSongs = new List<Song>();
        protected List<Album> Albums = new List<Album>();
        protected List<Artist> Artists = new List<Artist>();
        protected List<Playlist> MyPlaylists = new List<Playlist>();

        protected List<string> FeaturedRows = new List<string>();
        protected List<string> AlbumCards = new List<string>();
        protected List<string> ArtistCards = new List<string>();
        protected List<string> PlaylistCards = new List<string>();

        /// <summary>Aviso cuando hay canciones en la BD sin archivo de audio real.</summary>
        protected string AudioWarning { get; set; }

        protected int SongTotal { get; set; }
        protected int ArtistTotal { get; set; }
        protected int AlbumTotal { get; set; }
        protected int MyPlaylistTotal { get; set; }

        protected string CurrentUsername
        {
            get { return UserContext.CurrentUsername; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UserContext.IsAuthenticated)
            {
                Response.Redirect("~/Login.aspx", true);
                return;
            }

            ((SiteMaster)Master).ActiveSection = "home";

            int userId = UserContext.CurrentUserId;

            LoadFeaturedSongs();
            LoadAlbums();
            LoadArtists();
            LoadMyPlaylists(userId);
            ComputeStats(userId);
        }

        private void LoadFeaturedSongs()
        {
            FeaturedSongs = SongRepository.GetMostPlayed(6);

            var songList = new List<Song>();
            int playableCount = 0;
            foreach (Song s in FeaturedSongs)
            {
                if (MediaResolver.Resolve(s.SongPath) != null)
                {
                    playableCount++;
                }
            }

            // La cola de reproduccion se arma solo con las canciones que tienen
            // archivo de audio disponible.
            foreach (Song s in FeaturedSongs)
            {
                if (MediaResolver.Resolve(s.SongPath) != null)
                {
                    songList.Add(s);
                }
            }

            string queue = Ui.PlayQueue(songList, s => ArtistLabel(s.IdSong), null);

            int i = 1;
            foreach (Song s in FeaturedSongs)
            {
                FeaturedRows.Add(Ui.SongRow(s, ArtistLabel(s.IdSong), null, queue, i++));
            }

            if (FeaturedSongs.Count > 0 && playableCount == 0)
            {
                AudioWarning =
                    "Las canciones registradas en la base de datos no tienen un archivo de audio " +
                    "accesible, por lo que la reproducción está deshabilitada. Copie los archivos " +
                    "de audio en la carpeta Assets/SongFiles del proyecto.";
            }
        }

        /// <summary>Nombres de los artistas de una cancion, via TBL_PLAYER.</summary>
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

        private void LoadAlbums()
        {
            Albums = AlbumRepository.GetAllWithCounts();
            int limit = 0;
            foreach (Album a in Albums)
            {
                if (limit++ >= 5)
                {
                    break;
                }
                string art = MediaResolver.Resolve(a.AlbumImagePath);
                AlbumCards.Add(Ui.AlbumCard(a, art, ResolveUrl("~/Albumes.aspx?id=" + a.IdAlbum)));
            }
        }

        private void LoadArtists()
        {
            Artists = ArtistRepository.GetAll();
            int limit = 0;
            foreach (Artist a in Artists)
            {
                if (limit++ >= 5)
                {
                    break;
                }
                string art = MediaResolver.Resolve(a.ArtistImage);
                ArtistCards.Add(Ui.ArtistCard(a, art, ResolveUrl("~/Artistas.aspx?id=" + a.IdArtist)));
            }
        }

        private void LoadMyPlaylists(int userId)
        {
            MyPlaylists = PlaylistRepository.GetOwnedBy(userId);
            int limit = 0;
            foreach (Playlist p in MyPlaylists)
            {
                if (limit++ >= 5)
                {
                    break;
                }
                string art = MediaResolver.ResolveWithFallback(p.Photo, "~/playlistImages/default.jpg");
                PlaylistCards.Add(Ui.PlaylistCard(p, art, ResolveUrl("~/PlaylistDetalle.aspx?id=" + p.IdPlaylist)));
            }
        }

        private void ComputeStats(int userId)
        {
            SongTotal = SongRepository.GetAll().Count;
            ArtistTotal = ArtistRepository.GetAll().Count;
            AlbumTotal = AlbumRepository.GetAll().Count;
            MyPlaylistTotal = PlaylistRepository.GetOwnedBy(userId).Count;
        }

        protected static string H(string value)
        {
            return HttpUtility.HtmlEncode(value ?? string.Empty);
        }
    }
}
