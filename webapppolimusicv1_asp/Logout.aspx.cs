using System;
using System.Web;
using System.Web.UI;
using WebAppPoliMusicV1.Services;

namespace WebAppPoliMusicV1
{
    /// <summary>
    /// Cierre de sesion. Invalida la sesion del servidor, elimina la cookie de
    /// sesion y la cookie de autenticacion, y no permite volver a usar la sesion
    /// cerrada mediante el boton Atras del navegador.
    /// </summary>
    public partial class Logout : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            UserContext.SignOut();

            // Evita que la pagina quede en cache del navegador: si el usuario
            // pulsa Atras, el navegador vuelve a solicitar la pagina al servidor
            // y la proteccion la redirige al login.
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            Response.AppendHeader("Pragma", "no-cache");

            Response.Redirect("~/Login.aspx?msg=logout", true);
        }
    }
}
