using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using WebAppPoliMusicV2.BLL;
using WebAppPoliMusicV2.Entities;
using WebAppPoliMusicV2.Util;

namespace WebAppPoliMusicV2
{
    /// <summary>Gestion de albumes (solo administradores).</summary>
    public partial class AdminAlbumes : PageBase
    {
        protected string RowsHtml;
        protected string ErrorMessage;
        protected string TitleValue;
        protected string YearValue;
        protected string ImageValue;
        protected bool IsEditing;
        protected int EditingId;
        protected string ModalOpenClass;

        private readonly AlbumBll _albums = new AlbumBll();

        public AdminAlbumes()
        {
            RequiresAdmin = true;
            ActiveSection = "admin-albums";
            RowsHtml = string.Empty;
            ErrorMessage = string.Empty;
            TitleValue = string.Empty;
            YearValue = string.Empty;
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
                OperationResult deleted = _albums.Delete(deleteId);
                RedirectWithFlash("~/Admin/Albumes.aspx", deleted.Message, !deleted.Success);
                return;
            }

            string action = FormStr("action");
            int id = FormInt("id");
            string title = FormStr("title");
            string yearRaw = FormStr("year");
            string image = FormStr("currentImage");

            int year;
            int? releaseYear = null;
            if (int.TryParse(yearRaw, out year))
            {
                releaseYear = year;
            }

            HttpPostedFile file = Request.Files["image"];
            if (file != null && file.ContentLength > 0)
            {
                string error;
                string saved = FileStorage.SaveImage(file, "~/AlbumImages", title, out error);
                if (error != null)
                {
                    ShowError(error, action, id, title, yearRaw, image);
                    return;
                }
                image = saved;
            }

            Album album = new Album
            {
                IdAlbum = id,
                TitleAlbum = title,
                ReleaseYear = releaseYear,
                AlbumImagePath = image
            };

            OperationResult result = action == "update"
                ? _albums.Update(album)
                : _albums.Save(album);

            if (!result.Success)
            {
                ShowError(result.Message, action, id, title, yearRaw, image);
                return;
            }

            RedirectWithFlash("~/Admin/Albumes.aspx", result.Message, false);
        }

        private void ShowError(string message, string action, int id, string title,
                               string year, string image)
        {
            ErrorMessage = message;
            IsEditing = action == "update";
            EditingId = id;
            TitleValue = title;
            YearValue = year;
            ImageValue = image;
            ModalOpenClass = " open";
            LoadRows();
        }

        private void LoadForEdit(int id)
        {
            Album album = _albums.ReadById(id);
            if (album == null)
            {
                return;
            }
            IsEditing = true;
            EditingId = album.IdAlbum;
            TitleValue = album.TitleAlbum;
            YearValue = album.ReleaseYear.HasValue ? album.ReleaseYear.Value.ToString() : string.Empty;
            ImageValue = album.AlbumImagePath;
            ModalOpenClass = " open";
        }

        private void LoadRows()
        {
            List<Album> albums = _albums.Read(null);
            StringBuilder sb = new StringBuilder();

            foreach (Album album in albums)
            {
                string image = string.IsNullOrEmpty(album.AlbumImagePath)
                    ? "&mdash;"
                    : Ui.E(album.AlbumImagePath);

                sb.Append("<tr>");
                sb.Append("<td data-label=\"Álbum\"><strong>").Append(Ui.E(album.TitleAlbum)).Append("</strong></td>");
                sb.Append("<td data-label=\"Año\">").Append(Ui.E(album.ReleaseLabel)).Append("</td>");
                sb.Append("<td data-label=\"Canciones\" class=\"pm-plays\">").Append(album.SongCount).Append("</td>");
                sb.Append("<td data-label=\"Imagen\"><span style=\"font-size:0.78rem;\">").Append(image).Append("</span></td>");
                sb.Append("<td data-label=\"Acciones\"><div class=\"pm-row-actions\">");
                sb.Append("<a class=\"pm-btn pm-btn-ghost pm-btn-sm\" href=\"")
                  .Append(ResolveUrl("~/Admin/Albumes.aspx?edit=" + album.IdAlbum)).Append("\">Editar</a>");
                sb.Append("<button type=\"submit\" class=\"pm-btn pm-btn-danger pm-btn-sm\" name=\"deleteId\" value=\"")
                  .Append(album.IdAlbum)
                  .Append("\" data-pm-confirm=\"\u00BFEliminar este \u00E1lbum y su lista de canciones?\">Eliminar</button>");
                sb.Append("</div></td>");
                sb.Append("</tr>");
            }
            RowsHtml = sb.ToString();
        }
    }
}
