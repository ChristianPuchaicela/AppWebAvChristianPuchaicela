using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebAppPoliMusicV1.Data;
using WebAppPoliMusicV1.Models;
using WebAppPoliMusicV1.Services;

namespace WebAppPoliMusicV1
{
    /// <summary>
    /// Catalogo de canciones con reproduccion. El listado lee de TBL_SONG y
    /// resuelve artista (TBL_PLAYER), album (TBL_TRACKLIST) y disponibilidad
    /// real del archivo de audio.
    /// </summary>
    public partial class Canciones : BasePage
    {
        protected List<Song> Songs = new List<Song>();
        protected List<string> Rows = new List<string>();
        protected string SearchTerm { get; set; }
        protected string AudioWarning { get; set; }

        private List<Playlist> MyPlaylists = new List<Playlist>();

        protected void Page_Load(object sender, EventArgs e)
        {
            ((SiteMaster)Master).ActiveSection = "songs";

            int userId = UserContext.CurrentUserId;
            MyPlaylists = PlaylistRepository.GetOwnedBy(userId);

            SearchTerm = Request.QueryString["q"];
            LoadSongs(SearchTerm);
        }

        private void LoadSongs(string search)
        {
            Songs = SongRepository.GetAll(search);

            // La cola compartida incluye unicamente canciones reproducibles.
            var playable = new List<Song>();
            var artistOf = new Dictionary<int, string>();
            var albumOf = BuildAlbumMap();

            foreach (Song s in Songs)
            {
                if (MediaResolver.Resolve(s.SongPath) != null)
                {
                    playable.Add(s);
                }
                artistOf[s.IdSong] = ArtistLabel(s.IdSong);
            }

            string queue = Ui.PlayQueue(playable, s => artistOf[s.IdSong], null);

            int index = 1;
            int missing = 0;
            foreach (Song s in Songs)
            {
                if (MediaResolver.Resolve(s.SongPath) == null)
                {
                    missing++;
                }

                var actions = new StringBuilder();
                if (MyPlaylists.Count > 0)
                {
                    // Formulario en linea para incorporar la cancion a una playlist.
                    actions.Append("<form method=\"post\" action=\"")
                           .Append(HttpUtility.HtmlAttributeEncode(ResolveUrl("~/Canciones.aspx")))
                           .Append("\" style=\"display:inline-flex;gap:6px;align-items:center;\">")
                           .Append("<input type=\"hidden\" name=\"__PoliMusicToken\" value=\"")
                           .Append(HttpUtility.HtmlAttributeEncode(GetCsrf())).Append("\" />")
                           .Append("<input type=\"hidden\" name=\"songId\" value=\"").Append(s.IdSong).Append("\" />")
                           .Append("<select name=\"playlistId\" class=\"pm-select\" style=\"padding:5px 26px 5px 10px;font-size:0.78rem;width:auto;\">");
                    foreach (Playlist p in MyPlaylists)
                    {
                        actions.Append("<option value=\"").Append(p.IdPlaylist).Append("\">")
                               .Append(HttpUtility.HtmlEncode(p.Title)).Append("</option>");
                    }
                    actions.Append("</select>")
                           .Append("<button type=\"submit\" class=\"pm-btn pm-btn-sm\">Agregar</button>")
                           .Append("</form>");
                }

                string album;
                if (!albumOf.TryGetValue(s.IdSong, out album))
                {
                    album = "—";
                }

                Rows.Add(Ui.SongRow(s, artistOf[s.IdSong], null, queue, index++, actions.ToString(), album));
            }

            if (missing > 0)
            {
                AudioWarning = missing + " de " + Songs.Count +
                    " canción(es) no tienen un archivo de audio accesible en la carpeta Assets/SongFiles, " +
                    "por lo que su reproducción está deshabilitada.";
            }
        }

        /// <summary>
        /// Construye el mapa cancion -> album con una sola lectura por album,
        /// en lugar de consultar la lista completa por cada cancion.
        /// </summary>
        private static Dictionary<int, string> BuildAlbumMap()
        {
            var bySong = new Dictionary<int, List<string>>();
            foreach (Album a in AlbumRepository.GetAll())
            {
                foreach (Song s in SongRepository.GetByAlbum(a.IdAlbum))
                {
                    List<string> list;
                    if (!bySong.TryGetValue(s.IdSong, out list))
                    {
                        list = new List<string>();
                        bySong[s.IdSong] = list;
                    }
                    list.Add(a.TitleAlbum);
                }
            }

            var map = new Dictionary<int, string>();
            foreach (var pair in bySong)
            {
                map[pair.Key] = string.Join(", ", pair.Value.ToArray());
            }
            return map;
        }

        private string GetCsrf()
        {
            const string key = "PM_CSRF";
            string token = Session[key] as string;
            if (string.IsNullOrEmpty(token))
            {
                token = Guid.NewGuid().ToString("N");
                Session[key] = token;
            }
            return token;
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

        /// <summary>
        /// Agrega la cancion a una playlist propiedad del usuario. La fila de
        /// encabezado de la tabla muestra el album; aqui se agrega la logica del
        /// formulario en linea (POST clasico).
        /// </summary>
        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                return;
            }

            if (Request.Form["songId"] == null || Request.Form["playlistId"] == null)
            {
                return;
            }

            if (!ValidateCsrfToken())
            {
                ShowMessage("La solicitud no es válida. Vuelva a intentarlo.", true);
                return;
            }

            int songId = FormInt("songId");
            int playlistId = FormInt("playlistId");

            if (songId <= 0 || playlistId <= 0)
            {
                ShowMessage("Selección inválida.", true);
                return;
            }

            // Control de propiedad: la playlist debe pertenecer al usuario.
            Playlist owned = PlaylistRepository.GetOwned(playlistId, UserContext.CurrentUserId);
            if (owned == null)
            {
                ShowMessage("No tiene permiso para modificar esa playlist.", true);
                return;
            }

            if (PlaylistRepository.ContainsSong(playlistId, songId))
            {
                ShowMessage("La canción ya está en «" + owned.Title + "».", false);
                return;
            }

            PlaylistRepository.AddSong(playlistId, songId);
            ShowMessage("Canción agregada a «" + owned.Title + "».", false);
            Response.Redirect(ResolveUrl("~/Canciones.aspx") + "?q=" + HttpUtility.UrlEncode(SearchTerm ?? string.Empty), true);
        }

        private bool ValidateCsrfToken()
        {
            string expected = Session["PM_CSRF"] as string;
            string received = Request.Form["__PoliMusicToken"];
            return !string.IsNullOrEmpty(expected) && !string.IsNullOrEmpty(received)
                && string.Equals(expected, received, StringComparison.Ordinal);
        }

        private void ShowMessage(string message, bool isError)
        {
            lblMessage.Text = HttpUtility.HtmlEncode(message);
            pnlMessage.Visible = true;
            pnlMessage.CssClass = "alert " + (isError ? "alert-error" : "alert-success");
        }

        protected static string H(string value)
        {
            return HttpUtility.HtmlEncode(value ?? string.Empty);
        }
    }
}
