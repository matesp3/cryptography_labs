using System.Text;

namespace dictionary_attack.utils
{
    public class FileLoader
    {

        /// <summary>
        /// Načíta mená (bez diakritiky) a vráti unikátny zoznam v lowercase.
        /// Očakáva súbor s jedným menom na riadok.
        /// </summary>
        public static List<string> LoadNames(string path = "files/names_full.txt")
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

        public static List<string> LoadCommonTrigrams(string path = "files/trigrams.txt")
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Súbor s trigramami neexistuje: {path}");
            var trigrams = new HashSet<string>(StringComparer.Ordinal);
            foreach (var line in File.ReadLines(path, Encoding.UTF8))
            {
                var p = (line ?? string.Empty).Trim();
                if (p.Length == 0) continue;
                trigrams.Add(p);
            }
            return trigrams.ToList();
        }

        public static void SaveCrackedHashes(string path, IEnumerable<CrackedHash> crackedHashes, double seconds)
        {
            using var writer = new StreamWriter(path, false, Encoding.UTF8);
            writer.WriteLine($"Dictionary attack ended in {seconds} seconds = {seconds / 60} minutes = {seconds / 3600} hours");
            if (crackedHashes == null || !crackedHashes.Any())
            {
                writer.WriteLine("No hashes were cracked.");
                return;
            }

            (int longestLoginLen, int longestPasswordLen, int longestSrc) res =
            (
                crackedHashes.Select(ch => ch.Login?.Length ?? 0).DefaultIfEmpty(0).Max(),
                crackedHashes.Select(ch => ch.Password?.Length ?? 0).DefaultIfEmpty(0).Max(),
                crackedHashes.Select(ch => ch.Source?.Length ?? 0).DefaultIfEmpty(0).Max()
            );

            foreach (var ch in crackedHashes)
            {
                var loginPadding = new string(' ', res.longestLoginLen - (ch.Login?.Length ?? 0) + 1);
                var passwordPadding = new string(' ', res.longestPasswordLen - (ch.Password?.Length ?? 0) + 1);
                var srcPadding = new string(' ', res.longestSrc - (ch.Source?.Length ?? 0) + 1);
                writer.WriteLine($"{ch.Login+loginPadding} - {ch.Password+passwordPadding} " +
                    $" - {ch.Source+srcPadding} - {CrackTypeExtensions.GetDescription(ch.UsedCrackType)}");
            }
        }

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

        public class ShadowEntry
        {
            public required string Login { get; set; }
            public required string Salt { get; set; }
            public required string HashBase64 { get; set; }
        }
    }
}
