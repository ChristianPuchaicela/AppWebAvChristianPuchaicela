using System;
using System.Web;

namespace WebAppPoliMusicV2
{
    /// <summary>
    /// Pagina de error configurada en Web.config (customErrors). No muestra
    /// trazas ni rutas fisicas: solo un mensaje generico y el tipo de error.
    /// </summary>
    public partial class ErrorPage : PageBase
    {
        protected string Detail;

        public ErrorPage()
        {
            IsPublicPage = true;
            Detail = "No se registro informacion adicional.";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Exception error = Server.GetLastError();
            if (error != null)
            {
                // Solo el tipo de excepcion: nunca el mensaje completo ni el stack.
                Detail = "Tipo de error: " + error.GetType().Name;
            }
        }
    }
}
