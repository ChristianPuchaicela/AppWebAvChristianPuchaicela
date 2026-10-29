using System;
using WebAppPoliMusicV2.BLL;
using WebAppPoliMusicV2.Util;

namespace WebAppPoliMusicV2
{
    /// <summary>
    /// Inicio de sesion. Pagina publica: no exige sesion previa.
    ///
    /// La verificacion se hace contra TBL_USER con AuthService, que admite
    /// los tres formatos de contrasena que existen en BDD_PoliMusic.sql
    /// (texto plano heredado, SHA-1 heredado y PBKDF2 nuevo) y migra los
    /// heredados a PBKDF2 tras un acceso correcto.
    /// </summary>
    public partial class Login : PageBase
    {
        protected string ErrorMessage;
        protected string LoginValue;

        public Login()
        {
            IsPublicPage = true;
            ActiveSection = string.Empty;
            ErrorMessage = string.Empty;
            LoginValue = string.Empty;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (UserSession.IsAuthenticated)
            {
                Response.Redirect("~/Default.aspx", true);
                return;
            }

            if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                ProcessLogin();
            }
        }

        private void ProcessLogin()
        {
            LoginValue = FormStr("username");

            if (!ValidateCsrf())
            {
                ErrorMessage = "La sesion del formulario expiro. Intente nuevamente.";
                return;
            }

            string password = FormStr("password");
            AuthResult result = new AuthService().Authenticate(LoginValue, password);

            if (!result.Success)
            {
                ErrorMessage = result.Message;
                return;
            }

            UserSession.SignIn(result.User);

            // Solo se aceptan destinos locales: evita redirecciones abiertas.
            string returnUrl = QueryStr("returnUrl");
            if (IsLocalUrl(returnUrl))
            {
                Response.Redirect(returnUrl, true);
                return;
            }

            Response.Redirect("~/Default.aspx", true);
        }

        private static bool IsLocalUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }
            return url.StartsWith("/", StringComparison.Ordinal) &&
                   !url.StartsWith("//", StringComparison.Ordinal) &&
                   !url.StartsWith("/\\", StringComparison.Ordinal);
        }
    }
}
