using System;

namespace WebAppPoliMusicV1.Models
{
    /// <summary>
    /// Refleja exactamente [dbo].[TBL_USER].
    /// USER_TYPE = 0 -> usuario normal, 1 -> administrador (ver TBL_CATALOG).
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
            get { return UserType == UserRoles.Admin; }
        }

        public string DisplayName
        {
            get { return string.IsNullOrEmpty(Username) ? "(sin nombre)" : Username; }
        }
    }

    /// <summary>
    /// Valores de USER_TYPE tomados de TBL_CATALOG (COD_CATALOG_PARENT = '1').
    /// </summary>
    public static class UserRoles
    {
        public const int Normal = 0;
        public const int Admin = 1;

        public static string Name(int userType)
        {
            return userType == Admin ? "Administrador" : "Usuario";
        }
    }

    /// <summary>
    /// TBL_PLAYLIST.TYPE segun TBL_CATALOG (COD_CATALOG_PARENT = '2').
    /// </summary>
    public static class PlaylistTypes
    {
        public const int Public = 0;
        public const int Private = 1;

        public static string Name(int type)
        {
            return type == Private ? "Privada" : "Publica";
        }
    }
}
