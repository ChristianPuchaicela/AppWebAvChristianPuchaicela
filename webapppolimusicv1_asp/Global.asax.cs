using System;
using System.Web;

namespace WebAppPoliMusicV1
{
    public partial class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Cualquier excepcion no controlada se registra y se muestra una pagina
        /// generica, sin filtrar detalles internos ni credenciales al usuario.
        /// </summary>
        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            if (ex != null)
            {
                System.Diagnostics.Trace.TraceError("[PoliMusic] " + ex);
            }
            Server.ClearError();
            Response.Redirect("~/Error.aspx?code=500", true);
        }
    }
}
