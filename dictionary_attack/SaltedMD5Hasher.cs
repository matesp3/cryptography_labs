using System.Security.Cryptography;
using System.Text;

namespace dictionary_attack
{
    internal class SaltedMD5Hasher : IDisposable
    {

        private readonly MD5 md5;

        public SaltedMD5Hasher()
        {
            md5 = MD5.Create();
        }

        /// <summary>
        /// Funkcia vygeneruje nahodnu sol pouzivanu pri ukladani odtalcku hesiel pouzivatelov
        /// </summary>
        /// <param name="lengthInBytes">pocet nahodne vygenerovanych bajtov</param>
        /// <returns></returns>
        public static string createSalt(int lengthInBytes)
        {
            // pripravim buffer, do ktoreho vygenerujem nahodne cislo
            byte[] buffer = new byte[lengthInBytes];
            // pomocny objekt pre generovanie nahody
            Random rnd = new Random();
            // vygenerujem nahodne cislo
            rnd.NextBytes(buffer);
            // vratim nahodu ako Base64 string
            return Convert.ToBase64String(buffer);
        }

        /// <summary>
        /// Funkcia na vypocet odtlacku zadaneho hesla s pouzitim zadanej soli
        /// </summary>
        /// <param name="passwd">heslo</param>
        /// <param name="salt">sol</param>
        /// <returns>vytvoreny odtlacok skonvertovany do Base64 string</returns>
        public string crypt(string passwd, string salt)
        {
            // retazec skonvertujem na pole bajtov
            byte[] b = Encoding.UTF8.GetBytes(passwd + salt); // odtlacok budem pocitat z retazca pozostavajuceho z hesla a soli
            byte[] h = md5.ComputeHash(b); // vypocitam MD5 odtlacok hesla a soli
            return Convert.ToBase64String(h); // vratim hash ako Base64 string
        }

        public void Dispose()
        {
            md5.Dispose();
        }
    }
}
