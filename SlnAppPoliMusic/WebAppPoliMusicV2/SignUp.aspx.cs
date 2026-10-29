using System;
using System.Web;
using WebAppPoliMusicV2.BLL;
using WebAppPoliMusicV2.Util;

namespace WebAppPoliMusicV2
{
    /// <summary>
    /// Registro de usuarios. Pagina publica.
    ///
    /// El rol se fija en el servidor con Constants.ID_USER_NORMAL: el
    /// formulario no puede enviar ni alterar el tipo de usuario. La foto es
    /// opcional y se valida antes de escribir en disco.
    /// </summary>
    public partial class SignUp : PageBase
    {
        protected string ErrorMessage;
        protected string UserNameValue;
        protected string EmailValue;
        protected string BirthdayValue;

        public SignUp()
        {
            IsPublicPage = true;
            ErrorMessage = string.Empty;
            UserNameValue = string.Empty;
            EmailValue = string.Empty;
            BirthdayValue = string.Empty;
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
                ProcessSignUp();
            }
        }

        private void ProcessSignUp()
        {
            UserNameValue = FormStr("username");
            EmailValue = FormStr("email");
            DateTime? birthday = FormDate("birthday");
            BirthdayValue = birthday.HasValue ? birthday.Value.ToString("yyyy-MM-dd") : FormStr("birthday");

            if (!ValidateCsrf())
            {
                ErrorMessage = "La sesion del formulario expiro. Intente nuevamente.";
                return;
            }

            string password = FormStr("password");
            string confirm = FormStr("confirm");

            // Foto opcional: si no se sube, se usa la imagen por defecto.
            string photo = null;
            HttpPostedFile file = Request.Files["photo"];
            if (file != null && file.ContentLength > 0)
            {
                string error;
                photo = FileStorage.SaveImage(file, "~/UserImages", UserNameValue, out error);
                if (error != null)
                {
                    ErrorMessage = error;
                    return;
                }
            }

            AuthResult result = new AuthService().Register(
                UserNameValue, EmailValue, password, confirm, birthday, photo);

            if (!result.Success)
            {
                ErrorMessage = result.Message;
                return;
            }

            RedirectWithFlash("~/Login.aspx", result.Message, false);
        }
    }
}
