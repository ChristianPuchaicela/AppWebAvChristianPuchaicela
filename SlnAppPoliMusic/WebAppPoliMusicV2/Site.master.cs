using System;
using System.Web;
using WebAppPoliMusicV2.Entities;
using WebAppPoliMusicV2.Util;

namespace WebAppPoliMusicV2
{
    /// <summary>
    /// Pagina maestra: barra lateral de navegacion, barra superior y
    /// reproductor persistente. Todo el estado que muestra (usuario, rol,
    /// token anti-CSRF) proviene de la sesion del servidor.
    /// </summary>
    public partial class SiteMaster : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack && !UserSession.IsAuthenticated)
            {
                // El master no decide el acceso: eso corresponde a PageBase.
                // Si se llega aqui sin sesion, se envia al login.
                Response.Redirect("~/Login.aspx", true);
            }
        }

        protected bool IsAdmin
        {
            get { return UserSession.IsAdmin; }
        }

        protected string CurrentUsername
        {
            get
            {
                User user = UserSession.Current;
                return user == null ? string.Empty : user.Username;
            }
        }

        protected string CurrentRole
        {
            get
            {
                User user = UserSession.Current;
                return user == null ? string.Empty : user.RoleName;
            }
        }

        protected string Initials
        {
            get
            {
                User user = UserSession.Current;
                return user == null ? "?" : user.Initials;
            }
        }

        /// <summary>Ruta web de la foto de perfil, ya validada en disco.</summary>
        protected string CurrentPhotoUrl
        {
            get
            {
                User user = UserSession.Current;
                if (user == null)
                {
                    return null;
                }
                return MediaResolver.ResolveWithFallback(user.UserPhoto, Constants.DEFAULT_USER_PHOTO);
            }
        }

        protected string CsrfToken
        {
            get
            {
                PageBase page = Page as PageBase;
                return page == null ? string.Empty : page.CsrfToken;
            }
        }

        protected string SearchValue
        {
            get
            {
                string value = Request.QueryString["q"];
                return value ?? string.Empty;
            }
        }

        /// <summary>Marca la opcion de navegacion correspondiente a la seccion activa.</summary>
        protected string NavClass(string section)
        {
            PageBase page = Page as PageBase;
            string active = page == null ? string.Empty : page.ActiveSection;
            return string.Equals(active, section, StringComparison.OrdinalIgnoreCase) ? "active" : string.Empty;
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
