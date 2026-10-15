using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebAppPoliMusicV1.Models;
using WebAppPoliMusicV1.Services;

namespace WebAppPoliMusicV1
{
    /// <summary>
    /// Pagina base de PoliMusic. Aplica la politica de "acceso denegado por
    /// defecto": cualquier pagina que herede de BasePage exige sesion iniciada
    /// salvo que se marque explicitamente como publica en el constructor.
    ///
    /// La comprobacion ocurre en el servidor, en cada solicitud, antes de que
    /// la pagina se procese. No depende de ocultar botones en la interfaz.
    /// </summary>
    public abstract class BasePage : Page
    {
        /// <summary>Indica si la pagina admite usuarios sin sesion (login, registro).</summary>
        protected bool IsPublicPage { get; set; }

        /// <summary>Exige ademas el rol de administrador.</summary>
        protected bool RequiresAdmin { get; set; }

        /// <summary>Nombre de la seccion, usado para resaltar la navegacion.</summary>
        protected string ActiveSection { get; set; }

        protected BasePage()
        {
            IsPublicPage = false;
            RequiresAdmin = false;
        }

        /// <summary>Usuario autenticado de la sesion, o null.</summary>
        protected User CurrentUser
        {
            get
            {
                if (!UserContext.IsAuthenticated)
                {
                    return null;
                }
                return new User
                {
                    IdUser = UserContext.CurrentUserId,
                    Username = UserContext.CurrentUsername,
                    Email = UserContext.CurrentEmail,
                    UserPhoto = UserContext.CurrentPhoto,
                    UserType = UserContext.CurrentUserType
                };
            }
        }

        protected bool IsAuthenticated
        {
            get { return UserContext.IsAuthenticated; }
        }

        protected bool IsAdmin
        {
            get { return UserContext.IsAdmin; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnforceAuthorization();
        }

        /// <summary>
        /// Comprobacion de autorizacion en el servidor. Se ejecuta antes de
        /// cargar cualquier control de la pagina.
        /// </summary>
        private void EnforceAuthorization()
        {
            if (IsPublicPage)
            {
                // Aun siendo publica, un usuario ya autenticado que intente ver
                // el login se redirige a su panel.
                return;
            }

            if (!UserContext.IsAuthenticated)
            {
                RedirectToLogin();
                return;
            }

            if (RequiresAdmin && !UserContext.IsAdmin)
            {
                DenyAccess();
            }
        }

        protected void RedirectToLogin()
        {
            string returnUrl = HttpUtility.UrlEncode(Request.RawUrl);
            Response.Redirect("~/Login.aspx?returnUrl=" + returnUrl, true);
        }

        /// <summary>
        /// Respuesta de acceso denegado. Se registra el intento y se responde
        /// con 403 y una pagina propia, sin revelar detalles internos.
        /// </summary>
        protected void DenyAccess()
        {
            Response.StatusCode = 403;
            Response.StatusDescription = "Forbidden";
            Response.Redirect("~/AccesoDenegado.aspx?from=" + HttpUtility.UrlEncode(Request.AppRelativeCurrentExecutionFilePath), true);
        }

        /// <summary>
        /// Genera un token anti-CSRF almacenado en la sesion y lo devuelve para
        /// incluirlo en los formularios.
        /// </summary>
        protected string GetCsrfToken()
        {
            const string key = "PM_CSRF";
            string token = Session[key] as string;
            if (string.IsNullOrEmpty(token))
            {
                token = Guid.NewGuid().ToString("N");
                Session[key] = token;
            }
            return token;
        }

        /// <summary>Valida el token anti-CSRF de un POST.</summary>
        protected bool ValidateCsrf()
        {
            const string key = "PM_CSRF";
            string expected = Session[key] as string;
            string received = Request.Form["__PoliMusicToken"];
            if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(received))
            {
                return false;
            }
            return string.Equals(expected, received, StringComparison.Ordinal);
        }

        /// <summary>Muestra un mensaje de error uniforme en la pagina.</summary>
        protected void ShowMessage(Panel panel, Label label, string message, bool isError)
        {
            if (panel == null || label == null)
            {
                return;
            }
            label.Text = message;
            panel.Visible = true;
            panel.CssClass = isError ? "alert alert-error" : "alert alert-success";
        }

        /// <summary>Lee un entero de la cadena de consulta; devuelve 0 si no es valido.</summary>
        protected int QueryInt(string name)
        {
            int value;
            return int.TryParse(Request.QueryString[name], out value) ? value : 0;
        }

        /// <summary>Lee un entero del formulario; devuelve 0 si no es valido.</summary>
        protected int FormInt(string name)
        {
            int value;
            return int.TryParse(Request.Form[name], out value) ? value : 0;
        }

        protected string FormStr(string name)
        {
            string value = Request.Form[name];
            return value == null ? string.Empty : value.Trim();
        }

        /// <summary>Escapa texto para insertarlo en HTML de forma segura.</summary>
        protected static string H(string value)
        {
            return HttpUtility.HtmlEncode(value ?? string.Empty);
        }

        /// <summary>Escapa texto para usarlo dentro de un atributo o de JavaScript.</summary>
        protected static string Js(string value)
        {
            return HttpUtility.JavaScriptStringEncode(value ?? string.Empty);
        }

        /// <summary>Escapa una ruta para incluirla en un atributo HTML.</summary>
        protected static string Url(string value)
        {
            return HttpUtility.HtmlAttributeEncode(value ?? string.Empty);
        }
    }
}
