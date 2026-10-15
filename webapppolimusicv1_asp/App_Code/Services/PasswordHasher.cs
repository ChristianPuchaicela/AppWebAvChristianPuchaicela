using System;
using System.Security.Cryptography;
using System.Text;

namespace WebAppPoliMusicV1.Services
{
    /// <summary>
    /// Cifrado de contrasenas con PBKDF2 (Rfc2898DeriveBytes / HMACSHA256),
    /// salt aleatorio por usuario.
    ///
    /// TBL_USER.PASSWORD es VARCHAR(50), por eso el formato usa hexadecimal:
    ///     PBH1$&lt;saltHex(16)&gt;$&lt;hashHex(24)&gt;   -> 46 caracteres
    ///
    /// Compatibilidad con los datos heredados de BDD_PoliMusic.sql:
    ///  - Texto plano (p. ej. '12345678').
    ///  - SHA-1 hexadecimal (p. ej. '7C222FB2927D828AF22F592134E8932480637C0D').
    /// En ambos casos la verificacion funciona y, tras un inicio de sesion
    /// correcto, la contrasena se re-cifra automaticamente con PBKDF2.
    /// </summary>
    public static class PasswordHasher
    {
        private const string Prefix = "PBH1";
        private const int SaltBytes = 8;
        private const int HashBytes = 12;
        private const int Iterations = 10000;

        public static bool IsModernHash(string stored)
        {
            return !string.IsNullOrEmpty(stored) && stored.StartsWith(Prefix + "$", StringComparison.Ordinal);
        }

        /// <summary>Genera un hash PBKDF2 nuevo con salt aleatorio.</summary>
        public static string Hash(string password)
        {
            byte[] salt = new byte[SaltBytes];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            byte[] hash = Derive(password, salt, HashBytes);
            return Prefix + "$" + ToHex(salt) + "$" + ToHex(hash);
        }

        private static byte[] Derive(string password, byte[] salt, int length)
        {
            using (var kdf = new Rfc2898DeriveBytes(password ?? string.Empty, salt, Iterations, HashAlgorithmName.SHA256))
            {
                return kdf.GetBytes(length);
            }
        }

        /// <summary>
        /// Verifica una contrasena contra el valor almacenado, sea PBKDF2,
        /// SHA-1 hexadecimal o texto plano heredado.
        /// </summary>
        public static bool Verify(string password, string stored)
        {
            if (string.IsNullOrEmpty(stored))
            {
                return false;
            }

            if (IsModernHash(stored))
            {
                string[] parts = stored.Split('$');
                if (parts.Length != 3)
                {
                    return false;
                }
                byte[] salt;
                byte[] expected;
                try
                {
                    salt = FromHex(parts[1]);
                    expected = FromHex(parts[2]);
                }
                catch (FormatException)
                {
                    return false;
                }
                if (salt.Length == 0 || expected.Length == 0)
                {
                    return false;
                }
                byte[] actual = Derive(password, salt, expected.Length);
                return FixedTimeEquals(actual, expected);
            }

            if (LooksLikeSha1Hex(stored))
            {
                return FixedTimeEquals(Sha1Hex(password), stored.ToUpperInvariant());
            }

            // Contrasena en texto plano heredada.
            return FixedTimeEquals(password ?? string.Empty, stored);
        }

        /// <summary>
        /// Indica si el valor almacenado deberia migrarse a PBKDF2 tras un
        /// inicio de sesion correcto.
        /// </summary>
        public static bool NeedsUpgrade(string stored)
        {
            return !string.IsNullOrEmpty(stored) && !IsModernHash(stored);
        }

        private static string ToHex(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        private static byte[] FromHex(string hex)
        {
            if (hex == null || hex.Length == 0 || hex.Length % 2 != 0)
            {
                throw new FormatException("Longitud hexadecimal invalida.");
            }
            byte[] result = new byte[hex.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return result;
        }

        private static bool LooksLikeSha1Hex(string value)
        {
            if (value.Length != 40)
            {
                return false;
            }
            foreach (char c in value)
            {
                bool hex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
                if (!hex)
                {
                    return false;
                }
            }
            return true;
        }

        private static string Sha1Hex(string password)
        {
            using (var sha1 = SHA1.Create())
            {
                byte[] bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(password ?? string.Empty));
                return ToHex(bytes).ToUpperInvariant();
            }
        }

        /// <summary>Comparacion en tiempo constante para no filtrar informacion por temporizacion.</summary>
        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }

        private static bool FixedTimeEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }
    }
}
