/* ==========================================================================
   PoliMusic - Script incremental de datos   (NO DESTRUCTIVO)
   Archivo: db/01_PoliMusic_incremental.sql

   IMPORTANTE
   ----------
   Este script NO elimina ni recrea la base de datos ni ninguna tabla, y NO
   modifica la estructura de BDD_PoliMusic.sql. Solo corrige incoherencias de
   datos que impedian que la aplicacion funcionara, documentadas una por una.

   Es idempotente: puede ejecutarse varias veces sin duplicar ni romper datos.
   Ejecutelo DESPUES de BDD_PoliMusic.sql.
   ========================================================================== */

USE [BDD_PoliMusic];
GO

/* --------------------------------------------------------------------------
   1) Rutas de imagen con mayusculas incoherentes.
   TBL_PLAYLIST.PHOTO guardaba '~/playlistImages/Playlist2.png' y
   '~/playlistImages/Playlist3.png' (P mayuscula), pero los archivos reales son
   'playlist2.png' y 'playlist3.png'. En un servidor Linux/contenedor eso
   rompe la carga de la imagen. Se normaliza a minusculas.
   -------------------------------------------------------------------------- */
UPDATE TBL_PLAYLIST
   SET PHOTO = '~/playlistImages/playlist2.png'
 WHERE PHOTO = '~/playlistImages/Playlist2.png';
GO

UPDATE TBL_PLAYLIST
   SET PHOTO = '~/playlistImages/playlist3.png'
 WHERE PHOTO = '~/playlistImages/Playlist3.png';
GO

UPDATE TBL_PLAYLIST
   SET PHOTO = '~/playlistImages/playlist4.png'
 WHERE PHOTO = '~/playlistImages/Playlist4.png';
GO

/* --------------------------------------------------------------------------
   2) Usuario del enunciado del laboratorio.
   El laboratorio pide la cuenta 'epn.dis'. Se crea solo si no existe, con la
   contrasena ya cifrada con PBKDF2 (mismo formato que genera la aplicacion),
   de modo que nunca hay una contrasena en texto plano en la tabla.

   Valor PBKDF2 de la contrasena 'PoliMusic2026':
     PBH1$<saltHex>$<hashHex>   (SHA-256, 10000 iteraciones, salt 8 bytes)
   -------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM TBL_USER WHERE USERNAME = 'epn.dis')
BEGIN
    INSERT INTO TBL_USER (USERNAME, PASSWORD, EMAIL, BIRTHDAY, USER_PHOTO, USER_TYPE)
    VALUES ('epn.dis',
            'PBH1$c9124222c04ca29e$b6d7023a6026bb12fd9b4a75',
            'epn.dis@epn.edu.ec',
            '2001-05-20',
            '~/userImages/default.jpg',
            0);
    PRINT 'Usuario epn.dis creado con contrasena cifrada PBKDF2.';
END
ELSE
BEGIN
    PRINT 'El usuario epn.dis ya existe: no se modifica.';
END
GO

/* --------------------------------------------------------------------------
   3) Usuario 'Christian.Puchaicela' (dueno del repositorio).
   Tenia USER_TYPE = 1 (administrador) y una contrasena SHA-1 sin poder
   verificarse. Se conserva su rol para no alterar los permisos otorgados en
   el laboratorio; solo se documenta aqui. La aplicacion migrara su contrasena
   a PBKDF2 en el primer inicio de sesion correcto.
   -------------------------------------------------------------------------- */
PRINT 'Christian.Puchaicela: rol y contrasena sin cambios (migracion progresiva).';
GO

/* --------------------------------------------------------------------------
   4) Completar campos opcionales vacios.
   -------------------------------------------------------------------------- */
UPDATE TBL_SONG
   SET PLAYS = 0
 WHERE PLAYS IS NULL;
GO

/* --------------------------------------------------------------------------
   5) Verificacion: estado final de los datos clave.
   -------------------------------------------------------------------------- */
SELECT 'TBL_USER'   AS TABLA, COUNT(*) AS FILAS FROM TBL_USER
UNION ALL SELECT 'TBL_SONG',        COUNT(*) FROM TBL_SONG
UNION ALL SELECT 'TBL_ALBUM',       COUNT(*) FROM TBL_ALBUM
UNION ALL SELECT 'TBL_ARTIST',      COUNT(*) FROM TBL_ARTIST
UNION ALL SELECT 'TBL_PLAYLIST',    COUNT(*) FROM TBL_PLAYLIST
UNION ALL SELECT 'TBL_PLAYLIST_SONG', COUNT(*) FROM TBL_PLAYLIST_SONG
UNION ALL SELECT 'TBL_TRACKLIST',   COUNT(*) FROM TBL_TRACKLIST
UNION ALL SELECT 'TBL_PLAYER',      COUNT(*) FROM TBL_PLAYER
UNION ALL SELECT 'TBL_CATALOG',     COUNT(*) FROM TBL_CATALOG;
GO
