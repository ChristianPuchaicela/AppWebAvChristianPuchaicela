using System;
using System.Web;
using System.Web.UI;
using WebAppPoliMusicV1.Services;

namespace WebAppPoliMusicV1
{
    /// <summary>
    /// Enrolamiento de nuevos usuarios. El rol se asigna siempre en el servidor
    /// como Usuario normal: el formulario no expone ningun campo de rol.
    /// </summary>
    public partial class Registro : BasePage
    {
        public Registro()
        {
            // Pagina publica: no exige sesion iniciada.
            IsPublicPage = true;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (UserContext.IsAuthenticated)
            {
                Response.Redirect("~/Default.aspx", true);
            }
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string email = txtEmail.Text;
            string password = txtPassword.Text;
            string confirm = txtConfirm.Text;

            if (password != confirm)
            {
                ShowMessage("La contraseña y su confirmación no coinciden.", true);
                return;
            }

            DateTime birthday;
            DateTime? parsedBirthday = null;
            if (!string.IsNullOrEmpty(txtBirthday.Text) &&
                DateTime.TryParse(txtBirthday.Text, out birthday))
            {
                parsedBirthday = birthday;
            }

            AuthResult result = AuthService.Register(username, email, password, parsedBirthday, null);

            if (!result.Success)
            {
                ShowMessage(result.Error, true);
                return;
            }

            Response.Redirect("~/Login.aspx?msg=registered", true);
        }

        private void ShowMessage(string message, bool isError)
        {
            lblMessage.Text = HttpUtility.HtmlEncode(message);
            pnlMessage.Visible = true;
            pnlMessage.CssClass = "alert " + (isError ? "alert-error" : "alert-success");
        }
    }
}
