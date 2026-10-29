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
    /// Gestion de canciones (solo administradores). Una sola pagina resuelve
    /// listado, alta, edicion y borrado:
    ///   GET  ?edit=N  precarga el formulario
    ///   POST action=create|update  crea o actualiza
    ///   POST deleteId=N            elimina en cascada
    /// </summary>
    public partial class AdminCanciones : PageBase
    {
        protected string RowsHtml;
        protected string ErrorMessage;
        protected string ArtistOptionsHtml;
        protected string AlbumOptionsHtml;
        protected string NameValue;
        protected string PathValue;
        protected string PlaysValue;
        protected bool IsEditing;
        protected int EditingId;
        protected string ModalOpenClass;

        private readonly SongBll _songs = new SongBll();

        public AdminCanciones()
        {
            RequiresAdmin = true;
            ActiveSection = "admin-songs";
            RowsHtml = string.Empty;
            ErrorMessage = string.Empty;
            NameValue = string.Empty;
            PathValue = string.Empty;
            PlaysValue = "0";
            ModalOpenClass = string.Empty;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                HandlePost();
                return;
            }

            int editId = QueryInt("edit");
            if (editId > 0)
            {
                LoadForEdit(editId);
            }
            else
            {
                LoadSelects(0, 0);
            }
            LoadRows();
        }

        // ------------------------------------------------------------ POST ---

        private void HandlePost()
        {
            if (!ValidateCsrf())
            {
                ErrorMessage = "La sesion del formulario expiro. Recargue la pagina.";
                LoadSelects(0, 0);
                LoadRows();
                return;
            }

            int deleteId = FormInt("deleteId");
            if (deleteId > 0)
            {
                OperationResult deleted = _songs.Delete(deleteId);
                RedirectWithFlash("~/Admin/Canciones.aspx", deleted.Message, !deleted.Success);
                return;
            }

            string action = FormStr("action");
            int id = FormInt("id");
            string name = FormStr("name");
            string path = FormStr("path");
            int artistId = FormInt("artistId");
            int albumId = FormInt("albumId");
            int plays = FormInt("plays");

            HttpPostedFile file = Request.Files["songFile"];
            if (file != null && file.ContentLength > 0)
            {
                string error;
                string saved = FileStorage.SaveSong(file, "~/SongFiles", name, out error);
                if (error != null)
                {
                    ShowFormError(error, action, id, name, path, plays, artistId, albumId);
                    return;
                }
                path = saved;
            }

            if (string.IsNullOrEmpty(path))
            {
                ShowFormError("Debe subir un archivo MP3 o indicar la ruta del archivo.",
                    action, id, name, path, plays, artistId, albumId);
                return;
            }

            OperationResult result = action == "update"
                ? _songs.Update(id, name, path, plays, artistId, albumId)
                : _songs.Save(name, path, plays, artistId, albumId);

            if (!result.Success)
            {
                ShowFormError(result.Message, action, id, name, path, plays, artistId, albumId);
                return;
            }

            RedirectWithFlash("~/Admin/Canciones.aspx", result.Message, false);
        }

        private void ShowFormError(string message, string action, int id, string name,
                                   string path, int plays, int artistId, int albumId)
        {
            ErrorMessage = message;
            IsEditing = action == "update";
            EditingId = id;
            NameValue = name;
            PathValue = path;
            PlaysValue = plays.ToString();
            ModalOpenClass = " open";
            LoadSelects(artistId, albumId);
            LoadRows();
        }

        // ------------------------------------------------------------ vista --

        private void LoadForEdit(int id)
        {
            Song song = _songs.ReadById(id);
            if (song == null)
            {
                LoadSelects(0, 0);
                return;
            }

            IsEditing = true;
            EditingId = song.IdSong;
            NameValue = song.SongName;
            PathValue = song.SongPath;
            PlaysValue = song.PlayCount.ToString();
            ModalOpenClass = " open";

            LoadSelects(_songs.ReadArtistIdOfSong(id), _songs.ReadAlbumIdOfSong(id));
        }

        private void LoadSelects(int selectedArtist, int selectedAlbum)
        {
            StringBuilder artists = new StringBuilder();
            foreach (Artist artist in new ArtistBll().Read(null))
            {
                artists.Append("<option value=\"").Append(artist.IdArtist).Append("\"")
                       .Append(artist.IdArtist == selectedArtist ? " selected=\"selected\"" : string.Empty)
                       .Append(">").Append(Ui.E(artist.FullName)).Append("</option>");
            }
            ArtistOptionsHtml = artists.ToString();

            StringBuilder albums = new StringBuilder();
            foreach (Album album in new AlbumBll().Read(null))
            {
                albums.Append("<option value=\"").Append(album.IdAlbum).Append("\"")
                      .Append(album.IdAlbum == selectedAlbum ? " selected=\"selected\"" : string.Empty)
                      .Append(">").Append(Ui.E(album.TitleAlbum)).Append("</option>");
            }
            AlbumOptionsHtml = albums.ToString();
        }

        private void LoadRows()
        {
            List<Song> songs = _songs.Read(null);
            string queue = Ui.PlayQueue(songs, null);

            StringBuilder sb = new StringBuilder();
            int index = 1;
            foreach (Song song in songs)
            {
                string actions =
                    "<a class=\"pm-btn pm-btn-ghost pm-btn-sm\" href=\"" +
                    ResolveUrl("~/Admin/Canciones.aspx?edit=" + song.IdSong) + "\">Editar</a>" +
                    "<button type=\"submit\" class=\"pm-btn pm-btn-danger pm-btn-sm\" " +
                    "name=\"deleteId\" value=\"" + song.IdSong + "\" " +
                    "data-pm-confirm=\"\u00BFEliminar esta canci\u00F3n y sus v\u00EDnculos? \">Eliminar</button>";

                sb.Append(Ui.SongRow(song, index, queue, actions));
                index++;
            }
            RowsHtml = sb.ToString();
        }
    }
}
