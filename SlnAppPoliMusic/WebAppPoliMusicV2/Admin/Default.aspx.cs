using System;
using System.Collections.Generic;
using System.Text;
using WebAppPoliMusicV2.BLL;
using WebAppPoliMusicV2.Entities;

namespace WebAppPoliMusicV2
{
    /// <summary>
    /// Panel de administracion: resumen de las tablas principales y accesos
    /// directos a cada modulo de gestion. Exige rol de administrador.
    /// </summary>
    public partial class AdminDefault : PageBase
    {
        protected string StatsHtml;
        protected string UsersHtml;
        protected string SongsHtml;

        public AdminDefault()
        {
            RequiresAdmin = true;
            ActiveSection = "admin";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            LoadStats();
            LoadUsers();
            LoadSongs();
        }

        private void LoadStats()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Ui.StatCard(new UserBll().CountAll().ToString(), "Usuarios", "TBL_USER"));
            sb.Append(Ui.StatCard(new SongBll().CountAll().ToString(), "Canciones", "TBL_SONG"));
            sb.Append(Ui.StatCard(new ArtistBll().CountAll().ToString(), "Artistas", "TBL_ARTIST"));
            sb.Append(Ui.StatCard(new AlbumBll().CountAll().ToString(), "\u00C1lbumes", "TBL_ALBUM"));
            sb.Append(Ui.StatCard(new PlaylistBll().CountAll().ToString(), "Playlists", "TBL_PLAYLIST"));
            StatsHtml = sb.ToString();
        }

        private void LoadUsers()
        {
            List<User> users = new UserBll().Read();
            StringBuilder sb = new StringBuilder();
            int count = 0;
            foreach (User user in users)
            {
                if (count >= 6)
                {
                    break;
                }
                string badge = user.IsAdmin
                    ? "<span class=\"pm-badge pm-badge-admin\">Admin</span>"
                    : "<span class=\"pm-badge pm-badge-user\">Usuario</span>";

                sb.Append("<tr>");
                sb.Append("<td data-label=\"Usuario\"><strong>").Append(Ui.E(user.Username)).Append("</strong></td>");
                sb.Append("<td data-label=\"Correo\">").Append(Ui.E(user.Email)).Append("</td>");
                sb.Append("<td data-label=\"Rol\">").Append(badge).Append("</td>");
                sb.Append("<td data-label=\"Nacimiento\">")
                  .Append(user.Birthday.HasValue ? user.Birthday.Value.ToString("dd/MM/yyyy") : "&mdash;")
                  .Append("</td>");
                sb.Append("</tr>");
                count++;
            }
            UsersHtml = sb.ToString();
        }

        private void LoadSongs()
        {
            List<Song> songs = new SongBll().ReadMostPlayed(6);
            StringBuilder sb = new StringBuilder();
            int index = 1;
            foreach (Song song in songs)
            {
                sb.Append("<tr>");
                sb.Append("<td data-label=\"#\">").Append(index).Append("</td>");
                sb.Append("<td data-label=\"Canción\"><strong>").Append(Ui.E(song.SongName)).Append("</strong></td>");
                sb.Append("<td data-label=\"Artista\">").Append(Ui.E(song.ArtistLabel ?? "Sin artista")).Append("</td>");
                sb.Append("<td data-label=\"Reproducciones\" class=\"pm-plays\">").Append(song.PlayCount).Append("</td>");
                sb.Append("</tr>");
                index++;
            }
            SongsHtml = sb.ToString();
        }
    }
}
