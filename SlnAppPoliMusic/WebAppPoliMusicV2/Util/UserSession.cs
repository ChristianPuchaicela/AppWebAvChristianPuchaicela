using System;
using System.Web;
using System.Web.SessionState;
using WebAppPoliMusicV2.Entities;

namespace WebAppPoliMusicV2.Util
{
    /// <summary>
    /// Estado de sesion del usuario autenticado. Es la unica fuente de verdad
    /// sobre quien esta conectado y con que rol: las paginas nunca confian en
    /// un identificador de usuario recibido por la URL o el formulario.
    /// </summary>
    public static class UserSession
    {
        private static HttpSessionState Session
        {
            get
            {
                HttpContext ctx = HttpContext.Current;
                return ctx == null ? null : ctx.Session;
            }
        }

        public static User Current
        {
            get
            {
                HttpSessionState session = Session;
                return session == null ? null : session[Constants.SESSION_USER] as User;
            }
        }

        public static bool IsAuthenticated
        {
            get { return Current != null; }
        }

        public static bool IsAdmin
        {
            get
            {
                User user = Current;
                return user != null && user.IsAdmin;
            }
        }

        public static int CurrentUserId
        {
            get
            {
                User user = Current;
                return user == null ? 0 : user.IdUser;
            }
        }

        public static string CurrentUsername
        {
            get
            {
                User user = Current;
                return user == null ? string.Empty : user.Username;
            }
        }

        public static void SignIn(User user)
        {
            HttpSessionState session = Session;
            if (session != null)
            {
                session[Constants.SESSION_USER] = user;
            }
        }

        /// <summary>Refresca los datos del usuario guardados en la sesion.</summary>
        public static void Refresh(User user)
        {
            SignIn(user);
        }

        public static void SignOut()
        {
            HttpSessionState session = Session;
            if (session != null)
            {
                session.Remove(Constants.SESSION_USER);
                session.Remove(Constants.SESSION_CSRF);
                session.Abandon();
            }
        }

        /// <summary>
        /// Guarda un mensaje para mostrarlo despues de una redireccion
        /// (patron POST-Redirect-GET).
        /// </summary>
        public static void SetFlash(string message, bool isError)
        {
            HttpSessionState session = Session;
            if (session != null)
            {
                session[Constants.SESSION_FLASH] = (isError ? "E|" : "S|") + (message ?? string.Empty);
            }
        }

        /// <summary>Devuelve y limpia el mensaje pendiente.</summary>
        public static void ConsumeFlash(out string message, out bool isError)
        {
            message = null;
            isError = false;
            HttpSessionState session = Session;
            if (session == null)
            {
                return;
            }
            string raw = session[Constants.SESSION_FLASH] as string;
            if (string.IsNullOrEmpty(raw))
            {
                return;
            }
            session.Remove(Constants.SESSION_FLASH);
            isError = raw.StartsWith("E|", StringComparison.Ordinal);
            message = raw.Substring(2);
        }
    }
}
