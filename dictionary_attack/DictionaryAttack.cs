using dictionary_attack.data_loading;
using dictionary_attack.utils;
using System.Diagnostics;

namespace dictionary_attack
{
    internal class DictionaryAttack
    {/// <summary>
     /// Pokus prelomiť heslá načítané zo súboru entry pomocou rôznych stratégií generovania hesiel na výpočet odtlačku.
     /// </summary>
     /// <param name="endedStopWatch">ukončný časovač pre získanie informácie o dĺžke behu pokusov o prelomenie hesiel</param>
     /// <param name="outputPath">cesta k výstupnému súboru s prelomenými heslami</param>
     /// <returns>zoznam prelomených hesiel</returns>
        public static List<CrackedHash> CrackHashes(out Stopwatch endedStopWatch, in string outputPath, in string fileDirPath)
        {
            var cracked = new List<CrackedHash>();
            endedStopWatch = Stopwatch.StartNew();

            var md = new SaltedMD5Hasher();
            var names = FileLoader.LoadNames(Path.Combine(fileDirPath, "names_full.txt"));
            var trigrams = FileLoader.LoadCommonTrigrams(Path.Combine(fileDirPath, "trigrams.txt"));


            string shadowFile, shadowId;
            for (int i = 1; i < 5; i++)
            {
                shadowId = $"shadow{i}";
                shadowFile = Path.Combine(fileDirPath, @$"{shadowId}.txt");

                var shadowEntries = FileLoader.ReadShadowFile(shadowFile).ToList();
                Console.WriteLine($" > Trying to crack entries from file '{shadowFile}'");
                TryToCrackHashes(cracked, shadowEntries,
                    PasswordsEnumerator.GetNamesWithOneUpper(names), CrackType.NameWithOneUpper, md, shadowId);

                TryToCrackHashes(cracked, shadowEntries,
                    PasswordsEnumerator.Get6To7LongPasswordsFromLettersOnly(trigrams), CrackType.Letters6To7, md, shadowId);

                TryToCrackHashes(cracked, shadowEntries,
                    PasswordsEnumerator.Get4To5LongFromDigitsAndLetters(), CrackType.DigitsAndLetters4To5, md, shadowId);
            }

            endedStopWatch.Stop();
            if (!string.IsNullOrWhiteSpace(outputPath) && cracked.Count > 0)
            {
                FileLoader.SaveCrackedHashes(outputPath, cracked, endedStopWatch.Elapsed.TotalSeconds);
            }
            return cracked;
        }

        /// <summary>
        /// Pokus prelomiť heslá získané z enumerátora hesiel. Ak sa prelomenie podarí, zapíše heslo do prelomených odtlačkov.
        /// </summary>
        /// <param name="cracked">Zoznam, do ktorého sa majú pridávať prelomené heslá</param>
        /// <param name="shadowEntries">Zoznam užívateľov, voči ktorým bude skúšaná hešovacia funkcia na prelomenie</param>
        /// <param name="passwordsEnumerator">Enumerátor sprístupňujúci potenciálne heslá</param>
        /// <param name="crackType">O aký typ skúšaných hesiel ide</param>
        /// <param name="md">Hešovacia funkcia</param>
        /// <param name="shadowId">Identifikátor súboru so záznamami užívateľov</param>
        private static void TryToCrackHashes(in List<CrackedHash> cracked, in List<FileLoader.ShadowEntry> shadowEntries,
            IEnumerable<string> passwordsEnumerator, in CrackType crackType, in SaltedMD5Hasher md, string shadowId)
        {
            Console.WriteLine($"  >> using technique: {CrackTypeExtensions.GetDescription(crackType)}...");
            foreach (var password in passwordsEnumerator)
            {
                foreach (var entry in shadowEntries)
                {
                    var computedHash = md.crypt(password, entry.Salt);
                    if (computedHash == entry.HashBase64)
                    {
                        var c = new CrackedHash
                        {
                            Login = entry.Login,
                            Password = password,
                            Source = shadowId,
                            UsedCrackType = crackType
                        };
                        cracked.Add(c);
                        Console.WriteLine($"[CRACKED] User: {entry.Login}, Password: {password}, Source: {shadowId}, Type: {CrackTypeExtensions.GetDescription(crackType)}");
                    }
                }
            }
            Console.WriteLine($"   >>> used {passwordsEnumerator.Count()} passwords for each entry!");
        }
    }
}
