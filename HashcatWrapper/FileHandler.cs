using System.Text;

namespace HashcatWrapper
{
    public class FileHandler
    {
        /// <summary>
        /// Načíta súbor so záznamami užívateľov a ich hesiel v shadow formáte. (Login:Salt:HashBase64)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerable<ShadowEntry> ReadShadowFile(string path)
        {
            foreach (var line in File.ReadLines(path, Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(':');
                if (parts.Length != 3) continue;

                yield return new ShadowEntry
                {
                    Login = parts[0].Trim(),
                    Salt = parts[1].Trim(),
                    HashBase64 = parts[2].Trim()
                };
            }
        }

        /// <summary>
        /// Načíta mená (bez diakritiky) a vráti unikátny zoznam v lowercase.
        /// Očakáva súbor s jedným menom na riadok.
        /// </summary>
        public static List<string> LoadNames(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Súbor s menami neexistuje: {path}");

            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var line in File.ReadLines(path, Encoding.UTF8))
            {
                var n = (line ?? string.Empty).Trim();
                if (n.Length == 0) continue;
                names.Add(n.ToLower());
            }

            return names.ToList();
        }

        public static bool CreateNamesUpperCharCombinationsFile(string path, IEnumerable<string> names)
        {
            try
            {
                using var writer = new StreamWriter(path, false, Encoding.UTF8);
                foreach (var name in names)
                {
                    for (int i = 0; i < name.Length; i++)
                    {
                        var charArray = name.ToCharArray();
                        charArray[i] = char.ToUpper(charArray[i]);
                        writer.WriteLine(new string(charArray));
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Vytvorí súbor v HashCat formáte: 'base64_hash:salt'.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="shadowEntries"></param>
        /// <returns>true, ak sa operácia zápisu podarila, inak false</returns>
        public static bool CreateHashCatFormatFile(string path, IEnumerable<ShadowEntry> shadowEntries)
        {
            try
            {
                using var writer = new StreamWriter(path, false, Encoding.UTF8);
                foreach (var entry in shadowEntries)
                {
                    writer.WriteLine($"{Base64ToHex(entry.HashBase64)}:{entry.Salt}");
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string Base64ToHex(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

    }
}
