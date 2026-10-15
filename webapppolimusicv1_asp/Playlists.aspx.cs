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
    /// Gestion de playlists del usuario. Todas las operaciones de escritura
    /// verifican la propiedad del dato en el servidor mediante
    /// PlaylistRepository.GetOwned / UpdateOwned / DeleteOwned.
    /// </summary>
    public partial class Playlists : BasePage
    {
        protected List<Playlist> MyPlaylists = new List<Playlist>();
        protected List<Playlist> PublicPlaylists = new List<Playlist>();
        protected List<string> MyCards = new List<string>();
        protected List<string> PublicCards = new List<string>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UserContext.IsAuthenticated)
            {
                Response.Redirect("~/Login.aspx", true);
                return;
            }

            ((SiteMaster)Master).ActiveSection = "playlists";
            int userId = UserContext.CurrentUserId;

            if (!IsPostBack)
            {
                LoadData(userId);
                BindPhotoOptions();
                BindTypeOptions();
            }
        }

        private void LoadData(int userId)
        {
            MyPlaylists = PlaylistRepository.GetOwnedBy(userId);

            foreach (Playlist p in MyPlaylists)
            {
                string art = MediaResolver.ResolveWithFallback(p.Photo, "~/playlistImages/default.jpg");
                MyCards.Add(Ui.PlaylistCard(p, art, ResolveUrl("~/PlaylistDetalle.aspx?id=" + p.IdPlaylist)));
            }

            // Playlists publicas de otros usuarios: solo lectura.
            PublicPlaylists = PlaylistRepository.GetVisibleFor(userId);
            foreach (Playlist p in PublicPlaylists)
            {
                if (p.IdUser == userId)
                {
                    continue;
                }
                string art = MediaResolver.ResolveWithFallback(p.Photo, "~/playlistImages/default.jpg");
                PublicCards.Add(Ui.PlaylistCard(p, art, ResolveUrl("~/PlaylistDetalle.aspx?id=" + p.IdPlaylist)));
            }
        }

        /// <summary>Carga las portadas disponibles de la carpeta real de assets.</summary>
        private void BindPhotoOptions()
        {
            ddlNewPhoto.Items.Clear();
            ddlNewPhoto.Items.Add(new ListItem("Predeterminada", "~/playlistImages/default.jpg"));

            string physical = Server.MapPath("~/Assets/PlaylistImages");
            if (System.IO.Directory.Exists(physical))
            {
                var files = System.IO.Directory.GetFiles(physical);
                Array.Sort(files, StringComparer.OrdinalIgnoreCase);
                foreach (string f in files)
                {
                    string name = System.IO.Path.GetFileName(f);
                    if (string.Equals(name, "default.jpg", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    ddlNewPhoto.Items.Add(new ListItem(name, "~/playlistImages/" + name));
                }
            }
        }

        private void BindTypeOptions()
        {
            // Los valores provienen de TBL_CATALOG (PARENT = '2').
            ddlNewType.Items.Clear();
            foreach (CatalogItem item in CatalogRepository.GetByParent("2"))
            {
                int value;
                if (int.TryParse(item.CodCatalog, out value))
                {
                    ddlNewType.Items.Add(new ListItem(item.Value + " (" + item.CodCatalog + ")", item.CodCatalog));
                }
            }
            if (ddlNewType.Items.Count == 0)
            {
                ddlNewType.Items.Add(new ListItem("Pública", "0"));
                ddlNewType.Items.Add(new ListItem("Privada", "1"));
            }
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            string title = (txtNewTitle.Text ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(title))
            {
                ShowMessage(pnlError, lblError, "El título de la playlist es obligatorio.");
                return;
            }
            if (title.Length > 50)
            {
                ShowMessage(pnlError, lblError, "El título no puede superar los 50 caracteres.");
                return;
            }

            int type = 0;
            int.TryParse(ddlNewType.SelectedValue, out type);
            string photo = ddlNewPhoto.SelectedValue;

            // El propietario se toma de la sesion, nunca del formulario.
            int newId = PlaylistRepository.Create(
                UserContext.CurrentUserId, title, DateTime.Now, type, photo);

            Response.Redirect("~/PlaylistDetalle.aspx?id=" + newId + "&created=1", true);
        }

        private void ShowMessage(Panel panel, Label label, string message)
        {
            label.Text = HttpUtility.HtmlEncode(message);
            panel.Visible = true;
        }
    }
}
