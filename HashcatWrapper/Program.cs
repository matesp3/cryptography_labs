using HashcatWrapper;

internal class Program
{
    public static void Main()
    {
        Console.Write("\n~~~ Welcome to Hashcat Wrapper ~~~\n");
        Console.Write("> ");
        while (true)
        {
            string? entry = Console.ReadLine();

            if (entry == null)
                continue;

            string[] p = entry.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            ProcessCommand(p);

            Console.Write("> ");
        }
    }

    private static void ProcessCommand(string[] args)
    {
        if (args.Length == 0)
            return;
        switch (args[0].ToLower())
        {
            case "-hc_file":
            {
                if (args.Length < 3)
                {
                    Console.WriteLine("Error: Missing arguments for -hc_file command. Usage: -hc_file pathToShadowFile -hc_name outputFileName");
                    return;
                }
                string shadowFilePath = args[1];
                string outputFileName = args[3];
                try
                {
                    var shadowEntries = FileHandler.ReadShadowFile(shadowFilePath);
                    bool success = FileHandler.CreateHashCatFormatFile(outputFileName, shadowEntries);
                    if (success)
                        Console.WriteLine($"Successfully converted shadow file to Hashcat format and saved to '{outputFileName}'.");
                    else
                        Console.WriteLine("Error: Failed to create Hashcat format file.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                break;
            }
            case "-uc_file":
            {
                if (args.Length < 3)
                {
                    Console.WriteLine("Error: Missing arguments for -uc_file command. Usage: -uc_file pathToNamesFile -ouf outputFileName");
                    return;
                }
                string namesFilePath = args[1];
                string outputFileName = args[3];
                try
                {
                    var names = FileHandler.LoadNames(namesFilePath);
                    bool success = FileHandler.CreateNamesUpperCharCombinationsFile(outputFileName, names);
                    if (success)
                        Console.WriteLine($"Successfully created upper char combinations file and saved to '{outputFileName}'.");
                    else
                        Console.WriteLine("Error: Failed to create upper char combinations file.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                break;
            }
            case "-help":
                PrintHelp();
                break;
            case "exit":
                Environment.Exit(0);
                break;
            default:
                Console.WriteLine($"Unknown command: {args[0]}. Type -help for available commands.");
                break;
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("(Help) Available commands:");
        Console.WriteLine(" -hc_file pathToShadowFile -hc_name (converts specified shadowFile to desired Hashcat format in file hc_name)");
        Console.WriteLine(" -uc_file pathToNamesFile -ouf pathToOutputFile (converts specified entries-names to file where each name is in such number of rows with which it can have one upper char within it)");
        Console.WriteLine(" -help (shows this hint)");
        Console.WriteLine(" -exit (end program)");
    }

}