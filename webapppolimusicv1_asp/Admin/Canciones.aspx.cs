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
    /// CRUD de canciones (solo administradores).
    ///
    /// La eliminacion usa SongRepository.DeleteCascade, que respeta las tres
    /// claves foraneas que apuntan a TBL_SONG (TBL_PLAYER, TBL_TRACKLIST y
    /// TBL_PLAYLIST_SONG) dentro de una transaccion.
    /// </summary>
    public partial class AdminCanciones : BasePage
    {
        public AdminCanciones()
        {
            // Solo administradores: la comprobacion ocurre en el servidor.
            RequiresAdmin = true;
        }

        protected List<Song> Songs = new List<Song>();
        protected List<string> Rows = new List<string>();
        protected Song EditSong { get; set; }

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

            ((SiteMaster)Master).ActiveSection = "admin-songs";

            if (!IsPostBack)
            {
                string action = Request.QueryString["action"];
                int id = QueryInt("id");

                if (string.Equals(action, "delete", StringComparison.OrdinalIgnoreCase) && id > 0)
                {
                    DeleteSong(id);
                    return;
                }
                if (id > 0)
                {
                    EditSong = SongRepository.GetById(id);
                    if (EditSong != null)
                    {
                        BindEditFields(EditSong);
                    }
                }
                else if (string.Equals(Request.QueryString["deleted"], "1", StringComparison.Ordinal))
                {
                    ShowMessage("La canción se eliminó correctamente.", false);
                }
                else if (string.Equals(Request.QueryString["created"], "1", StringComparison.Ordinal))
                {
                    ShowMessage("La canción se creó correctamente.", false);
                }
                else if (string.Equals(Request.QueryString["updated"], "1", StringComparison.Ordinal))
                {
                    ShowMessage("La canción se actualizó correctamente.", false);
                }
                else if (string.Equals(Request.QueryString["warning"], "nofile", StringComparison.Ordinal))
                {
                    string fileName = Server.UrlDecode(Request.QueryString["file"]);
                    ShowMessage("La canción se creó, pero la ruta «" + fileName +
                        "» no corresponde a ningún archivo en Assets/SongFiles, por lo que no podrá reproducirse. " +
                        "Copie el archivo de audio y vuelva a intentarlo.", true);
                }
            }

            LoadSongs();
        }

        private void LoadSongs()
        {
            Songs = SongRepository.GetAll();

            var artistOf = new Dictionary<int, string>();
            var albumOf = new Dictionary<int, string>();

            foreach (Song s in Songs)
            {
                artistOf[s.IdSong] = BuildArtistLabel(s.IdSong);
            }
            foreach (Album a in AlbumRepository.GetAll())
            {
                foreach (Song s in SongRepository.GetByAlbum(a.IdAlbum))
                {
                    string existing;
                    albumOf[s.IdSong] = albumOf.TryGetValue(s.IdSong, out existing)
                        ? existing + ", " + a.TitleAlbum
                        : a.TitleAlbum;
                }
            }

            foreach (Song s in Songs)
            {
                bool hasAudio = MediaResolver.Resolve(s.SongPath) != null;

                var sb = new StringBuilder();
                sb.Append("<tr>");
                sb.Append("<td data-label=\"ID\" class=\"pm-plays\">").Append(s.IdSong).Append("</td>");
                sb.Append("<td data-label=\"Canción\"><strong>").Append(H(s.SongName)).Append("</strong>");
                string album;
                if (albumOf.TryGetValue(s.IdSong, out album))
                {
                    sb.Append("<br><span style=\"font-size:0.76rem;color:#A9808B;\">Álbum: ").Append(H(album)).Append("</span>");
                }
                string artist;
                if (artistOf.TryGetValue(s.IdSong, out artist) && artist != "Sin artista")
                {
                    sb.Append("<br><span style=\"font-size:0.76rem;color:#A9808B;\">Artista: ").Append(H(artist)).Append("</span>");
                }
                sb.Append("</td>");
                sb.Append("<td data-label=\"Ruta\" style=\"font-size:0.79rem;word-break:break-all;\">")
                  .Append(H(s.SongPath)).Append("</td>");
                sb.Append("<td data-label=\"Audio\">");
                sb.Append(hasAudio
                    ? "<span class=\"pm-badge pm-badge-public\">Disponible</span>"
                    : "<span class=\"pm-badge pm-badge-private\">Falta archivo</span>");
                sb.Append("</td>");
                sb.Append("<td data-label=\"Plays\" class=\"pm-plays\">").Append(s.Plays ?? 0).Append("</td>");
                sb.Append("<td data-label=\"Acciones\"><div class=\"pm-row-actions\">");

                if (hasAudio)
                {
                    string play = Ui.PlayPayload(s, artistOf[s.IdSong], null);
                    sb.Append("<button type=\"button\" class=\"pm-btn pm-btn-sm pm-btn-ghost\" data-pm-play=\"")
                      .Append(HttpUtility.HtmlAttributeEncode(play)).Append("\">Reproducir</button>");
                }

                sb.Append("<a class=\"pm-btn pm-btn-sm pm-btn-ghost\" href=\"")
                  .Append(HttpUtility.HtmlAttributeEncode(ResolveUrl("~/Admin/Canciones.aspx?id=" + s.IdSong)))
                  .Append("\">Editar</a>");

                int inPlaylists = SongRepository.CountPlaylistsUsing(s.IdSong);
                int inAlbums = SongRepository.CountAlbumsUsing(s.IdSong);
                int inArtists = SongRepository.CountArtistsUsing(s.IdSong);

                var deps = new List<string>();
                if (inPlaylists > 0) { deps.Add(inPlaylists + " playlist(s)"); }
                if (inAlbums > 0) { deps.Add(inAlbums + " álbum(es)"); }
                if (inArtists > 0) { deps.Add(inArtists + " artista(s)"); }

                string confirm = deps.Count > 0
                    ? "¿Eliminar la canción «" + s.SongName + "»? También se quitará de "
                      + string.Join(", ", deps.ToArray()) + ". Esta acción no se puede deshacer."
                    : "¿Eliminar la canción «" + s.SongName + "»? Esta acción no se puede deshacer.";

                sb.Append("<a class=\"pm-btn pm-btn-sm pm-btn-danger\" href=\"")
                  .Append(HttpUtility.HtmlAttributeEncode(ResolveUrl("~/Admin/Canciones.aspx?action=delete&id="
                      + s.IdSong + "&token=" + GetCsrf())))
                  .Append("\" data-pm-confirm=\"")
                  .Append(HttpUtility.HtmlAttributeEncode(confirm))
                  .Append("\">Eliminar</a>");

                sb.Append("</div></td></tr>");
                Rows.Add(sb.ToString());
            }
        }

        private void DeleteSong(int id)
        {
            if (!IsCsrfValid())
            {
                ShowMessage("Solicitud no válida. Vuelva a intentarlo.", true);
                LoadSongs();
                return;
            }

            Song target = SongRepository.GetById(id);
            if (target == null)
            {
                ShowMessage("La canción indicada no existe.", true);
                LoadSongs();
                return;
            }

            try
            {
                SongRepository.DeleteCascade(id);
                Response.Redirect("~/Admin/Canciones.aspx?deleted=1", true);
            }
            catch (Exception ex)
            {
                ShowMessage("No se pudo eliminar la canción: " + ex.Message, true);
                LoadSongs();
            }
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            string name = (txtNewName.Text ?? string.Empty).Trim();
            string path = (txtNewPath.Text ?? string.Empty).Trim();
            int plays;
            int.TryParse(txtNewPlays.Text, out plays);

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(path))
            {
                ShowMessage("El nombre y la ruta del archivo son obligatorios.", true);
                return;
            }
            if (name.Length > 50)
            {
                ShowMessage("El nombre no puede superar los 50 caracteres.", true);
                return;
            }
            if (path.Length > 255)
            {
                ShowMessage("La ruta no puede superar los 255 caracteres.", true);
                return;
            }

            int newId = SongRepository.Create(name, path, plays);

            // Se informa si la ruta no corresponde a un archivo real.
            if (MediaResolver.Resolve(path) == null)
            {
                Response.Redirect("~/Admin/Canciones.aspx?id=" + newId + "&warning=nofile&file="
                    + HttpUtility.UrlEncode(path), true);
                return;
            }

            Response.Redirect("~/Admin/Canciones.aspx?created=1", true);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int id = QueryInt("id");
            Song target = SongRepository.GetById(id);
            if (target == null)
            {
                ShowMessage("La canción indicada no existe.", true);
                return;
            }

            string name = (txtName.Text ?? string.Empty).Trim();
            string path = (txtPath.Text ?? string.Empty).Trim();
            int plays;
            int.TryParse(txtPlays.Text, out plays);

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(path))
            {
                ShowMessage("El nombre y la ruta del archivo son obligatorios.", true);
                return;
            }

            SongRepository.Update(id, name, path, plays);

            // Actualizacion de las relaciones seleccionadas.
            int albumId;
            if (int.TryParse(ddlAlbum.SelectedValue, out albumId))
            {
                if (albumId > 0)
                {
                    SongRepository.SetAlbum(id, albumId);
                }
                else
                {
                    foreach (Album a in AlbumRepository.GetAll())
                    {
                        SongRepository.RemoveAlbum(id, a.IdAlbum);
                    }
                }
            }

            int artistId;
            if (int.TryParse(ddlArtist.SelectedValue, out artistId))
            {
                if (artistId > 0)
                {
                    SongRepository.SetArtist(id, artistId);
                }
                else
                {
                    foreach (Artist a in ArtistRepository.GetAll())
                    {
                        SongRepository.RemoveArtist(id, a.IdArtist);
                    }
                }
            }

            Response.Redirect("~/Admin/Canciones.aspx?updated=1", true);
        }

        private void BindEditFields(Song s)
        {
            txtName.Text = s.SongName;
            txtPath.Text = s.SongPath;
            txtPlays.Text = (s.Plays ?? 0).ToString();

            ddlAlbum.Items.Clear();
            ddlAlbum.Items.Add(new ListItem("— Sin álbum —", "0"));
            int currentAlbum = 0;
            foreach (Album a in AlbumRepository.GetAll())
            {
                ddlAlbum.Items.Add(new ListItem(a.TitleAlbum, a.IdAlbum.ToString()));
                foreach (Song inAlbum in SongRepository.GetByAlbum(a.IdAlbum))
                {
                    if (inAlbum.IdSong == s.IdSong)
                    {
                        currentAlbum = a.IdAlbum;
                    }
                }
            }
            ListItem albumItem = ddlAlbum.Items.FindByValue(currentAlbum.ToString());
            if (albumItem != null)
            {
                ddlAlbum.ClearSelection();
                albumItem.Selected = true;
            }

            ddlArtist.Items.Clear();
            ddlArtist.Items.Add(new ListItem("— Sin artista —", "0"));
            int currentArtist = 0;
            foreach (Artist a in ArtistRepository.GetAll())
            {
                ddlArtist.Items.Add(new ListItem(a.FullName, a.IdArtist.ToString()));
            }
            foreach (Artist a in SongRepository.GetArtistsOfSong(s.IdSong))
            {
                currentArtist = a.IdArtist;
            }
            ListItem artistItem = ddlArtist.Items.FindByValue(currentArtist.ToString());
            if (artistItem != null)
            {
                ddlArtist.ClearSelection();
                artistItem.Selected = true;
            }
        }

        private static string BuildArtistLabel(int songId)
        {
            List<Artist> artists = SongRepository.GetArtistsOfSong(songId);
            if (artists.Count == 0)
            {
                return "Sin artista";
            }
            var names = new List<string>();
            foreach (Artist a in artists)
            {
                names.Add(a.FullName);
            }
            return string.Join(", ", names.ToArray());
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
