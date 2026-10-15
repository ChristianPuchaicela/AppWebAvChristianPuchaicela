using System;
using System.Web;
using System.Web.UI;

namespace WebAppPoliMusicV1
{
    /// <summary>
    /// Pagina de error generica. Nunca muestra detalles internos ni la cadena
    /// de conexion: solo el codigo y un mensaje util para el usuario.
    /// </summary>
    public partial class ErrorPage : BasePage
    {
        public ErrorPage()
        {
            // Pagina publica: no exige sesion iniciada.
            IsPublicPage = true;
        }

        protected string Code { get; set; }
        protected string Detail { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            string code = Request.QueryString["code"];
            Code = string.IsNullOrEmpty(code) ? "500" : code;

            switch (Code)
            {
                case "404":
                    Detail = "La página o el recurso solicitado no existe.";
                    break;
                case "403":
                    Detail = "No tiene permisos para acceder a este recurso.";
                    break;
                default:
                    Detail = "Ocurrió un error inesperado. Si el problema persiste, contacte al administrador.";
                    break;
            }
        }

        protected static string H(string value)
        {
            return HttpUtility.HtmlEncode(value ?? string.Empty);
        }
    }
}
