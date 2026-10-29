using System;
using System.Collections.Generic;
using System.Text;
using WebAppPoliMusicV2.BLL;
using WebAppPoliMusicV2.Entities;
using WebAppPoliMusicV2.Util;

namespace WebAppPoliMusicV2
{
    /// <summary>
    /// Vista administrativa de todas las playlists. Permite eliminarlas junto
    /// con sus vinculos en TBL_PLAYLIST_SONG.
    /// </summary>
    public partial class AdminPlaylists : PageBase
    {
        protected string RowsHtml;
        protected string ErrorMessage;

        private readonly PlaylistBll _playlists = new PlaylistBll();

        public AdminPlaylists()
        {
            RequiresAdmin = true;
            ActiveSection = "admin-playlists";
            RowsHtml = string.Empty;
            ErrorMessage = string.Empty;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                HandlePost();
                return;
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
                OperationResult deleted = _playlists.Delete(deleteId);
                RedirectWithFlash("~/Admin/Playlists.aspx", deleted.Message, !deleted.Success);
                return;
            }

            LoadRows();
        }

        private void LoadRows()
        {
            List<Playlist> playlists = _playlists.Read();
            StringBuilder sb = new StringBuilder();

            foreach (Playlist playlist in playlists)
            {
                string badge = playlist.IsPrivate
                    ? "<span class=\"pm-badge pm-badge-private\">Privada</span>"
                    : "<span class=\"pm-badge pm-badge-public\">P\u00fablica</span>";

                sb.Append("<tr>");
                sb.Append("<td data-label=\"Playlist\"><strong>").Append(Ui.E(playlist.Title)).Append("</strong></td>");
                sb.Append("<td data-label=\"Propietario\">").Append(Ui.E(playlist.OwnerUsername)).Append("</td>");
                sb.Append("<td data-label=\"Visibilidad\">").Append(badge).Append("</td>");
                sb.Append("<td data-label=\"Canciones\" class=\"pm-plays\">").Append(playlist.SongCount).Append("</td>");
                sb.Append("<td data-label=\"Creada\">").Append(playlist.CreationDate.ToString("dd/MM/yyyy")).Append("</td>");
                sb.Append("<td data-label=\"Acciones\"><div class=\"pm-row-actions\">");
                sb.Append("<a class=\"pm-btn pm-btn-ghost pm-btn-sm\" href=\"")
                  .Append(ResolveUrl("~/PlaylistDetalle.aspx?id=" + playlist.IdPlaylist)).Append("\">Ver</a>");
                sb.Append("<button type=\"submit\" class=\"pm-btn pm-btn-danger pm-btn-sm\" name=\"deleteId\" value=\"")
                  .Append(playlist.IdPlaylist)
                  .Append("\" data-pm-confirm=\"\u00BFEliminar la playlist y sus v\u00EDnculos con canciones?\">Eliminar</button>");
                sb.Append("</div></td>");
                sb.Append("</tr>");
            }
            RowsHtml = sb.ToString();
        }
    }
}
