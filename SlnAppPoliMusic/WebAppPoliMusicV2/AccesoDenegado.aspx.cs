using System;
using System.Web;
using WebAppPoliMusicV2.Util;

namespace WebAppPoliMusicV2
{
    /// <summary>
    /// Respuesta 403 con una pagina propia. No revela detalles internos ni
    /// rutas fisicas del servidor.
    /// </summary>
    public partial class AccesoDenegado : PageBase
    {
        protected string FromPath;

        public AccesoDenegado()
        {
            IsPublicPage = true;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.StatusCode = 403;
            Response.StatusDescription = "Forbidden";

            string from = QueryStr("from");
            FromPath = string.IsNullOrEmpty(from) ? "(no especificado)" : from;

            if (!UserSession.IsAuthenticated)
            {
                // Sin sesion no hay nada que hacer aqui: se vuelve al login.
                Response.Redirect("~/Login.aspx", true);
            }
        }
    }
}
