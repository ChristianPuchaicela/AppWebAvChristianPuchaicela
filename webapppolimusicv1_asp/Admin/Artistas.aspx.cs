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
    /// CRUD de artistas (solo administradores).
    ///
    /// Al eliminar un artista se eliminan sus vinculos en TBL_PLAYER y se
    /// conservan las canciones, porque TBL_SONG no depende de TBL_ARTIST.
    /// </summary>
    public partial class AdminArtistas : BasePage
    {
        public AdminArtistas()
        {
            // Solo administradores: la comprobacion ocurre en el servidor.
            RequiresAdmin = true;
        }

        protected List<Artist> Artists = new List<Artist>();
        protected List<string> Rows = new List<string>();
        protected Artist EditArtist { get; set; }

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

            ((SiteMaster)Master).ActiveSection = "admin-artists";

            if (!IsPostBack)
            {
                string action = Request.QueryString["action"];
                int id = QueryInt("id");

                if (string.Equals(action, "delete", StringComparison.OrdinalIgnoreCase) && id > 0)
                {
                    DeleteArtist(id);
                    return;
                }
                if (id > 0)
                {
                    EditArtist = ArtistRepository.GetById(id);
                    if (EditArtist != null)
                    {
                        BindEditFields(EditArtist);
                    }
                }
                else if (string.Equals(Request.QueryString["deleted"], "1", StringComparison.Ordinal))
                {
                    ShowMessage("El artista se eliminó correctamente.", false);
                }
                else if (string.Equals(Request.QueryString["created"], "1", StringComparison.Ordinal))
                {
                    ShowMessage("El artista se creó correctamente.", false);
                }
                else if (string.Equals(Request.QueryString["updated"], "1", StringComparison.Ordinal))
                {
                    ShowMessage("El artista se actualizó correctamente.", false);
                }
            }

            LoadArtists();
        }

        private void LoadArtists()
        {
            Artists = ArtistRepository.GetAll();
            Dictionary<int, int> counts = ArtistRepository.GetSongCounts();

            foreach (Artist a in Artists)
            {
                int songCount;
                if (!counts.TryGetValue(a.IdArtist, out songCount))
                {
                    songCount = 0;
                }

                string art = MediaResolver.Resolve(a.ArtistImage);
                bool hasImage = art != null;

                var sb = new StringBuilder();
                sb.Append("<tr>");
                sb.Append("<td data-label=\"ID\" class=\"pm-plays\">").Append(a.IdArtist).Append("</td>");
                sb.Append("<td data-label=\"Artista\"><strong>").Append(H(a.FullName)).Append("</strong></td>");
                sb.Append("<td data-label=\"Imagen\" style=\"font-size:0.78rem;word-break:break-all;\">")
                  .Append(H(a.ArtistImage ?? "—"));
                if (!hasImage)
                {
                    sb.Append(" <span class=\"pm-badge pm-badge-private\">No encontrada</span>");
                }
                sb.Append("</td>");
                sb.Append("<td data-label=\"Canciones\" class=\"pm-plays\">").Append(songCount).Append("</td>");
                sb.Append("<td data-label=\"Acciones\"><div class=\"pm-row-actions\">");
                sb.Append("<a class=\"pm-btn pm-btn-sm pm-btn-ghost\" href=\"")
                  .Append(HttpUtility.HtmlAttributeEncode(ResolveUrl("~/Admin/Artistas.aspx?id=" + a.IdArtist)))
                  .Append("\">Editar</a>");

                string confirm = songCount > 0
                    ? "¿Eliminar el artista «" + a.FullName + "»? Se quitarán sus vínculos con "
                      + songCount + " canción(es) en TBL_PLAYER. Las canciones se conservan. "
                      + "Esta acción no se puede deshacer."
                    : "¿Eliminar el artista «" + a.FullName + "»? Esta acción no se puede deshacer.";

                sb.Append("<a class=\"pm-btn pm-btn-sm pm-btn-danger\" href=\"")
                  .Append(HttpUtility.HtmlAttributeEncode(ResolveUrl("~/Admin/Artistas.aspx?action=delete&id="
                      + a.IdArtist + "&token=" + GetCsrf())))
                  .Append("\" data-pm-confirm=\"")
                  .Append(HttpUtility.HtmlAttributeEncode(confirm))
                  .Append("\">Eliminar</a>");
                sb.Append("</div></td></tr>");
                Rows.Add(sb.ToString());
            }
        }

        private void DeleteArtist(int id)
        {
            if (!IsCsrfValid())
            {
                ShowMessage("Solicitud no válida. Vuelva a intentarlo.", true);
                LoadArtists();
                return;
            }

            Artist target = ArtistRepository.GetById(id);
            if (target == null)
            {
                ShowMessage("El artista indicado no existe.", true);
                LoadArtists();
                return;
            }

            try
            {
                ArtistRepository.DeleteCascade(id);
                Response.Redirect("~/Admin/Artistas.aspx?deleted=1", true);
            }
            catch (Exception ex)
            {
                ShowMessage("No se pudo eliminar el artista: " + ex.Message, true);
                LoadArtists();
            }
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            string name = (txtNewName.Text ?? string.Empty).Trim();
            string lastname = (txtNewLastname.Text ?? string.Empty).Trim();
            string image = (txtNewImage.Text ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(name))
            {
                ShowMessage("El nombre del artista es obligatorio.", true);
                return;
            }
            if (name.Length > 100)
            {
                ShowMessage("El nombre no puede superar los 100 caracteres.", true);
                return;
            }
            if (string.IsNullOrEmpty(image))
            {
                image = "~/ArtistImages/artist1.jpg";
            }

            ArtistRepository.Create(name, lastname, image);
            Response.Redirect("~/Admin/Artistas.aspx?created=1", true);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int id = QueryInt("id");
            Artist target = ArtistRepository.GetById(id);
            if (target == null)
            {
                ShowMessage("El artista indicado no existe.", true);
                return;
            }

            string name = (txtName.Text ?? string.Empty).Trim();
            string lastname = (txtLastname.Text ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(name))
            {
                ShowMessage("El nombre del artista es obligatorio.", true);
                return;
            }

            string image = string.IsNullOrEmpty(ddlImage.SelectedValue)
                ? target.ArtistImage
                : ddlImage.SelectedValue;

            ArtistRepository.Update(id, name, lastname, image);
            Response.Redirect("~/Admin/Artistas.aspx?updated=1", true);
        }

        private void BindEditFields(Artist a)
        {
            txtName.Text = a.ArtistName;
            txtLastname.Text = a.ArtistLastname;

            ddlImage.Items.Clear();
            ddlImage.Items.Add(new ListItem("— Sin imagen —", string.Empty));

            string physical = Server.MapPath("~/Assets/ArtistImages");
            if (System.IO.Directory.Exists(physical))
            {
                var files = System.IO.Directory.GetFiles(physical);
                Array.Sort(files, StringComparer.OrdinalIgnoreCase);
                foreach (string f in files)
                {
                    string fileName = System.IO.Path.GetFileName(f);
                    ddlImage.Items.Add(new ListItem(fileName, "~/ArtistImages/" + fileName));
                }
            }

            ListItem current = ddlImage.Items.FindByValue(a.ArtistImage);
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
