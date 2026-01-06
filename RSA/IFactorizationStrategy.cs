using System.Numerics;

namespace RSAcracker
{
    public interface IFactorizationStrategy
    {
        public string AlgorithmName { get; }

        /// <summary>
        /// Faktorizácia N na dva netriviálne faktory p, q také, že N = p * q a p < q.
        /// </summary>
        /// <param name="n"></param>
        /// <returns>(p, q), kde p << q</returns>
        public Task<(BigInteger p, BigInteger q)?> FactorizeAsync(BigInteger n);
    }

    public class PollardsRhoFactorization : IFactorizationStrategy
    {
        public string AlgorithmName => "PollardsRho algorithm";
        public Task<(BigInteger p, BigInteger q)?> FactorizeAsync(BigInteger n)
        {
            /* f(a) = a^2 + c (mod N), kde:
            - a je aktuálny prvok v sekvencii (začína sa na nami zvolenej hodnote, napr. 2)
            - c je konštanta (môže byť ľubovoľné číslo, ktoré nie je 0, dáme = 1, ak zlyhá, zvyšujeme o 2)
            - N je modulus (číslo, ktoré faktorizujeme)
            - f(a) je funkcia, ktorá generuje ďalší prvok v sekvencii
            - Floydov algoritmus na detekciu cyklov:
              - Používame dve ukazovatele, "pomalý" a "rýchly"
              - Pomalý ukazovateľ sa posúva o jeden krok v sekvencii (f(a))
              - Rýchly ukazovateľ sa posúva o dva kroky v sekvencii (f(f(a)))
              - hľadáme d = GCD(|pomalý - rýchly|, N) a 
              - vyhodnotenie pre d:
                    * končíme, keď 1 < d < N
                    * pokiaľ d = 1, pokračujeme
                    * ak d = N, zmeníme c (c+=2) a reštartujeme algoritmus
            */
            BigInteger x, y, c, d;
            while (true)
            {
                x = 2; // prvý člen postupnosti rýchlosti 1
                y = 2; // prvý člen postupnosti rýchlosti 2
                c = 1; // konštanta v generujúcej funkcii
                d = 1; // aktuálny deliteľ
                while (d == 1) // pokiaľ sme nenašli netriviálny deliteľ (ak d = 1, pokračujeme v aktuálnej konfigurácií)
                {
                    x = NextFuncElement(x, c, n);
                    y = NextFuncElement(NextFuncElement(y, c, n), c, n);
                    d = BigInteger.GreatestCommonDivisor(BigInteger.Abs(x - y), n);
                    if (d == n) // zlyhalo, zmena c alebo reštart s inou hodnotou prvého členu postupnosti
                        break;
                }
                if (1 < d && d < n)
                {
                    BigInteger other = n / d;
                    if (d < other)
                        return Task.FromResult<(BigInteger, BigInteger)?>( (d, other) );
                    else
                        return Task.FromResult<(BigInteger, BigInteger)?>( (other, d) );
                }
            }

            static BigInteger NextFuncElement(BigInteger a, BigInteger c, BigInteger n) => (a * a + c) % n;
        }
    }

    public class FactorDbServiceFactorization : IFactorizationStrategy
    {
        public string AlgorithmName => "FactorDB service";
        async public Task<(BigInteger p, BigInteger q)?> FactorizeAsync(BigInteger n)
        {
            FactorDbClient client = new FactorDbClient();
            var response = await client.GetFactorizationAsync(n);
            if (response == null)
                return null;
            var factors = FactorDbHelper.ExtractPrimeFactors(response);
            if (factors.Count != 2)
                return null;
            if (factors[0] < factors[1])
                return (factors[0], factors[1]);
            else
                return (factors[1], factors[0]);
        }
    }
}
