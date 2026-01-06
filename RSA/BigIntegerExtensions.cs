using System.Numerics;

namespace RSAcracker
{
    public static class BigIntegerExtensions
    {
        /// <summary>
        /// Rieši rovniciu: d === a^(-1) mod m, kde hľadá modulárny inverz čísla a modulo m, t.j. číslo d také, že (a * d) mod m = 1.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="m"></param>
        /// <returns>hodnota premennej d, t.j. modulárny inverz m čísla a^(-1)</returns>
        /// <exception cref="ArgumentException"></exception>
        public static BigInteger ModInverse(BigInteger a, BigInteger m)
        {
            BigInteger t = 0, newT = 1; // aktuálny koeficient a nový koeficient v postupnosti
            BigInteger r = m, newR = a; // aktuálny zvyšok a nový zvyšok v postupnosti

            while (newR != 0) // keď == 0, dosiahli sme GCD
            {
                BigInteger q = r / newR; // deliteľ

                // update prvkov postupnosti
                (t, newT) = (newT, t - q * newT);
                (r, newR) = (newR, r - q * newR);
            }

            if (r != 1) // inverzný prvok neexistuje, neplatný verejný exponent
                throw new ArgumentException("a nemá modulárny inverz modulo m");

            if (t < 0) // normalizácia výsledku na kladné číslo, pokiaľ je výsledok záporný
                t += m;

            return t;
        }

    }
}
