using System;
using System.Globalization;
using System.Web;
using System.Web.UI;
using WebAppPoliMusicV2.Entities;
using WebAppPoliMusicV2.Util;

namespace WebAppPoliMusicV2
{
    /// <summary>
    /// Pagina base de PoliMusic. Aplica la politica de "acceso denegado por
    /// defecto": cualquier pagina que herede de PageBase exige sesion iniciada
    /// salvo que se marque como publica en el constructor.
    ///
    /// La comprobacion ocurre en el servidor, en OnInit, antes de procesar
    /// ningun control. No depende de ocultar botones en la interfaz.
    /// </summary>
    public abstract class PageBase : System.Web.UI.Page
    {
        /// <summary>Pagina accesible sin sesion (login, registro, error).</summary>
        protected bool IsPublicPage { get; set; }

        /// <summary>Exige ademas el rol de administrador.</summary>
        protected bool RequiresAdmin { get; set; }

        /// <summary>Seccion activa, usada para resaltar la navegacion.</summary>
        public string ActiveSection { get; set; }

        /// <summary>Mensaje pendiente de mostrar (patron POST-Redirect-GET).</summary>
        public string FlashMessage { get; private set; }

        public bool FlashIsError { get; private set; }

        protected PageBase()
        {
            IsPublicPage = false;
            RequiresAdmin = false;
            ActiveSection = string.Empty;
        }

        // --------------------------------------------------------- sesion ---

        protected User CurrentUser
        {
            get { return UserSession.Current; }
        }

        protected bool IsAuthenticated
        {
            get { return UserSession.IsAuthenticated; }
        }

        protected bool IsAdmin
        {
            get { return UserSession.IsAdmin; }
        }

        protected int CurrentUserId
        {
            get { return UserSession.CurrentUserId; }
        }

        // ------------------------------------------------- autorizacion ----

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            string message;
            bool isError;
            UserSession.ConsumeFlash(out message, out isError);
            FlashMessage = message;
            FlashIsError = isError;
            EnforceAuthorization();
        }

        private void EnforceAuthorization()
        {
            if (IsPublicPage)
            {
                return;
            }
            if (!UserSession.IsAuthenticated)
            {
                RedirectToLogin();
                return;
            }
            if (RequiresAdmin && !UserSession.IsAdmin)
            {
                DenyAccess();
            }
        }

        protected void RedirectToLogin()
        {
            string returnUrl = HttpUtility.UrlEncode(Request.RawUrl);
            Response.Redirect("~/Login.aspx?returnUrl=" + returnUrl, true);
        }

        protected void DenyAccess()
        {
            Response.StatusCode = 403;
            Response.Redirect("~/AccesoDenegado.aspx?from=" +
                HttpUtility.UrlEncode(Request.AppRelativeCurrentExecutionFilePath), true);
        }

        // --------------------------------------------------------- CSRF -----

        /// <summary>
        /// Token anti-CSRF de la sesion. Se incluye en cada formulario como
        /// campo oculto __PoliMusicToken y se valida en cada POST.
        /// </summary>
        public string CsrfToken
        {
            get
            {
                string token = Session[Constants.SESSION_CSRF] as string;
                if (string.IsNullOrEmpty(token))
                {
                    token = Guid.NewGuid().ToString("N");
                    Session[Constants.SESSION_CSRF] = token;
                }
                return token;
            }
        }

        public bool ValidateCsrf()
        {
            string expected = Session[Constants.SESSION_CSRF] as string;
            string received = Request.Form["__PoliMusicToken"];
            if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(received))
            {
                return false;
            }
            return string.Equals(expected, received, StringComparison.Ordinal);
        }

        // -------------------------------------------------- lectura de datos -

        protected int QueryInt(string name)
        {
            int value;
            return int.TryParse(Request.QueryString[name], out value) ? value : 0;
        }

        protected string QueryStr(string name)
        {
            string value = Request.QueryString[name];
            return value == null ? string.Empty : value.Trim();
        }

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

        protected bool FormBool(string name)
        {
            string value = Request.Form[name];
            return value == "1" || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "on", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Lee una fecha en formato ISO (yyyy-MM-dd) o dd/MM/yyyy.</summary>
        protected DateTime? FormDate(string name)
        {
            string raw = FormStr(name);
            if (raw.Length == 0)
            {
                return null;
            }
            DateTime value;
            string[] formats = { "yyyy-MM-dd", "dd/MM/yyyy", "dd-MM-yyyy", "MM/dd/yyyy" };
            if (DateTime.TryParseExact(raw, formats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out value))
            {
                return value;
            }
            if (DateTime.TryParse(raw, CultureInfo.CurrentCulture, DateTimeStyles.None, out value))
            {
                return value;
            }
            return null;
        }

        /// <summary>Redirige guardando un mensaje que se mostrara en la pagina destino.</summary>
        protected void RedirectWithFlash(string url, string message, bool isError)
        {
            UserSession.SetFlash(message, isError);
            Response.Redirect(url, true);
        }

        // ------------------------------------------------------ utilidades ---

        protected static string H(string value)
        {
            return HttpUtility.HtmlEncode(value ?? string.Empty);
        }

        protected static string A(string value)
        {
            return HttpUtility.HtmlAttributeEncode(value ?? string.Empty);
        }

        protected static string Url(string value)
        {
            return HttpUtility.HtmlAttributeEncode(value ?? string.Empty);
        }
    }
}
