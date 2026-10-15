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
    /// CRUD de playlists (solo administradores).
    ///
    /// A diferencia del modulo de usuario, aqui no se aplica la restriccion de
    /// propiedad: el administrador puede editar y eliminar cualquier playlist.
    /// La eliminacion arrastra las filas de TBL_PLAYLIST_SONG en una transaccion.
    /// </summary>
    public partial class AdminPlaylists : BasePage
    {
        public AdminPlaylists()
        {
            // Solo administradores: la comprobacion ocurre en el servidor.
            RequiresAdmin = true;
        }

        protected List<Playlist> Playlists = new List<Playlist>();
        protected List<string> Rows = new List<string>();
        protected Playlist EditPlaylist { get; set; }
        protected List<Song> EditSongs = new List<Song>();

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

            ((SiteMaster)Master).ActiveSection = "admin-playlists";

            BindTypes();
            BindOwners();

            if (!IsPostBack)
            {
                string action = Request.QueryString["action"];
                int id = QueryInt("id");

                if (string.Equals(action, "delete", StringComparison.OrdinalIgnoreCase) && id > 0)
                {
                    DeletePlaylist(id);
                    return;
                }
                if (id > 0)
                {
                    EditPlaylist = PlaylistRepository.GetById(id);
                    if (EditPlaylist != null)
                    {
                        EditSongs = SongRepository.GetByPlaylist(id);
                        BindEditFields(EditPlaylist);
                    }
                }
                else if (string.Equals(Request.QueryString["deleted"], "1", StringComparison.Ordinal))
                {
                    ShowMessage("La playlist se eliminó correctamente.", false);
                }
                else if (string.Equals(Request.QueryString["created"], "1", StringComparison.Ordinal))
                {
                    ShowMessage("La playlist se creó correctamente.", false);
                }
                else if (string.Equals(Request.QueryString["updated"], "1", StringComparison.Ordinal))
                {
                    ShowMessage("La playlist se actualizó correctamente.", false);
                }
            }

            LoadPlaylists();
        }

        private void LoadPlaylists()
        {
            Playlists = PlaylistRepository.GetAll();

            foreach (Playlist p in Playlists)
            {
                var sb = new StringBuilder();
                sb.Append("<tr>");
                sb.Append("<td data-label=\"ID\" class=\"pm-plays\">").Append(p.IdPlaylist).Append("</td>");
                sb.Append("<td data-label=\"Título\"><strong>").Append(H(p.Title)).Append("</strong></td>");
                sb.Append("<td data-label=\"Propietario\">").Append(H(p.OwnerUsername)).Append("</td>");
                sb.Append("<td data-label=\"Visibilidad\">");
                sb.Append(p.IsPrivate
                    ? "<span class=\"pm-badge pm-badge-private\">Privada</span>"
                    : "<span class=\"pm-badge pm-badge-public\">Pública</span>");
                sb.Append("</td>");
                sb.Append("<td data-label=\"Canciones\" class=\"pm-plays\">").Append(p.SongCount).Append("</td>");
                sb.Append("<td data-label=\"Creada\" class=\"pm-plays\">")
                  .Append(p.CreationDate.ToString("dd/MM/yyyy")).Append("</td>");
                sb.Append("<td data-label=\"Acciones\"><div class=\"pm-row-actions\">");
                sb.Append("<a class=\"pm-btn pm-btn-sm pm-btn-ghost\" href=\"")
                  .Append(HttpUtility.HtmlAttributeEncode(ResolveUrl("~/Admin/Playlists.aspx?id=" + p.IdPlaylist)))
                  .Append("\">Editar</a>");

                string confirm = p.SongCount > 0
                    ? "¿Eliminar la playlist «" + p.Title + "» de " + p.OwnerUsername + "? "
                      + "Se quitarán sus " + p.SongCount + " canción(es) asociadas. "
                      + "Esta acción no se puede deshacer."
                    : "¿Eliminar la playlist «" + p.Title + "» de " + p.OwnerUsername
                      + "? Esta acción no se puede deshacer.";

                sb.Append("<a class=\"pm-btn pm-btn-sm pm-btn-danger\" href=\"")
                  .Append(HttpUtility.HtmlAttributeEncode(ResolveUrl("~/Admin/Playlists.aspx?action=delete&id="
                      + p.IdPlaylist + "&token=" + GetCsrf())))
                  .Append("\" data-pm-confirm=\"")
                  .Append(HttpUtility.HtmlAttributeEncode(confirm))
                  .Append("\">Eliminar</a>");
                sb.Append("</div></td></tr>");
                Rows.Add(sb.ToString());
            }
        }

        private void DeletePlaylist(int id)
        {
            if (!IsCsrfValid())
            {
                ShowMessage("Solicitud no válida. Vuelva a intentarlo.", true);
                LoadPlaylists();
                return;
            }

            Playlist target = PlaylistRepository.GetById(id);
            if (target == null)
            {
                ShowMessage("La playlist indicada no existe.", true);
                LoadPlaylists();
                return;
            }

            try
            {
                PlaylistRepository.DeleteCascade(id);
                Response.Redirect("~/Admin/Playlists.aspx?deleted=1", true);
            }
            catch (Exception ex)
            {
                ShowMessage("No se pudo eliminar la playlist: " + ex.Message, true);
                LoadPlaylists();
            }
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            string title = (txtNewTitle.Text ?? string.Empty).Trim();
            int ownerId;
            int.TryParse(ddlNewOwner.SelectedValue, out ownerId);

            if (string.IsNullOrEmpty(title))
            {
                ShowMessage("El título de la playlist es obligatorio.", true);
                return;
            }
            if (ownerId <= 0)
            {
                ShowMessage("Debe seleccionar un propietario válido.", true);
                return;
            }
            if (UserRepository.GetById(ownerId) == null)
            {
                ShowMessage("El propietario indicado no existe.", true);
                return;
            }

            int type;
            int.TryParse(ddlNewType.SelectedValue, out type);

            PlaylistRepository.Create(ownerId, title, DateTime.Now, type, ddlNewPhoto.SelectedValue);
            Response.Redirect("~/Admin/Playlists.aspx?created=1", true);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int id = QueryInt("id");
            Playlist target = PlaylistRepository.GetById(id);
            if (target == null)
            {
                ShowMessage("La playlist indicada no existe.", true);
                return;
            }

            string title = (txtTitle.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(title))
            {
                ShowMessage("El título de la playlist es obligatorio.", true);
                return;
            }

            int type;
            int.TryParse(ddlType.SelectedValue, out type);

            int ownerId;
            int.TryParse(ddlOwner.SelectedValue, out ownerId);
            if (ownerId <= 0)
            {
                ownerId = target.IdUser;
            }

            // Actualizacion sin filtro de propiedad: es un administrador.
            Db.ExecuteNonQuery(
                "UPDATE TBL_PLAYLIST SET ID_USER=@u, TITLE=@t, TYPE=@ty, PHOTO=@ph WHERE ID_PLAYLIST=@id",
                Db.P("@u", ownerId), Db.P("@t", title), Db.P("@ty", type),
                Db.P("@ph", ddlPhoto.SelectedValue), Db.P("@id", id));

            Response.Redirect("~/Admin/Playlists.aspx?updated=1", true);
        }

        private void BindTypes()
        {
            ddlNewType.Items.Clear();
            ddlType.Items.Clear();

            List<CatalogItem> types = CatalogRepository.GetByParent("2");
            if (types.Count == 0)
            {
                types.Add(new CatalogItem { CodCatalog = "0", Value = "Public" });
                types.Add(new CatalogItem { CodCatalog = "1", Value = "Private" });
            }

            foreach (CatalogItem item in types)
            {
                ddlNewType.Items.Add(new ListItem(item.Value, item.CodCatalog));
                ddlType.Items.Add(new ListItem(item.Value, item.CodCatalog));
            }
        }

        private void BindOwners()
        {
            ddlNewOwner.Items.Clear();
            ddlOwner.Items.Clear();

            foreach (User u in UserRepository.GetAll())
            {
                string label = u.Username + (u.IsAdmin ? " (admin)" : string.Empty);
                ddlNewOwner.Items.Add(new ListItem(label, u.IdUser.ToString()));
                ddlOwner.Items.Add(new ListItem(label, u.IdUser.ToString()));
            }
        }

        private void BindPhotoOptions(DropDownList ddl, string selected)
        {
            ddl.Items.Clear();
            ddl.Items.Add(new ListItem("Predeterminada", "~/playlistImages/default.jpg"));

            string physical = Server.MapPath("~/Assets/PlaylistImages");
            if (System.IO.Directory.Exists(physical))
            {
                var files = System.IO.Directory.GetFiles(physical);
                Array.Sort(files, StringComparer.OrdinalIgnoreCase);
                foreach (string f in files)
                {
                    string fileName = System.IO.Path.GetFileName(f);
                    if (string.Equals(fileName, "default.jpg", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    ddl.Items.Add(new ListItem(fileName, "~/playlistImages/" + fileName));
                }
            }

            ListItem current = ddl.Items.FindByValue(selected);
            if (current != null)
            {
                ddl.ClearSelection();
                current.Selected = true;
            }
        }

        private void BindEditFields(Playlist p)
        {
            txtTitle.Text = p.Title;
            BindPhotoOptions(ddlPhoto, p.Photo);

            ListItem owner = ddlOwner.Items.FindByValue(p.IdUser.ToString());
            if (owner != null)
            {
                ddlOwner.ClearSelection();
                owner.Selected = true;
            }

            ListItem type = ddlType.Items.FindByValue(p.Type.ToString());
            if (type != null)
            {
                ddlType.ClearSelection();
                type.Selected = true;
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
