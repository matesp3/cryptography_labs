
using System.Numerics;

namespace RSAcracker
{
    internal class FileHandler
    {
        public static RsaEntry[] LoadEntries(string filePath)
        {
            string delim = ":";
            var entries = new List<RsaEntry>();
            var lines = File.ReadAllLines(filePath);
            if (lines.Length == 0)
                return entries.ToArray();
            if (lines[0].ToLower().StartsWith("format"))
            {
                // preskočím hlavičku
                lines = lines[1..];
            }
            foreach (var line in lines)
            {
                var parts = line.Split(delim);
                if (parts.Length == 3 &&
                    BigInteger.TryParse(parts[0], out BigInteger n) &&
                    BigInteger.TryParse(parts[1], out BigInteger e) &&
                    BigInteger.TryParse(parts[2], out BigInteger y))
                {
                    entries.Add(new RsaEntry(n, e, y));
                }
            }
            return entries.ToArray();
        }

        public static void SaveResults(string filePath, RsaResult[] results)
        {
            string delim = ":";
            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Format is 'n:e:y:x:p:q' (modulus:public_key:encrypted:decrypted:prime1_factor:prime2_factor)");
                foreach (var result in results)
                {
                    writer.WriteLine($"{result.Entry.N}{delim}{result.Entry.E}{delim}{result.Entry.Y}{delim}{result.X}{delim}{result.P}{delim}{result.Q}");
                }
            }
        }
    }

    public class RsaEntry
    {
        /// <summary>
        /// Modulus (result of p*q)
        /// </summary>
        public BigInteger N { get; }
        /// <summary>
        /// Public exponent (public key)
        /// </summary>
        public BigInteger E { get; }

        /// <summary>
        /// Encrypted message
        /// </summary>
        public BigInteger Y { get; }

        public RsaEntry(BigInteger n, BigInteger e, BigInteger y)
        {
            this.N = n;
            this.E = e;
            this.Y = y;
        }
    }

    public class RsaResult
    {
        /// <summary>Original RSA entry</summary>
        public RsaEntry Entry { get; }

        /// <summary>
        /// Decrypted message
        /// </summary>
        public BigInteger X { get; }

        /// <summary>First (prime) non-trivial factor of N</summary>
        public BigInteger P { get; }

        /// <summary>Second (prime) non-trivial factor of N</summary>
        public BigInteger Q { get; }
        public RsaResult(RsaEntry entry, BigInteger decryptedMessage, BigInteger p, BigInteger q)
        {
            this.Entry = entry;
            this.X = decryptedMessage;
            this.P = p;
            this.Q = q;
        }
    }
}
