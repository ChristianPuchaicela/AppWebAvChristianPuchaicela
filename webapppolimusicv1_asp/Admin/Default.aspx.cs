using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using WebAppPoliMusicV1.Data;
using WebAppPoliMusicV1.Models;
using WebAppPoliMusicV1.Services;

namespace WebAppPoliMusicV1.Admin
{
    /// <summary>
    /// Panel de administracion. Solo accesible para usuarios con
    /// TBL_USER.USER_TYPE = 1. La comprobacion se repite en Page_Load, de modo
    /// que no basta con ocultar el enlace en el menu.
    /// </summary>
    public partial class AdminDefault : BasePage
    {
        public AdminDefault()
        {
            // Solo administradores: la comprobacion ocurre en el servidor.
            RequiresAdmin = true;
        }

        protected int UserTotal { get; set; }
        protected int SongTotal { get; set; }
        protected int ArtistTotal { get; set; }
        protected int AlbumTotal { get; set; }
        protected int PlaylistTotal { get; set; }
        protected int AdminTotal { get; set; }
        protected int PlayableSongs { get; set; }
        protected string AudioNotice { get; set; }

        protected List<User> RecentUsers = new List<User>();
        protected List<string> RecentUserRows = new List<string>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UserContext.IsAuthenticated)
            {
                Response.Redirect("~/Login.aspx", true);
                return;
            }

            // Verificacion de rol en el servidor para cada solicitud.
            if (!UserContext.IsAdmin)
            {
                Response.StatusCode = 403;
                Response.Redirect("~/AccesoDenegado.aspx", true);
                return;
            }

            ((SiteMaster)Master).ActiveSection = "admin";

            List<User> users = UserRepository.GetAll();
            UserTotal = users.Count;
            AdminTotal = 0;
            foreach (User u in users)
            {
                if (u.IsAdmin)
                {
                    AdminTotal++;
                }
            }

            List<Song> songs = SongRepository.GetAll();
            SongTotal = songs.Count;
            PlayableSongs = 0;
            foreach (Song s in songs)
            {
                if (MediaResolver.Resolve(s.SongPath) != null)
                {
                    PlayableSongs++;
                }
            }

            ArtistTotal = ArtistRepository.GetAll().Count;
            AlbumTotal = AlbumRepository.GetAll().Count;
            PlaylistTotal = PlaylistRepository.CountAll();

            if (SongTotal > 0 && PlayableSongs < SongTotal)
            {
                AudioNotice = (SongTotal - PlayableSongs) + " canción(es) no tienen archivo de audio en " +
                              "Assets/SongFiles. Revise la columna SONG_PATH en el módulo de canciones.";
            }

            // Se muestran los ultimos usuarios por ID descendente.
            users.Sort((a, b) => b.IdUser.CompareTo(a.IdUser));
            int shown = 0;
            foreach (User u in users)
            {
                if (shown++ >= 6)
                {
                    break;
                }
                RecentUsers.Add(u);

                var sb = new StringBuilder();
                sb.Append("<tr>");
                sb.Append("<td data-label=\"Usuario\"><strong>").Append(H(u.Username)).Append("</strong></td>");
                sb.Append("<td data-label=\"Correo\">").Append(H(u.Email)).Append("</td>");
                sb.Append("<td data-label=\"Rol\">");
                sb.Append(u.IsAdmin
                    ? "<span class=\"pm-badge pm-badge-admin\">Administrador</span>"
                    : "<span class=\"pm-badge pm-badge-user\">Usuario</span>");
                sb.Append("</td>");
                sb.Append("<td data-label=\"Playlists\" class=\"pm-plays\">")
                  .Append(UserRepository.CountPlaylists(u.IdUser)).Append("</td>");
                sb.Append("</tr>");
                RecentUserRows.Add(sb.ToString());
            }
        }

        protected static string H(string value)
        {
            return HttpUtility.HtmlEncode(value ?? string.Empty);
        }
    }
}
