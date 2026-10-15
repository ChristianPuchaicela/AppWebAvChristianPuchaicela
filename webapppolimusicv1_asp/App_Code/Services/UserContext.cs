using System;
using System.Web;
using System.Web.SessionState;
using WebAppPoliMusicV1.Models;

namespace WebAppPoliMusicV1.Services
{
    /// <summary>
    /// Estado de autenticacion del usuario en la sesion ASP.NET.
    ///
    /// El rol se lee SIEMPRE de la base de datos en el momento del login y se
    /// guarda en la sesion del servidor. Nunca se deduce del nombre de usuario
    /// ni del contenido de un parametro enviado por el navegador.
    /// </summary>
    public static class UserContext
    {
        private const string KeyUserId = "PM_ID_USER";
        private const string KeyUsername = "PM_USERNAME";
        private const string KeyEmail = "PM_EMAIL";
        private const string KeyPhoto = "PM_PHOTO";
        private const string KeyRole = "PM_USER_TYPE";

        public static bool IsAuthenticated
        {
            get
            {
                var session = HttpContext.Current != null ? HttpContext.Current.Session : null;
                return session != null && session[KeyUserId] != null;
            }
        }

        public static int CurrentUserId
        {
            get
            {
                var session = HttpContext.Current != null ? HttpContext.Current.Session : null;
                if (session == null || session[KeyUserId] == null)
                {
                    return 0;
                }
                return Convert.ToInt32(session[KeyUserId]);
            }
        }

        public static string CurrentUsername
        {
            get
            {
                var session = HttpContext.Current != null ? HttpContext.Current.Session : null;
                return session == null ? null : session[KeyUsername] as string;
            }
        }

        public static string CurrentEmail
        {
            get
            {
                var session = HttpContext.Current != null ? HttpContext.Current.Session : null;
                return session == null ? null : session[KeyEmail] as string;
            }
        }

        public static string CurrentPhoto
        {
            get
            {
                var session = HttpContext.Current != null ? HttpContext.Current.Session : null;
                return session == null ? null : session[KeyPhoto] as string;
            }
        }

        public static int CurrentUserType
        {
            get
            {
                var session = HttpContext.Current != null ? HttpContext.Current.Session : null;
                if (session == null || session[KeyRole] == null)
                {
                    return UserRoles.Normal;
                }
                return Convert.ToInt32(session[KeyRole]);
            }
        }

        public static bool IsAdmin
        {
            get { return IsAuthenticated && CurrentUserType == UserRoles.Admin; }
        }

        /// <summary>Registra al usuario autenticado en la sesion del servidor.</summary>
        public static void SignIn(User user)
        {
            HttpSessionState session = HttpContext.Current.Session;
            session.Clear();
            session[KeyUserId] = user.IdUser;
            session[KeyUsername] = user.Username;
            session[KeyEmail] = user.Email;
            session[KeyPhoto] = user.UserPhoto;
            session[KeyRole] = user.UserType;
        }

        /// <summary>
        /// Refresca los datos de sesion desde la base de datos. Se usa despues de
        /// editar el propio perfil para que la cabecera refleje los cambios.
        /// </summary>
        public static void Refresh(User user)
        {
            if (user == null)
            {
                return;
            }
            HttpSessionState session = HttpContext.Current.Session;
            session[KeyUsername] = user.Username;
            session[KeyEmail] = user.Email;
            session[KeyPhoto] = user.UserPhoto;
            session[KeyRole] = user.UserType;
        }

        /// <summary>Cierra la sesion y abandona la cookie de sesion.</summary>
        public static void SignOut()
        {
            HttpContext context = HttpContext.Current;
            if (context == null)
            {
                return;
            }
            context.Session.Clear();
            context.Session.Abandon();
            context.Response.Cookies.Remove("ASP.NET_SessionId");
            var expired = new HttpCookie("ASP.NET_SessionId", string.Empty)
            {
                Expires = DateTime.UtcNow.AddYears(-1),
                HttpOnly = true
            };
            context.Response.Cookies.Add(expired);
            System.Web.Security.FormsAuthentication.SignOut();
        }
    }
}
