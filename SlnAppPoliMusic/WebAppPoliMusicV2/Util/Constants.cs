namespace WebAppPoliMusicV2.Util
{
    /// <summary>
    /// Constantes de dominio de PoliMusic. Los valores coinciden con los datos
    /// de BDD_PoliMusic.sql:
    ///   TBL_CATALOG parent '1' -> USER TYPE      0 = USER, 1 = ADMIN
    ///   TBL_CATALOG parent '2' -> PLAYLIST TYPE  0 = Publica, 1 = Privada
    /// </summary>
    public static class Constants
    {
        // ---- Tipos de usuario (TBL_USER.USER_TYPE) ----
        public const int ID_USER_NORMAL = 0;
        public const int ID_USER_ADMIN = 1;

        // ---- Tipos de playlist (TBL_PLAYLIST.TYPE) ----
        public const int ID_PLAYLIST_PUBLIC = 0;
        public const int ID_PLAYLIST_PRIVATE = 1;

        // ---- Codigos padre de TBL_CATALOG ----
        public const string CATALOG_USER_TYPE = "1";
        public const string CATALOG_PLAYLIST_TYPE = "2";

        // ---- Claves de sesion ----
        public const string SESSION_USER = "PM_USER";
        public const string SESSION_CSRF = "PM_CSRF";
        public const string SESSION_FLASH = "PM_FLASH";

        // ---- Rutas por defecto de los recursos ----
        public const string DEFAULT_USER_PHOTO = "~/UserImages/default.jpg";
        public const string DEFAULT_PLAYLIST_PHOTO = "~/PlaylistImages/default.jpg";

        // ---- Limites ----
        public const int MAX_SONG_NAME = 50;
        public const int MAX_TITLE = 50;
        public const int MAX_EMAIL = 50;
        public const int MAX_USERNAME = 50;
        public const int MAX_PASSWORD = 50;
        public const int MIN_PASSWORD = 8;
    }
}
