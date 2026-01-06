using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dictionary_attack.data_loading
{
    internal class PasswordsEnumerator
    {
        /// <summary>
        /// Vráti enumerátor mien, kde každé meno má jedno písmeno veľké (postupne pre každé meno použije všetky možnosti).
        /// Pre každé meno vráti varianty, kde práve jeden znak je veľký (ostatné sú zmenené na malé).
        /// </summary>
        /*
         Pseudocode / plan:
         - Validate input 'names' is not null.
         - For each 'name' in 'names':
             - If name is null or empty: yield return name.
             - Else:
                 - Create 'lower' = name.ToLowerInvariant() to have consistent base.
                 - For index i from 0 to lower.Length - 1:
                     - upperChar = Char.ToUpperInvariant(lower[i])
                     - Build variation:
                         - prefix = lower.Substring(0, i)
                         - suffix = lower.Substring(i + 1) if i+1 < a else empty
                         - variation = prefix + upperChar + suffix
                     - yield return variation
         */
        public static IEnumerable<string> GetNamesWithOneUpper(IEnumerable<string> names)
        {
            if (names == null)
                throw new ArgumentNullException(nameof(names));

            foreach (var name in names)
            {
                if (string.IsNullOrEmpty(name))
                {
                    yield return name;
                    continue;
                }

                var lower = name.ToLowerInvariant();
                for (var i = 0; i < lower.Length; i++)
                {
                    var upperChar = char.ToUpperInvariant(lower[i]);

                    if (i == 0)
                    {
                        yield return upperChar + (lower.Length > 1 ? lower[1..] : string.Empty);
                    }
                    else if (i == lower.Length - 1)
                    {
                        yield return lower[..i] + upperChar;
                    }
                    else
                    {
                        yield return lower[..i] + upperChar + lower[(i + 1)..];
                    }
                }
            }
        }

        /// <summary>
        /// Generuje všetky možné heslá dĺžky 6 a 7 znakov z kombinácií dvoch trigramov a prípadne jedného písmena. Okrem toho vytvára aj náhodne kombinácie znakov.
        /// </summary>
        /// <param name="trigrams"></param>
        /// <returns>Heslá dĺžky 6 alebo 7</returns>
        public static IEnumerable<string> Get6To7LongPasswordsFromLettersOnly(List<string> trigrams)
        {

            yield return "abcdefg";
            yield return "qwerty";
            yield return "asdfgh";
            yield return "asdfghj";
            yield return "zxcvbn";
            yield return "zxcvbnm";
            yield return "fghjkl";
            yield return "sdfghj";
            yield return "tyuiop";

            var maxCombinations = 1_250_000;

            IEnumerable<string> en;
            en = GenerateRandomCombinations(new Random(15), 6, maxCombinations);
            foreach (var password in en)
            {
                yield return password;
            }
            en = GenerateRandomCombinations(new Random(15+1000), 7, maxCombinations);
            foreach (var password in en)
            {
                yield return password;
            }
            en = GenerateCombinationsFromTrigrams(trigrams);
            foreach (var password in en)
            {
                yield return password;
            }
        }

        private static IEnumerable<string> GenerateRandomCombinations(Random rndIdx, int strLen, int maxCombinations)
        {
            int l = 0;
            char[] password = new char[strLen];
            while (l < maxCombinations)
            {
                l++;
                for (var i = 0; i < strLen; i++)
                {
                    password[i] = (char)('a' + rndIdx.Next(0, 26));
                }
                yield return new string(password);
            }
        }

        private static IEnumerable<string> GenerateCombinationsFromTrigrams(List<string> trigrams)
        {
            for (var i = 0; i < trigrams.Count; i++)
            {
                for (var j = 0; j < trigrams.Count; j++)
                {
                    for (var k = 'a'; k <= 'z'; k++)
                    {
                        for (var a = 0; a <= 3; a++)
                        {
                            switch (a)
                            {
                                case 0:
                                    yield return trigrams[i] + trigrams[j]; // 6-znakove
                                    break;
                                case 1:
                                    yield return trigrams[i] + trigrams[j] + Char.ToString(k); // 7-znakove A+B+k
                                    break;
                                case 2:
                                    yield return trigrams[i] + Char.ToString(k) + trigrams[j]; // 7-znakove A+k+B
                                    break;
                                case 3:
                                    yield return Char.ToString(k) + trigrams[i] + trigrams[j]; // 7-znakove k+A+B
                                    break;
                            }
                        }
                    }
                }
            }
        }

        public static IEnumerable<string> Get4To5LongFromDigitsAndLetters()
        {
            /* p = lowercase letter
             * P = uppercase letter
             * c = digit
             * -------------------- password patterns:
             * cccc 10*10*10*10 = 10000
             * ppcc 26*26*10*10 = 67600
             * pppc 26*26*26*10 = 175760
             * pcpc 26*10*26*10 = 67600
             * PcPc 26*10*26*10 = 67600
             * cpcp 10*26*10*26 = 67600
             * cPcP 10*26*10*26 = 67600
             * cppp 10*26*26*26 = 175760
             * Pppc 26*26*26*10 = 175760
             * cccpp 10*10*10*26*26 = 676000
             * ppccc 26*26*10*10*10 = 676000
             * Ppccc 26*26*10*10*10 = 676000
             * cPcpc 10*26*10*26*10 = 676000
             * Pcpcc 26*10*26*10*10 = 676000
             * ccccc 10*10*10*10*10 = 100000
             * sum dokopy pokusov : 3,521,720
             */
            var digits = Digits();
            var lower = LowercaseLetters();
            var upper = UppercaseLetters();

            // cccc 10*10*10*10 = 10000
            var enumerator = GenerateCombinationsOf4(digits, digits, digits, digits);
            foreach (var passwd in enumerator) yield return passwd;

            // ppcc 26*26*10*10 = 67600
            enumerator = GenerateCombinationsOf4(lower, lower, digits, digits);
            foreach (var passwd in enumerator) yield return passwd;

            // pcpc 26*10*26*10 = 67600
            enumerator = GenerateCombinationsOf4(lower, digits, lower, digits);
            foreach (var passwd in enumerator) yield return passwd;

            // PcPc 26*10*26*10 = 67600
            enumerator = GenerateCombinationsOf4(upper, digits, upper, digits);
            foreach (var passwd in enumerator) yield return passwd;

            // cpcp 10*26*10*26 = 67600
            enumerator = GenerateCombinationsOf4(digits, lower, digits, lower);
            foreach (var passwd in enumerator) yield return passwd;

            // cPcP 10*26*10*26 = 67600
            enumerator = GenerateCombinationsOf4(digits, upper, digits, upper);
            foreach (var passwd in enumerator) yield return passwd;

            // pppc 26*26*26*10 = 175760
            enumerator = GenerateCombinationsOf4(lower, lower, lower, digits);
            foreach (var passwd in enumerator) yield return passwd;

            // cppp 10*26*26*26 = 175760
            enumerator = GenerateCombinationsOf4(digits, lower, lower, lower);
            foreach (var passwd in enumerator) yield return passwd;

            // Pppc 26*26*26*10 = 175760
            enumerator = GenerateCombinationsOf4(upper, lower, lower, digits);
            foreach (var passwd in enumerator) yield return passwd;

            // cccpp 10*10*10*26*26 = 676000
            enumerator = GenerateCombinationsOf5(digits, digits, digits, lower, lower);
            foreach (var passwd in enumerator) yield return passwd;

            // ppccc 26*26*10*10*10 = 676000
            enumerator = GenerateCombinationsOf5(lower, lower, digits, digits, digits);
            foreach (var passwd in enumerator) yield return passwd;

            // Ppccc 26*26*10*10*10 = 676000
            enumerator = GenerateCombinationsOf5(upper, lower, digits, digits, digits);
            foreach (var passwd in enumerator) yield return passwd;

            // cPcpc 10*26*10*26*10 = 676000
            enumerator = GenerateCombinationsOf5(digits, upper, digits, lower, digits);
            foreach (var passwd in enumerator) yield return passwd;

            // Pcpcc 26*10*26*10*10 = 676000
            enumerator = GenerateCombinationsOf5(upper, digits, lower, digits, digits);
            foreach (var passwd in enumerator) yield return passwd;

            // ccccc 10*10*10*10*10 = 100000
            enumerator = GenerateCombinationsOf5(digits, digits, digits, digits, digits);
            foreach (var passwd in enumerator) yield return passwd;

        }

        private static IEnumerable<string> GenerateCombinationsOf4(char[] c1, char[] c2, char[] c3, char[] c4)
        {
            char[] arr = new char[4];
            for (var i1 = 0; i1 < c1.Length; i1++)
            {
                arr[0] = c1[i1];
                for (var i2 = 0; i2 < c2.Length; i2++)
                {
                    arr[1] = c2[i2];
                    for (var i3 = 0; i3 < c3.Length; i3++)
                    {
                        arr[2] = c3[i3];
                        for (var i4 = 0; i4 < c4.Length; i4++)
                        {
                            arr[3] = c4[i4];
                            yield return new string(arr);
                        }
                    }
                }
            }
        }

        private static IEnumerable<string> GenerateCombinationsOf5(char[] c1, char[] c2, char[] c3, char[] c4, char[] c5)
        {
            char[] arr = new char[5];
            for (var i1 = 0; i1 < c1.Length; i1++)
            {
                arr[0] = c1[i1];
                for (var i2 = 0; i2 < c2.Length; i2++)
                {
                    arr[1] = c2[i2];
                    for (var i3 = 0; i3 < c3.Length; i3++)
                    {
                        arr[2] = c3[i3];
                        for (var i4 = 0; i4 < c4.Length; i4++)
                        {
                            arr[3] = c4[i4];
                            for (var i5 = 0; i5 < c5.Length; i5++)
                            {
                                arr[4] = c5[i5];
                                yield return new string(arr);
                            }
                        }
                    }
                }
            }
        }

        private static char[] UppercaseLetters()
        {
            var arr = new char[26];
            for (int i = 0; i < 26; i++) arr[i] = (char)('A' + i);
            return arr;
        }

        private static char[] LowercaseLetters()
        {
            var arr = new char[26];
            for (int i = 0; i < 26; i++) arr[i] = (char)('a' + i);
            return arr;
        }

        private static char[] Digits()
        {
            var arr = new char[10];
            for (int i = 0; i < 10; i++) arr[i] = (char)('0' + i);
            return arr;
        }
    }
}