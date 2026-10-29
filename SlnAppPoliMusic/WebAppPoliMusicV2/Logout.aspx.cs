using System;
using WebAppPoliMusicV2.Util;

namespace WebAppPoliMusicV2
{
    /// <summary>
    /// Cierre de sesion. Elimina el usuario y el token anti-CSRF de la sesion
    /// y abandona la sesion actual; despues vuelve al inicio de sesion.
    /// </summary>
    public partial class Logout : PageBase
    {
        public Logout()
        {
            IsPublicPage = true;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            UserSession.SignOut();
            Response.Redirect("~/Login.aspx", true);
        }
    }
}
