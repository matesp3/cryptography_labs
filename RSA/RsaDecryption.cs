
using System.Numerics;

namespace RSAcracker
{
    internal class RsaDecryption
    {
        /// <summary>
        /// Metóda na výpočet súkromného kľúča d RSA dešifrovania. Parametre sú dve prvočísla p a q, verejný exponent E a zašifrovaná správa Y.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <param name="e"></param>
        /// <param name="y"></param>
        /// <returns>Vypočítaný súkromný kľúč d</returns>
        public static BigInteger DecryptPrivateKey(BigInteger p, BigInteger q, BigInteger e, BigInteger y)
        {
            BigInteger phi = (p - 1) * (q - 1); // Eulerova funkcia φ(N) - počet prirodzených čísel menších/rovných ako N, ktoré sú nesúdeliteľné s N
            return BigIntegerExtensions.ModInverse(e, phi); // výpočet súkromného exponentu
        }

        /// <summary>
        /// Vykoná dešifrovanie správy Y pomocou súkromného exponentu d a modulu N podľa vzorca: x === Y^d mod N.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="n"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static BigInteger DecryptMessage(BigInteger d, BigInteger n, BigInteger y)
        {
            return BigInteger.ModPow(y, d, n); // dešifrovanie správy pomocou súkromného exponentu
        }

        /// <summary>
        /// Overí správnosť dešifrovania porovnaním pôvodnej zašifrovanej správy Y so znovu zašifrovanou správou získanou z dešifrovanej správy x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="e"></param>
        /// <param name="n"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool VerifyDecryption(BigInteger x, BigInteger e, BigInteger n, BigInteger y)
        {
            BigInteger yCheck = BigInteger.ModPow(x, e, n); // zašifrujeme dešifrovanú správu
            return yCheck == y; // porovnáme so vstupnou zašifrovanou správou
        }
    }
}
