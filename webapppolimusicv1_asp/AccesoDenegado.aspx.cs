using System;
using System.Web;
using System.Web.UI;
using WebAppPoliMusicV1.Models;
using WebAppPoliMusicV1.Services;

namespace WebAppPoliMusicV1
{
    /// <summary>
    /// Respuesta de acceso denegado. Se muestra cuando un usuario autenticado
    /// sin el rol adecuado intenta abrir una pagina administrativa.
    /// </summary>
    public partial class AccesoDenegado : BasePage
    {
        public AccesoDenegado()
        {
            // Pagina publica: no exige sesion iniciada.
            IsPublicPage = true;
        }

        protected string CurrentUserLabel { get; set; }
        protected string CurrentRoleName { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.StatusCode = 403;

            if (UserContext.IsAuthenticated)
            {
                CurrentUserLabel = UserContext.CurrentUsername;
                CurrentRoleName = UserRoles.Name(UserContext.CurrentUserType);
            }
        }

        protected static string H(string value)
        {
            return HttpUtility.HtmlEncode(value ?? string.Empty);
        }
    }
}
