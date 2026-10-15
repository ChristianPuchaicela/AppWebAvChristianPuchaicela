using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebAppPoliMusicV1.Data;
using WebAppPoliMusicV1.Models;
using WebAppPoliMusicV1.Services;

namespace WebAppPoliMusicV1.Admin
{
    /// <summary>
    /// CRUD de albumes (solo administradores).
    ///
    /// Al eliminar un album se eliminan sus filas en TBL_TRACKLIST y se
    /// conservan las canciones, porque TBL_SONG no depende de TBL_ALBUM.
    /// </summary>
    public partial class AdminAlbumes : BasePage
    {
        public AdminAlbumes()
        {
            // Solo administradores: la comprobacion ocurre en el servidor.
            RequiresAdmin = true;
        }

        protected List<Album> Albums = new List<Album>();
        protected List<string> Rows = new List<string>();
        protected Album EditAlbum { get; set; }
        protected List<Song> AlbumSongs = new List<Song>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UserContext.IsAuthenticated)
            {
                Response.Redirect("~/Login.aspx", true);
                return;
            }
            if (!UserContext.IsAdmin)
            {
                Response.StatusCode = 403;
                Response.Redirect("~/AccesoDenegado.aspx", true);
                return;
            }

            ((SiteMaster)Master).ActiveSection = "admin-albums";

            if (!IsPostBack)
            {
                string action = Request.QueryString["action"];
                int id = QueryInt("id");

                if (string.Equals(action, "delete", StringComparison.OrdinalIgnoreCase) && id > 0)
                {
                    DeleteAlbum(id);
                    return;
                }
                if (id > 0)
                {
                    EditAlbum = AlbumRepository.GetById(id);
                    if (EditAlbum != null)
                    {
                        AlbumSongs = SongRepository.GetByAlbum(id);
                        BindEditFields(EditAlbum);
                    }
                }
                else if (string.Equals(Request.QueryString["deleted"], "1", StringComparison.Ordinal))
                {
                    ShowMessage("El álbum se eliminó correctamente.", false);
                }
                else if (string.Equals(Request.QueryString["created"], "1", StringComparison.Ordinal))
                {
                    ShowMessage("El álbum se creó correctamente.", false);
                }
                else if (string.Equals(Request.QueryString["updated"], "1", StringComparison.Ordinal))
                {
                    ShowMessage("El álbum se actualizó correctamente.", false);
                }
            }

            LoadAlbums();
        }

        private void LoadAlbums()
        {
            Albums = AlbumRepository.GetAllWithCounts();

            foreach (Album a in Albums)
            {
                string art = MediaResolver.Resolve(a.AlbumImagePath);
                bool hasImage = art != null;

                var sb = new StringBuilder();
                sb.Append("<tr>");
                sb.Append("<td data-label=\"ID\" class=\"pm-plays\">").Append(a.IdAlbum).Append("</td>");
                sb.Append("<td data-label=\"Título\"><strong>").Append(H(a.TitleAlbum)).Append("</strong></td>");
                sb.Append("<td data-label=\"Año\" class=\"pm-plays\">")
                  .Append(a.ReleaseYear.HasValue ? a.ReleaseYear.Value.ToString() : "—").Append("</td>");
                sb.Append("<td data-label=\"Imagen\" style=\"font-size:0.78rem;word-break:break-all;\">")
                  .Append(H(a.AlbumImagePath ?? "—"));
                if (!hasImage)
                {
                    sb.Append(" <span class=\"pm-badge pm-badge-private\">No encontrada</span>");
                }
                sb.Append("</td>");
                sb.Append("<td data-label=\"Canciones\" class=\"pm-plays\">").Append(a.SongCount).Append("</td>");
                sb.Append("<td data-label=\"Acciones\"><div class=\"pm-row-actions\">");
                sb.Append("<a class=\"pm-btn pm-btn-sm pm-btn-ghost\" href=\"")
                  .Append(HttpUtility.HtmlAttributeEncode(ResolveUrl("~/Admin/Albumes.aspx?id=" + a.IdAlbum)))
                  .Append("\">Editar</a>");

                string confirm = a.SongCount > 0
                    ? "¿Eliminar el álbum «" + a.TitleAlbum + "»? Se quitarán sus "
                      + a.SongCount + " relación(es) en TBL_TRACKLIST. Las canciones se conservan. "
                      + "Esta acción no se puede deshacer."
                    : "¿Eliminar el álbum «" + a.TitleAlbum + "»? Esta acción no se puede deshacer.";

                sb.Append("<a class=\"pm-btn pm-btn-sm pm-btn-danger\" href=\"")
                  .Append(HttpUtility.HtmlAttributeEncode(ResolveUrl("~/Admin/Albumes.aspx?action=delete&id="
                      + a.IdAlbum + "&token=" + GetCsrf())))
                  .Append("\" data-pm-confirm=\"")
                  .Append(HttpUtility.HtmlAttributeEncode(confirm))
                  .Append("\">Eliminar</a>");
                sb.Append("</div></td></tr>");
                Rows.Add(sb.ToString());
            }
        }

        private void DeleteAlbum(int id)
        {
            if (!IsCsrfValid())
            {
                ShowMessage("Solicitud no válida. Vuelva a intentarlo.", true);
                LoadAlbums();
                return;
            }

            Album target = AlbumRepository.GetById(id);
            if (target == null)
            {
                ShowMessage("El álbum indicado no existe.", true);
                LoadAlbums();
                return;
            }

            try
            {
                AlbumRepository.DeleteCascade(id);
                Response.Redirect("~/Admin/Albumes.aspx?deleted=1", true);
            }
            catch (Exception ex)
            {
                ShowMessage("No se pudo eliminar el álbum: " + ex.Message, true);
                LoadAlbums();
            }
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            string title = (txtNewTitle.Text ?? string.Empty).Trim();
            string image = (txtNewImage.Text ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(title))
            {
                ShowMessage("El título del álbum es obligatorio.", true);
                return;
            }
            if (title.Length > 50)
            {
                ShowMessage("El título no puede superar los 50 caracteres.", true);
                return;
            }

            int year;
            int? parsedYear = null;
            if (int.TryParse(txtNewYear.Text, out year) && year >= 1900 && year <= 2100)
            {
                parsedYear = year;
            }

            if (string.IsNullOrEmpty(image))
            {
                image = "~/albumImages/album1.png";
            }

            AlbumRepository.Create(title, parsedYear, image);
            Response.Redirect("~/Admin/Albumes.aspx?created=1", true);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int id = QueryInt("id");
            Album target = AlbumRepository.GetById(id);
            if (target == null)
            {
                ShowMessage("El álbum indicado no existe.", true);
                return;
            }

            string title = (txtTitle.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(title))
            {
                ShowMessage("El título del álbum es obligatorio.", true);
                return;
            }

            int year;
            int? parsedYear = null;
            if (int.TryParse(txtYear.Text, out year) && year >= 1900 && year <= 2100)
            {
                parsedYear = year;
            }

            string image = string.IsNullOrEmpty(ddlImage.SelectedValue)
                ? target.AlbumImagePath
                : ddlImage.SelectedValue;

            AlbumRepository.Update(id, title, parsedYear, image);
            Response.Redirect("~/Admin/Albumes.aspx?updated=1", true);
        }

        private void BindEditFields(Album a)
        {
            txtTitle.Text = a.TitleAlbum;
            txtYear.Text = a.ReleaseYear.HasValue ? a.ReleaseYear.Value.ToString() : string.Empty;

            ddlImage.Items.Clear();
            ddlImage.Items.Add(new ListItem("— Sin imagen —", string.Empty));

            string physical = Server.MapPath("~/Assets/AlbumImages");
            if (System.IO.Directory.Exists(physical))
            {
                var files = System.IO.Directory.GetFiles(physical);
                Array.Sort(files, StringComparer.OrdinalIgnoreCase);
                foreach (string f in files)
                {
                    string fileName = System.IO.Path.GetFileName(f);
                    ddlImage.Items.Add(new ListItem(fileName, "~/albumImages/" + fileName));
                }
            }

            ListItem current = ddlImage.Items.FindByValue(a.AlbumImagePath);
            if (current != null)
            {
                ddlImage.ClearSelection();
                current.Selected = true;
            }
        }

        private bool IsCsrfValid()
        {
            string expected = Session["PM_CSRF"] as string;
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
