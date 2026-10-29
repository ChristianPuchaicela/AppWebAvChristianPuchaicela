using System;
using System.Web;

namespace WebAppPoliMusicV2.Entities
{
    /// <summary>
    /// Fila de [dbo].[TBL_USER]. La contrasena que viaja en esta entidad es
    /// siempre el valor ALMACENADO (hash PBKDF2, SHA-1 heredado o texto plano
    /// heredado); nunca se guarda una contrasena en claro desde la interfaz.
    /// </summary>
    public class User
    {
        public int IdUser { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public DateTime? Birthday { get; set; }
        public string UserPhoto { get; set; }
        public int UserType { get; set; }

        public bool IsAdmin
        {
            get { return UserType == Util.Constants.ID_USER_ADMIN; }
        }

        /// <summary>Nombre legible del rol, tomado del dominio de TBL_CATALOG.</summary>
        public string RoleName
        {
            get { return IsAdmin ? "Administrador" : "Usuario"; }
        }

        /// <summary>Iniciales para el avatar cuando no hay fotografia.</summary>
        public string Initials
        {
            get
            {
                string source = string.IsNullOrEmpty(Username) ? "?" : Username.Trim();
                string[] parts = source.Split(new[] { ' ', '.', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    return (parts[0].Substring(0, 1) + parts[1].Substring(0, 1)).ToUpperInvariant();
                }
                return source.Substring(0, Math.Min(2, source.Length)).ToUpperInvariant();
            }
        }

        /// <summary>Edad calculada a partir de BIRTHDAY, o null si no hay fecha.</summary>
        public int? Age
        {
            get
            {
                if (!Birthday.HasValue)
                {
                    return null;
                }
                DateTime today = DateTime.Today;
                int age = today.Year - Birthday.Value.Year;
                if (Birthday.Value.Date > today.AddYears(-age))
                {
                    age--;
                }
                return age < 0 ? (int?)null : age;
            }
        }
    }
}
