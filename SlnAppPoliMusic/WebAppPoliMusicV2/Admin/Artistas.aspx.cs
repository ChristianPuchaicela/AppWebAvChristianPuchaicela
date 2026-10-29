using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using WebAppPoliMusicV2.BLL;
using WebAppPoliMusicV2.Entities;
using WebAppPoliMusicV2.Util;

namespace WebAppPoliMusicV2
{
    /// <summary>Gestion de artistas (solo administradores).</summary>
    public partial class AdminArtistas : PageBase
    {
        protected string RowsHtml;
        protected string ErrorMessage;
        protected string NameValue;
        protected string LastnameValue;
        protected string ImageValue;
        protected bool IsEditing;
        protected int EditingId;
        protected string ModalOpenClass;

        private readonly ArtistBll _artists = new ArtistBll();

        public AdminArtistas()
        {
            RequiresAdmin = true;
            ActiveSection = "admin-artists";
            RowsHtml = string.Empty;
            ErrorMessage = string.Empty;
            NameValue = string.Empty;
            LastnameValue = string.Empty;
            ImageValue = string.Empty;
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
            LoadRows();
        }

        private void HandlePost()
        {
            if (!ValidateCsrf())
            {
                ErrorMessage = "La sesion del formulario expiro. Recargue la pagina.";
                LoadRows();
                return;
            }

            int deleteId = FormInt("deleteId");
            if (deleteId > 0)
            {
                OperationResult deleted = _artists.Delete(deleteId);
                RedirectWithFlash("~/Admin/Artistas.aspx", deleted.Message, !deleted.Success);
                return;
            }

            string action = FormStr("action");
            int id = FormInt("id");
            string name = FormStr("name");
            string lastname = FormStr("lastname");
            string image = FormStr("currentImage");

            HttpPostedFile file = Request.Files["image"];
            if (file != null && file.ContentLength > 0)
            {
                string error;
                string saved = FileStorage.SaveImage(file, "~/ArtistImages", name, out error);
                if (error != null)
                {
                    ShowError(error, action, id, name, lastname, image);
                    return;
                }
                image = saved;
            }

            Artist artist = new Artist
            {
                IdArtist = id,
                ArtistName = name,
                ArtistLastname = lastname,
                ArtistImage = image
            };

            OperationResult result = action == "update"
                ? _artists.Update(artist)
                : _artists.Save(artist);

            if (!result.Success)
            {
                ShowError(result.Message, action, id, name, lastname, image);
                return;
            }

            RedirectWithFlash("~/Admin/Artistas.aspx", result.Message, false);
        }

        private void ShowError(string message, string action, int id, string name,
                               string lastname, string image)
        {
            ErrorMessage = message;
            IsEditing = action == "update";
            EditingId = id;
            NameValue = name;
            LastnameValue = lastname;
            ImageValue = image;
            ModalOpenClass = " open";
            LoadRows();
        }

        private void LoadForEdit(int id)
        {
            Artist artist = _artists.ReadById(id);
            if (artist == null)
            {
                return;
            }
            IsEditing = true;
            EditingId = artist.IdArtist;
            NameValue = artist.ArtistName;
            LastnameValue = artist.ArtistLastname;
            ImageValue = artist.ArtistImage;
            ModalOpenClass = " open";
        }

        private void LoadRows()
        {
            List<Artist> artists = _artists.Read(null);
            StringBuilder sb = new StringBuilder();

            foreach (Artist artist in artists)
            {
                string image = string.IsNullOrEmpty(artist.ArtistImage)
                    ? "&mdash;"
                    : Ui.E(artist.ArtistImage);

                sb.Append("<tr>");
                sb.Append("<td data-label=\"Artista\"><strong>").Append(Ui.E(artist.FullName)).Append("</strong></td>");
                sb.Append("<td data-label=\"Canciones\" class=\"pm-plays\">").Append(artist.SongCount).Append("</td>");
                sb.Append("<td data-label=\"Imagen\"><span style=\"font-size:0.78rem;\">").Append(image).Append("</span></td>");
                sb.Append("<td data-label=\"Acciones\"><div class=\"pm-row-actions\">");
                sb.Append("<a class=\"pm-btn pm-btn-ghost pm-btn-sm\" href=\"")
                  .Append(ResolveUrl("~/Admin/Artistas.aspx?edit=" + artist.IdArtist)).Append("\">Editar</a>");
                sb.Append("<button type=\"submit\" class=\"pm-btn pm-btn-danger pm-btn-sm\" name=\"deleteId\" value=\"")
                  .Append(artist.IdArtist)
                  .Append("\" data-pm-confirm=\"\u00BFEliminar este artista y sus v\u00EDnculos con canciones?\">Eliminar</button>");
                sb.Append("</div></td>");
                sb.Append("</tr>");
            }
            RowsHtml = sb.ToString();
        }
    }
}
