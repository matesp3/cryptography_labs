
using RSAcracker;
using System;
using System.Numerics;
using static System.Net.Mime.MediaTypeNames;

internal class Program
{
    static async Task Main()
    {
        PrintWelcome();
        while (true)
        {
            Console.Write(" > ");
            string input = Console.ReadLine() ?? "";
            string[] commands = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            await ProcessRequestAsync(commands);
        }
    }

    async static Task ProcessRequestAsync(string[] args)
    {
        if (args == null || args.Length == 0)
        {
            PrintHelp();
            return;
        }
        switch (args[0].ToLower())
        {
            case "help":
                {
                    PrintHelp();
                    break;
                }
            case "decrypt":
                {
                    await SolveSingleRsaTaskAsync(args);
                    break;
                }
            case "decrypt_file":
                {
                    await SolveAllFileEntriesAsync(args);
                    break;
                }
            case "exit":
                {
                    Environment.Exit(0);
                    break;
                }
            default:
                {
                    Console.WriteLine("Unknown command. Type 'help' to see available functions.");
                    break;
                }
        }
    }

    private static async Task SolveSingleRsaTaskAsync(string[] args)
    {
        BigInteger n = 0, e = 0, y = 0;
        for (int i = 1; i < args.Length - 1; i++)
        {
            if (i + 1 >= args.Length) break;
            switch (args[i].ToLower())
            {
                case "-n":
                    BigInteger.TryParse(args[i + 1], out n);
                    break;
                case "-e":
                    BigInteger.TryParse(args[i + 1], out e);
                    break;
                case "-y":
                    BigInteger.TryParse(args[i + 1], out y);
                    break;
            }
        }
        if (n > 1 && e > 0 && y >= 0)
        {
            await DecryptRsaAsync(n, e, y, true);
        }
        else
        {
            Console.WriteLine("Invalid parameters for decryption. Please provide valid n, e, and y values.");
        }
    }

    private static async Task SolveAllFileEntriesAsync(string[] args)
    {
        string? inputFile = null;
        string? outputFile = null;
        bool? print = null; // výpis na konzolu
        for (int i = 1; i < args.Length - 1; i++)
        {
            if (i + 1 >= args.Length) break;
            switch (args[i].ToLower())
            {
                case "-in":
                    inputFile = args[i + 1];
                    break;
                case "-out":
                    outputFile = args[i + 1];
                    break;
                case "-show":
                    print = (args[i + 1].ToLower() == "y");
                    break;
            }
        }
        if (!string.IsNullOrEmpty(inputFile) && !string.IsNullOrEmpty(outputFile) && print is not null)
        {
            var entries = FileHandler.LoadEntries(inputFile);
            var results = new List<RsaResult>();
            foreach (var entry in entries)
            {
                var decryptionResult = await DecryptRsaAsync(entry.N, entry.E, entry.Y, print??false);
                if (decryptionResult.HasValue)
                {
                    var (x, p, q) = decryptionResult.Value;
                    results.Add(new RsaResult(entry, x, p, q));
                }
            }
            FileHandler.SaveResults(outputFile, results.ToArray());
            Console.WriteLine($"Decryption completed. Results saved to '{outputFile}'.");
        }
        else
        {
            Console.WriteLine("Invalid parameters for decrypt_file. Please provide valid input and output file paths\n and also choice whether to print results on console or not");
        }
    }

    async static Task<(BigInteger x, BigInteger p, BigInteger q)?> DecryptRsaAsync(BigInteger n, BigInteger e, BigInteger y, bool print)
    {
        IFactorizationStrategy f = n.ToString().Length <= 25
            ? new PollardsRhoFactorization()
            : new FactorDbServiceFactorization();
        var factors = await f.FactorizeAsync(n);
        if (factors is null)
        {
            if (print) PrintFailedFactorization(n);
            return null;
        }
        else // faktorácia úspešná
        {
            var (p, q) = factors.Value;
            BigInteger d = RsaDecryption.DecryptPrivateKey(p, q, e, y);
            BigInteger x = RsaDecryption.DecryptMessage(d, n, y);
            if (!RsaDecryption.VerifyDecryption(x, e, n, y)) // zhoduje sa zašifrovaná správa so zašifrovaním dešifrovanej správy?
            {
                if (print) PrintFailedVerification(n, e, y, p, q, d, x);
                return null;
            }
            if (print) PrintSolutionOfDecryption(n, y, p, q, d, x, f.AlgorithmName);
            return (x, p, q);
        }
    }

    private static void PrintFailedVerification(BigInteger n, BigInteger e, BigInteger y, BigInteger p, BigInteger q, BigInteger d, BigInteger x)
    {
        Console.WriteLine("-------------------------------------------------------");
        Console.WriteLine("Decryption verification failed for parameters:");
        Console.WriteLine($"n={n} p={p} q={q} e={e} d={d} y={y} x={x}");
        Console.WriteLine("-------------------------------------------------------");
    }

    private static void PrintSolutionOfDecryption(BigInteger n, BigInteger y, BigInteger p, BigInteger q, BigInteger d, BigInteger x, string algoName)
    {
        Console.WriteLine("-------------------------------------------------------");
        Console.WriteLine($" Used factorization algorithm: {algoName}");
        Console.WriteLine($" A non-trivial factors of {n} are: p={p} & q={q}");
        Console.WriteLine($" Found private key d={d}");
        Console.WriteLine($" Decryption of message y={y} is: x={x}");
        Console.WriteLine("-------------------------------------------------------");
    }

    private static void PrintFailedFactorization(BigInteger n)
    {
        Console.WriteLine("-------------------------------------------------------");
        Console.WriteLine($"Failed to find a non-trivial factor for n={n} (Message cannot be decrypted)");
        Console.WriteLine("-------------------------------------------------------");
    }

    static void PrintWelcome()
    {
        Console.WriteLine("\n * * *  W E L C O M E  T O  RSA C r a c k e r * * *");
        Console.WriteLine("  --> This program allows you to decrypt RSA encrypted messages by factoring the modulus.");
        Console.WriteLine("  --> Type 'help' to see available functions.\n");
    }

    static void PrintHelp()
    {
        Console.WriteLine(" ~~~  H E L P ~~~");
        Console.WriteLine(" Available functions:\n");
        Console.WriteLine("  > decrypt -n modulus(p*q) -e public_exponent -y encrypted_message");
        Console.WriteLine("   [Decrypts the encrypted_message using RSA decryption and prints primes p & q]\n");
        Console.WriteLine("  > decrypt_file -in input_file -out output_file -show <y/n>");
        Console.WriteLine("   [Decrypts multiple RSA encrypted messages from input_file and saves results to output_file");
        Console.WriteLine("   [If -show = 'y', then results are printed into console]");
        Console.WriteLine("   [Input file must match row pattern: n:e:y (modulus:public_exponent:encrypted_message]\n");
        Console.WriteLine("  > exit");
        Console.WriteLine("   [Exits the program]\n");
        Console.WriteLine("  > help");
        Console.WriteLine("   [Shows this help]\n");
    }
}