using System;
using WebAppPoliMusicV2.BLL;
using WebAppPoliMusicV2.Util;

namespace WebAppPoliMusicV2
{
    /// <summary>
    /// Endpoint interno que registra una reproduccion (incremento atomico de
    /// TBL_SONG.PLAYS). Lo invoca el reproductor por AJAX al iniciar una
    /// cancion. No devuelve HTML: solo un codigo de estado.
    /// </summary>
    public partial class Play : PageBase
    {
        public Play()
        {
            IsPublicPage = true;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentType = "text/plain";
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);

            if (!UserSession.IsAuthenticated)
            {
                Response.StatusCode = 401;
                Response.Write("no-session");
                Response.End();
                return;
            }

            if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                Response.StatusCode = 405;
                Response.Write("method-not-allowed");
                Response.End();
                return;
            }

            if (!ValidateCsrf())
            {
                Response.StatusCode = 403;
                Response.Write("invalid-token");
                Response.End();
                return;
            }

            int idSong = FormInt("id");
            if (idSong <= 0)
            {
                Response.StatusCode = 400;
                Response.Write("invalid-id");
                Response.End();
                return;
            }

            new SongBll().IncrementPlays(idSong);
            Response.StatusCode = 204;
            Response.End();
        }
    }
}
