using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebAppPoliMusicV1.Data;
using WebAppPoliMusicV1.Models;
using WebAppPoliMusicV1.Services;

namespace WebAppPoliMusicV1
{
    /// <summary>
    /// Detalle y gestion de una playlist.
    ///
    /// Control de propiedad: el ID de la playlist llega por la URL, por lo que
    /// NUNCA se confia en el. Se consulta la playlist filtrando por ID_USER de
    /// la sesion; si no es propia, solo se permite lectura cuando es publica.
    /// Las operaciones de escritura vuelven a comprobar la propiedad en el
    /// servidor antes de tocar la base de datos.
    /// </summary>
    public partial class PlaylistDetalle : BasePage
    {
        protected Playlist Playlist { get; set; }
        protected bool IsOwner { get; set; }
        protected string ArtUrl { get; set; }
        protected string PageTitle { get; set; }
        protected List<Song> Songs = new List<Song>();
        protected List<string> Rows = new List<string>();
        protected string QueueJson { get; set; }
        protected string FirstPlayJson { get; set; }
        protected string Message { get; set; }
        protected bool MessageIsError { get; set; }

        private int _playlistId;
        private int _userId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UserContext.IsAuthenticated)
            {
                Response.Redirect("~/Login.aspx", true);
                return;
            }

            ((SiteMaster)Master).ActiveSection = "playlists";

            _playlistId = QueryInt("id");
            _userId = UserContext.CurrentUserId;

            if (_playlistId <= 0)
            {
                Playlist = null;
                PageTitle = "Playlist";
                return;
            }

            // Se resuelve la playlist respetando la propiedad y la visibilidad.
            Playlist = PlaylistRepository.GetReadable(_playlistId, _userId);

            if (Playlist == null)
            {
                // No existe, es privada de otro usuario o el ID fue manipulado.
                // La respuesta es la misma en los tres casos: no se filtra informacion.
                Response.StatusCode = 404;
                PageTitle = "Playlist no disponible";
                return;
            }

            IsOwner = Playlist.IdUser == _userId;
            PageTitle = Playlist.Title;
            ArtUrl = MediaResolver.ResolveWithFallback(Playlist.Photo, "~/playlistImages/default.jpg");

            if (!IsPostBack)
            {
                HandleAction();
                LoadSongs();
                if (IsOwner)
                {
                    BindSongOptions();
                    BindEditFields();
                }
            }
            else
            {
                LoadSongs();
            }
        }

        /// <summary>
        /// Procesa la accion de eliminacion solicitada por GET. Requiere que el
        /// usuario sea propietario y que el token anti-CSRF coincida.
        /// </summary>
        private void HandleAction()
        {
            if (string.Equals(Request.QueryString["action"], "deleted", StringComparison.OrdinalIgnoreCase))
            {
                Message = "La playlist se eliminó correctamente.";
                MessageIsError = false;
                return;
            }

            if (string.Equals(Request.QueryString["created"], "1", StringComparison.Ordinal))
            {
                Message = "La playlist se creó correctamente.";
                MessageIsError = false;
                return;
            }

            if (!string.Equals(Request.QueryString["action"], "delete", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Eliminar exige ser propietario.
            if (!IsOwner)
            {
                Response.StatusCode = 403;
                Message = "No tiene permiso para eliminar esta playlist.";
                MessageIsError = true;
                return;
            }

            if (!IsCsrfValid())
            {
                Message = "Solicitud no válida. Vuelva a intentarlo desde la página.";
                MessageIsError = true;
                return;
            }

            // DeleteOwned filtra por ID_USER en la propia sentencia SQL.
            bool deleted = PlaylistRepository.DeleteOwned(_playlistId, _userId);
            if (deleted)
            {
                Response.Redirect("~/Playlists.aspx?deleted=1", true);
            }
            else
            {
                Message = "No se pudo eliminar la playlist.";
                MessageIsError = true;
            }
        }

        private void LoadSongs()
        {
            if (Playlist == null)
            {
                return;
            }

            Songs = SongRepository.GetByPlaylist(_playlistId);

            var artistOf = new Dictionary<int, string>();
            foreach (Song s in Songs)
            {
                artistOf[s.IdSong] = ArtistLabel(s.IdSong);
            }

            var playable = new List<Song>();
            foreach (Song s in Songs)
            {
                if (MediaResolver.Resolve(s.SongPath) != null)
                {
                    playable.Add(s);
                }
            }

            QueueJson = Ui.PlayQueue(playable, s => artistOf[s.IdSong], ArtUrl);
            FirstPlayJson = playable.Count > 0
                ? Ui.PlayPayload(playable[0], artistOf[playable[0].IdSong], ArtUrl)
                : "{}";

            int i = 1;
            foreach (Song s in Songs)
            {
                string actions = string.Empty;
                if (IsOwner)
                {
                    // Quitar la cancion de la playlist (TBL_PLAYLIST_SONG).
                    actions = "<a class=\"pm-btn pm-btn-sm pm-btn-danger\" href=\""
                        + HttpUtility.HtmlAttributeEncode(ResolveUrl("~/PlaylistDetalle.aspx?id="
                            + _playlistId + "&action=remove&songId=" + s.IdSong
                            + "&token=" + GetCsrf()))
                        + "\" data-pm-confirm=\"¿Quitar «"
                        + HttpUtility.HtmlAttributeEncode(s.SongName)
                        + "» de esta playlist?\">Quitar</a>";
                }
                Rows.Add(Ui.SongRow(s, artistOf[s.IdSong], ArtUrl, QueueJson, i++, actions));
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (Playlist == null || !IsOwner || IsPostBack)
            {
                return;
            }

            // Procesa "quitar cancion", que llega por GET con token.
            if (!string.Equals(Request.QueryString["action"], "remove", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!IsCsrfValid())
            {
                Message = "Solicitud no válida.";
                MessageIsError = true;
                return;
            }

            int songId = QueryInt("songId");
            if (songId <= 0)
            {
                return;
            }

            // Se revalida la propiedad antes de escribir.
            Playlist owned = PlaylistRepository.GetOwned(_playlistId, _userId);
            if (owned == null)
            {
                Response.StatusCode = 403;
                Message = "No tiene permiso para modificar esta playlist.";
                MessageIsError = true;
                return;
            }

            PlaylistRepository.RemoveSong(_playlistId, songId);
            Message = "La canción se quitó de la playlist.";
            MessageIsError = false;
            LoadSongs();
            BindSongOptions();
        }

        private void BindSongOptions()
        {
            var existing = new HashSet<int>();
            foreach (Song s in Songs)
            {
                existing.Add(s.IdSong);
            }

            ddlSong.Items.Clear();
            foreach (Song s in SongRepository.GetAll())
            {
                if (existing.Contains(s.IdSong))
                {
                    continue;
                }
                string label = s.SongName;
                if (MediaResolver.Resolve(s.SongPath) == null)
                {
                    label += " (sin audio)";
                }
                ddlSong.Items.Add(new ListItem(label, s.IdSong.ToString()));
            }

            if (ddlSong.Items.Count == 0)
            {
                ddlSong.Items.Add(new ListItem("No hay canciones disponibles", "0"));
                btnAddSong.Enabled = false;
            }
        }

        private void BindEditFields()
        {
            txtEditTitle.Text = Playlist.Title;

            ddlEditType.Items.Clear();
            foreach (CatalogItem item in CatalogRepository.GetByParent("2"))
            {
                int value;
                if (int.TryParse(item.CodCatalog, out value))
                {
                    ddlEditType.Items.Add(new ListItem(item.Value, item.CodCatalog));
                }
            }
            if (ddlEditType.Items.Count == 0)
            {
                ddlEditType.Items.Add(new ListItem("Pública", "0"));
                ddlEditType.Items.Add(new ListItem("Privada", "1"));
            }

            string selected = Playlist.Type.ToString();
            ListItem match = ddlEditType.Items.FindByValue(selected);
            if (match != null)
            {
                ddlEditType.ClearSelection();
                match.Selected = true;
            }
            else if (ddlEditType.Items.Count > 0)
            {
                ddlEditType.SelectedIndex = 0;
            }

            ddlEditPhoto.Items.Clear();
            string physical = Server.MapPath("~/Assets/PlaylistImages");
            if (System.IO.Directory.Exists(physical))
            {
                var files = System.IO.Directory.GetFiles(physical);
                Array.Sort(files, StringComparer.OrdinalIgnoreCase);
                foreach (string f in files)
                {
                    string name = System.IO.Path.GetFileName(f);
                    ddlEditPhoto.Items.Add(new ListItem(name, "~/playlistImages/" + name));
                }
            }
            if (ddlEditPhoto.Items.Count == 0)
            {
                ddlEditPhoto.Items.Add(new ListItem("Predeterminada", "~/playlistImages/default.jpg"));
            }

            ListItem current = ddlEditPhoto.Items.FindByValue(Playlist.Photo);
            if (current != null)
            {
                ddlEditPhoto.ClearSelection();
                current.Selected = true;
            }
        }

        /// <summary>Agrega una cancion a la playlist propia.</summary>
        protected void btnAddSong_Click(object sender, EventArgs e)
        {
            if (Playlist == null || !IsOwner)
            {
                Response.StatusCode = 403;
                return;
            }

            int songId;
            if (!int.TryParse(ddlSong.SelectedValue, out songId) || songId <= 0)
            {
                Message = "Seleccione una canción válida.";
                MessageIsError = true;
                return;
            }

            // Revalidacion de propiedad en el servidor.
            Playlist owned = PlaylistRepository.GetOwned(_playlistId, _userId);
            if (owned == null)
            {
                Response.StatusCode = 403;
                Message = "No tiene permiso para modificar esta playlist.";
                MessageIsError = true;
                return;
            }

            // La cancion debe existir.
            if (SongRepository.GetById(songId) == null)
            {
                Message = "La canción indicada no existe.";
                MessageIsError = true;
                return;
            }

            PlaylistRepository.AddSong(_playlistId, songId);
            Message = "Canción agregada a la playlist.";
            MessageIsError = false;

            LoadSongs();
            BindSongOptions();
        }

        /// <summary>
        /// Guarda los cambios de titulo, visibilidad y portada. UpdateOwned
        /// incluye ID_USER en el WHERE, de modo que un usuario no puede editar
        /// una playlist ajena aunque manipule el ID de la URL.
        /// </summary>
        protected void btnSaveEdit_Click(object sender, EventArgs e)
        {
            if (Playlist == null || !IsOwner)
            {
                Response.StatusCode = 403;
                return;
            }

            string title = (txtEditTitle.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(title))
            {
                Message = "El título es obligatorio.";
                MessageIsError = true;
                return;
            }
            if (title.Length > 50)
            {
                Message = "El título no puede superar los 50 caracteres.";
                MessageIsError = true;
                return;
            }

            int type;
            if (!int.TryParse(ddlEditType.SelectedValue, out type))
            {
                type = PlaylistTypes.Public;
            }

            string photo = ddlEditPhoto.SelectedValue;

            int affected = PlaylistRepository.UpdateOwned(_playlistId, _userId, title, type, photo);
            if (affected == 0)
            {
                Message = "No se pudieron guardar los cambios: la playlist no le pertenece.";
                MessageIsError = true;
                return;
            }

            Response.Redirect("~/PlaylistDetalle.aspx?id=" + _playlistId, true);
        }

        private bool IsCsrfValid()
        {
            string expected = Session["PM_CSRF"] as string;
            // El token puede venir por GET (enlaces de accion) o del formulario.
            string received = Request.QueryString["token"] ?? Request.Form["__PoliMusicToken"];
            return !string.IsNullOrEmpty(expected) && !string.IsNullOrEmpty(received)
                && string.Equals(expected, received, StringComparison.Ordinal);
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
