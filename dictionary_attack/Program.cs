using System.Text;

namespace dictionary_attack
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            Console.WriteLine("Dictionary attack started!");
            string baseDir = AppContext.BaseDirectory;
            string filesDirPath = Path.Combine(baseDir, "files");
            var outputPath = Path.Combine(filesDirPath, "cracked_hashes.txt");

            DictionaryAttack.CrackHashes(out var sw, outputPath, filesDirPath);
            var seconds = sw.Elapsed.TotalSeconds;
            Console.WriteLine("Dictionary attack ended!");
            Console.WriteLine($"Overall time of dictionary attack in {seconds} seconds = {seconds / 60} minutes = {seconds / 3600} hours");
            Console.WriteLine($"Cracked hashes saved to: {outputPath}");
        }
    }
}

