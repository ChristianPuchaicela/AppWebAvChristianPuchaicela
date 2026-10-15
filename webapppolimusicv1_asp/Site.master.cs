using System;
using System.Web;
using System.Web.UI;
using WebAppPoliMusicV1.Models;
using WebAppPoliMusicV1.Services;

namespace WebAppPoliMusicV1
{
    /// <summary>
    /// Pagina maestra de PoliMusic. Concentra el armazon visual (barra lateral,
    /// cabecera, reproductor persistente) y expone la sesion a las paginas hijas.
    ///
    /// Aplica la politica de acceso denegado por defecto: si la pagina hija no
    /// declara lo contrario, exige sesion iniciada.
    /// </summary>
    public partial class SiteMaster : System.Web.UI.MasterPage
    {
        /// <summary>
        /// Nombre de la seccion activa. Las paginas hijas lo asignan antes de
        /// que se renderice el menu para resaltar la opcion correspondiente.
        /// </summary>
        public string ActiveSection { get; set; }

        /// <summary>Indica si la pagina actual admite visitantes sin sesion.</summary>
        public bool IsPublicPage { get; set; }

        /// <summary>Exige rol de administrador para la pagina actual.</summary>
        public bool RequiresAdmin { get; set; }

        protected bool IsAdminUser
        {
            get { return UserContext.IsAdmin; }
        }

        protected string CurrentUsername
        {
            get { return UserContext.CurrentUsername ?? "Invitado"; }
        }

        protected string CurrentPhotoUrl
        {
            get { return MediaResolver.Resolve(UserContext.CurrentPhoto); }
        }

        protected string CurrentRoleName
        {
            get { return UserRoles.Name(UserContext.CurrentUserType); }
        }

        protected string Initials
        {
            get
            {
                string name = UserContext.CurrentUsername ?? "?";
                string trimmed = name.Trim();
                if (trimmed.Length == 0)
                {
                    return "?";
                }
                int dot = trimmed.IndexOf('.');
                if (dot > 0 && dot < trimmed.Length - 1)
                {
                    return (trimmed.Substring(0, 1) + trimmed.Substring(dot + 1, 1)).ToUpperInvariant();
                }
                return trimmed.Substring(0, 1).ToUpperInvariant();
            }
        }

        /// <summary>Token anti-CSRF que se inyecta en la pagina.</summary>
        protected string CsrfToken
        {
            get
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
        }

        /// <summary>Texto actual del buscador, para repintarlo.</summary>
        protected string SearchValue
        {
            get
            {
                string q = Request.QueryString["q"];
                return HttpUtility.HtmlAttributeEncode(q ?? string.Empty);
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            if (IsPublicPage || UserContext.IsAuthenticated)
            {
                return;
            }

            string returnUrl = HttpUtility.UrlEncode(Request.RawUrl);
            Response.Redirect("~/Login.aspx?returnUrl=" + returnUrl, true);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // La comprobacion de rol tambien se repite aqui: no basta con ocultar
            // los enlaces de administracion en el menu.
            if (RequiresAdmin && !UserContext.IsAdmin)
            {
                Response.StatusCode = 403;
                Response.Redirect("~/AccesoDenegado.aspx", true);
            }
        }

        /// <summary>Devuelve la clase CSS de la opcion de menu activa.</summary>
        protected string NavClass(string section)
        {
            return string.Equals(ActiveSection, section, StringComparison.OrdinalIgnoreCase)
                ? "active"
                : string.Empty;
        }

        protected static string H(string value)
        {
            return HttpUtility.HtmlEncode(value ?? string.Empty);
        }

        protected static string Url(string value)
        {
            return HttpUtility.HtmlAttributeEncode(value ?? string.Empty);
        }
    }
}
