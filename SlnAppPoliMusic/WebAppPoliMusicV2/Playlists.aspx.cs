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
    /// Gestion de playlists del usuario: creacion y listado. Las playlists
    /// privadas de otros usuarios nunca se muestran.
    /// </summary>
    public partial class Playlists : PageBase
    {
        protected string OwnHtml;
        protected string PublicHtml;
        protected int OwnCount;
        protected int PublicCount;
        protected string ErrorMessage;

        public Playlists()
        {
            ActiveSection = "playlists";
            OwnHtml = string.Empty;
            PublicHtml = string.Empty;
            ErrorMessage = string.Empty;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                ProcessPost();
                return;
            }
            LoadLists();
        }

        private void ProcessPost()
        {
            if (!ValidateCsrf())
            {
                ErrorMessage = "La sesion del formulario expiro. Intente nuevamente.";
                LoadLists();
                return;
            }

            string action = FormStr("action");
            if (action != "create")
            {
                LoadLists();
                return;
            }

            string title = FormStr("title");
            int type = FormInt("type");

            // Portada opcional.
            string photo = null;
            HttpPostedFile file = Request.Files["photo"];
            if (file != null && file.ContentLength > 0)
            {
                string error;
                photo = FileStorage.SaveImage(file, "~/PlaylistImages", title, out error);
                if (error != null)
                {
                    ErrorMessage = error;
                    LoadLists();
                    return;
                }
            }

            OperationResult result = new PlaylistBll().Create(CurrentUserId, title, type, photo);
            if (!result.Success)
            {
                ErrorMessage = result.Message;
                LoadLists();
                return;
            }

            RedirectWithFlash("~/PlaylistDetalle.aspx?id=" + result.Id, result.Message, false);
        }

        private void LoadLists()
        {
            List<Playlist> visible = new PlaylistBll().ReadVisibleFor(CurrentUserId);

            StringBuilder own = new StringBuilder();
            StringBuilder other = new StringBuilder();
            int ownCount = 0;
            int publicCount = 0;

            foreach (Playlist playlist in visible)
            {
                string art = MediaResolver.ResolveWithFallback(
                    playlist.Photo, Constants.DEFAULT_PLAYLIST_PHOTO);
                string url = ResolveUrl("~/PlaylistDetalle.aspx?id=" + playlist.IdPlaylist);

                if (playlist.IdUser == CurrentUserId)
                {
                    own.Append(Ui.PlaylistCard(playlist, art, url));
                    ownCount++;
                }
                else
                {
                    other.Append(Ui.PlaylistCard(playlist, art, url));
                    publicCount++;
                }
            }

            OwnHtml = own.ToString();
            PublicHtml = other.ToString();
            OwnCount = ownCount;
            PublicCount = publicCount;
        }
    }
}
