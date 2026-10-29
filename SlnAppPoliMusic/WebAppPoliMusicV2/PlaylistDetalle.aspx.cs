using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using WebAppPoliMusicV2.BLL;
using WebAppPoliMusicV2.Entities;
using WebAppPoliMusicV2.Util;

namespace WebAppPoliMusicV2
{
    /// <summary>
    /// Detalle de una playlist. Solo el propietario puede modificarla:
    /// agregar o quitar canciones, renombrarla, cambiar su visibilidad,
    /// reemplazar la portada o eliminarla.
    ///
    /// El control de propiedad se hace en el servidor con ReadOwned/ReadReadable;
    /// el identificador de la URL nunca decide permisos por si solo.
    /// </summary>
    public partial class PlaylistDetalle : PageBase
    {
        protected bool NotFound;
        protected bool IsOwner;
        protected bool IsPrivate;
        protected int PlaylistId;
        protected int SongCount;
        protected string PlaylistTitle;
        protected string PlaylistArt;
        protected string OwnerName;
        protected string VisibilityHtml;
        protected string CreationDateText;
        protected string RowsHtml;
        protected string CatalogOptionsHtml;
        protected string ErrorMessage;
        protected string PageTitleText;

        private readonly PlaylistBll _playlists = new PlaylistBll();

        public PlaylistDetalle()
        {
            ActiveSection = "playlists";
            RowsHtml = string.Empty;
            CatalogOptionsHtml = string.Empty;
            ErrorMessage = string.Empty;
            PageTitleText = "Playlist \u00B7 PoliMusic";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            int id = QueryInt("id");
            Playlist playlist = id > 0 ? _playlists.ReadReadable(id, CurrentUserId) : null;

            if (playlist == null)
            {
                NotFound = true;
                Response.StatusCode = 404;
                PageTitleText = "Playlist no disponible \u00B7 PoliMusic";
                return;
            }

            PlaylistId = playlist.IdPlaylist;
            IsOwner = playlist.IdUser == CurrentUserId;
            IsPrivate = playlist.IsPrivate;
            PlaylistTitle = playlist.Title;
            OwnerName = playlist.OwnerUsername;
            PageTitleText = playlist.Title + " \u00B7 PoliMusic";
            CreationDateText = playlist.CreationDate.ToString("dd/MM/yyyy");
            PlaylistArt = MediaResolver.ResolveWithFallback(
                playlist.Photo, Constants.DEFAULT_PLAYLIST_PHOTO);
            VisibilityHtml = playlist.IsPrivate
                ? "<span class=\"pm-badge pm-badge-private\">Privada</span>"
                : "<span class=\"pm-badge pm-badge-public\">P\u00fablica</span>";

            if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                ProcessPost(playlist);
                return;
            }

            LoadRows();
        }

        // --------------------------------------------------------- acciones --

        private void ProcessPost(Playlist playlist)
        {
            if (!ValidateCsrf())
            {
                ErrorMessage = "La sesion del formulario expiro. Recargue la pagina e intente de nuevo.";
                LoadRows();
                return;
            }

            string action = FormStr("action");

            if (action == "delete")
            {
                if (!IsOwner)
                {
                    DenyAccess();
                    return;
                }
                OperationResult deleted = _playlists.DeleteOwned(PlaylistId, CurrentUserId);
                RedirectWithFlash("~/Playlists.aspx", deleted.Message, !deleted.Success);
                return;
            }

            if (action == "addSongs")
            {
                if (!IsOwner)
                {
                    DenyAccess();
                    return;
                }
                AddSelectedSongs();
                return;
            }

            if (action == "update")
            {
                if (!IsOwner)
                {
                    DenyAccess();
                    return;
                }
                UpdatePlaylist();
                return;
            }

            // Boton "Quitar" de cada fila.
            int idSong = FormInt("removeSong");
            if (idSong > 0)
            {
                if (!IsOwner)
                {
                    DenyAccess();
                    return;
                }
                OperationResult removed = _playlists.RemoveSong(PlaylistId, CurrentUserId, idSong);
                RedirectWithFlash("~/PlaylistDetalle.aspx?id=" + PlaylistId, removed.Message, !removed.Success);
                return;
            }

            LoadRows();
        }

        private void AddSelectedSongs()
        {
            string[] values = Request.Form.GetValues("songs");
            if (values == null || values.Length == 0)
            {
                ErrorMessage = "Seleccione al menos una cancion.";
                LoadRows();
                return;
            }

            int added = 0;
            foreach (string raw in values)
            {
                int idSong;
                if (!int.TryParse(raw, out idSong) || idSong <= 0)
                {
                    continue;
                }
                OperationResult result = _playlists.AddSong(PlaylistId, CurrentUserId, idSong);
                if (result.Success)
                {
                    added++;
                }
            }

            if (added == 0)
            {
                ErrorMessage = "No se agrego ninguna cancion: ya estaban en la playlist.";
                LoadRows();
                return;
            }

            RedirectWithFlash("~/PlaylistDetalle.aspx?id=" + PlaylistId,
                added + " cancion(es) agregada(s) a la playlist.", false);
        }

        private void UpdatePlaylist()
        {
            string title = FormStr("title");
            int type = FormInt("type");

            string photo = null;
            HttpPostedFile file = Request.Files["photo"];
            if (file != null && file.ContentLength > 0)
            {
                string error;
                photo = FileStorage.SaveImage(file, "~/PlaylistImages", title, out error);
                if (error != null)
                {
                    ErrorMessage = error;
                    LoadRows();
                    return;
                }
            }

            OperationResult result = _playlists.UpdateOwned(PlaylistId, CurrentUserId, title, type, photo);
            if (!result.Success)
            {
                ErrorMessage = result.Message;
                LoadRows();
                return;
            }

            RedirectWithFlash("~/PlaylistDetalle.aspx?id=" + PlaylistId, result.Message, false);
        }

        // ----------------------------------------------------------- vista ---

        private void LoadRows()
        {
            List<Song> songs = new SongBll().ReadByPlaylist(PlaylistId);
            SongCount = songs.Count;

            string queue = Ui.PlayQueue(songs, PlaylistArt);
            StringBuilder rows = new StringBuilder();
            int index = 1;
            foreach (Song song in songs)
            {
                string actions = string.Empty;
                if (IsOwner)
                {
                    actions = "<button type=\"submit\" class=\"pm-btn pm-btn-danger pm-btn-sm\" " +
                              "name=\"removeSong\" value=\"" + song.IdSong + "\" " +
                              "data-pm-confirm=\"\u00BFQuitar esta canci\u00F3n de la playlist?\">Quitar</button>";
                }
                rows.Append(Ui.SongRow(song, index, queue, actions));
                index++;
            }
            RowsHtml = rows.ToString();

            if (IsOwner)
            {
                LoadCatalogOptions(songs);
            }
        }

        private void LoadCatalogOptions(List<Song> current)
        {
            List<Song> catalog = new SongBll().Read(null);
            StringBuilder sb = new StringBuilder();

            foreach (Song song in catalog)
            {
                bool already = false;
                foreach (Song item in current)
                {
                    if (item.IdSong == song.IdSong)
                    {
                        already = true;
                        break;
                    }
                }
                if (already)
                {
                    continue;
                }

                sb.Append("<label class=\"pm-check\" style=\"margin-bottom:9px;\">");
                sb.Append("<input type=\"checkbox\" name=\"songs\" value=\"").Append(song.IdSong).Append("\" />");
                sb.Append("<span><strong>").Append(Ui.E(song.SongName)).Append("</strong>");
                if (!string.IsNullOrEmpty(song.ArtistLabel))
                {
                    sb.Append(" <span style=\"color:var(--text-faint);\">\u2014 ")
                      .Append(Ui.E(song.ArtistLabel)).Append("</span>");
                }
                sb.Append("</span></label>");
            }

            CatalogOptionsHtml = sb.ToString();
        }
    }
}
