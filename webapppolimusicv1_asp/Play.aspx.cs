using System;
using System.Web;
using System.Web.UI;
using WebAppPoliMusicV1.Data;
using WebAppPoliMusicV1.Models;
using WebAppPoliMusicV1.Services;

namespace WebAppPoliMusicV1
{
    /// <summary>
    /// Endpoint interno que incrementa TBL_SONG.PLAYS cuando el reproductor
    /// inicia una cancion. Requiere sesion iniciada y token anti-CSRF.
    /// No devuelve HTML: solo un codigo de estado.
    /// </summary>
    public partial class Play : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentType = "application/json";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            if (!UserContext.IsAuthenticated)
            {
                Response.StatusCode = 401;
                Response.Write("{\"ok\":false,\"error\":\"no-autenticado\"}");
                return;
            }

            if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                Response.StatusCode = 405;
                Response.Write("{\"ok\":false,\"error\":\"metodo-no-permitido\"}");
                return;
            }

            string expected = Session["PM_CSRF"] as string;
            string received = Request.Form["token"];
            if (string.IsNullOrEmpty(expected) || !string.Equals(expected, received, StringComparison.Ordinal))
            {
                Response.StatusCode = 403;
                Response.Write("{\"ok\":false,\"error\":\"token-invalido\"}");
                return;
            }

            int songId;
            if (!int.TryParse(Request.QueryString["id"], out songId) || songId <= 0)
            {
                Response.StatusCode = 400;
                Response.Write("{\"ok\":false,\"error\":\"id-invalido\"}");
                return;
            }

            Song song = SongRepository.GetById(songId);
            if (song == null)
            {
                Response.StatusCode = 404;
                Response.Write("{\"ok\":false,\"error\":\"cancion-no-existe\"}");
                return;
            }

            // Solo se contabiliza si el archivo de audio existe realmente.
            if (MediaResolver.Resolve(song.SongPath) == null)
            {
                Response.StatusCode = 409;
                Response.Write("{\"ok\":false,\"error\":\"sin-audio\"}");
                return;
            }

            SongRepository.IncrementPlays(songId);
            Response.Write("{\"ok\":true}");
        }
    }
}
