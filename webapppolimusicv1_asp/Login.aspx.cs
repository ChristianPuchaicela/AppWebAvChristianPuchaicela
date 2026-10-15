using System;
using System.Web;
using System.Web.UI;
using WebAppPoliMusicV1.Models;
using WebAppPoliMusicV1.Services;

namespace WebAppPoliMusicV1
{
    /// <summary>
    /// Pantalla de inicio de sesion. Las credenciales se validan contra SQL Server
    /// y el rol se lee de TBL_USER.USER_TYPE en el servidor.
    /// </summary>
    public partial class Login : BasePage
    {
        public Login()
        {
            // Pagina publica: no exige sesion iniciada.
            IsPublicPage = true;
        }

        /// <summary>Mensaje informativo (por ejemplo, tras cerrar sesion o registrarse).</summary>
        protected string InfoMessage { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (UserContext.IsAuthenticated)
            {
                RedirectByRole();
                return;
            }

            string reason = Request.QueryString["msg"];
            if (reason == "logout")
            {
                InfoMessage = "Su sesión se cerró correctamente.";
            }
            else if (reason == "registered")
            {
                InfoMessage = "Su cuenta se creó correctamente. Ya puede iniciar sesión.";
            }
            else if (reason == "expired")
            {
                InfoMessage = "Su sesión expiró. Vuelva a iniciar sesión.";
            }

            if (!IsPostBack)
            {
                // Si la sesion ya no existe pero quedo una cookie de autenticacion
                // huerfana, se elimina para que no pueda reutilizarse.
                if (Request.Cookies["ASP.NET_SessionId"] == null)
                {
                    System.Web.Security.FormsAuthentication.SignOut();
                }
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string login = txtLogin.Text;
            string password = txtPassword.Text;

            AuthResult result = AuthService.Authenticate(login, password);

            if (!result.Success)
            {
                txtPassword.Text = string.Empty;
                ShowMessage(result.Error, true);
                return;
            }

            UserContext.SignIn(result.User);

            // Se regenera el identificador de sesion para evitar fijacion de sesion.
            Session["PM_LOGIN_TIME"] = DateTime.Now.ToString("o");
            System.Web.Security.FormsAuthentication.SetAuthCookie(result.User.Username, false);

            RedirectByRole();
        }

        /// <summary>Redirige al panel correspondiente al rol del usuario.</summary>
        private void RedirectByRole()
        {
            string returnUrl = Request.QueryString["returnUrl"];
            if (!string.IsNullOrEmpty(returnUrl) && IsLocalUrl(returnUrl))
            {
                Response.Redirect(returnUrl, true);
                return;
            }

            if (UserContext.IsAdmin)
            {
                Response.Redirect("~/Admin/Default.aspx", true);
            }
            Response.Redirect("~/Default.aspx", true);
        }

        /// <summary>Evita redirecciones abiertas a dominios externos.</summary>
        private static bool IsLocalUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }
            return url.StartsWith("/", StringComparison.Ordinal)
                && !url.StartsWith("//", StringComparison.Ordinal)
                && !url.StartsWith("/\\", StringComparison.Ordinal);
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
