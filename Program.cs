using CommandLine;
using static IC10_Inliner.IC10Assembler;

string sectionName = "min";

var result = Parser.Default.ParseArguments<AssemblyOptions>(args);

if (result.Errors.Count() == 0 && args.Length > 0)
{
    bool wait = false;
    var Options = result.Value;

    if (Options.IncludeSections.Any())
    {
        sectionName = Options.IncludeSections.First();
    }

    var ParseResult = Parse(File.ReadAllText(Options.Filename));
    wait |= ParseResult.Warnings.Count > 0;

    if (ParseResult.Valid)
    {
        var AssemblyResult = Assemble(ParseResult, Options);

        wait |= AssemblyResult.Warnings.Count > 0;

        if (AssemblyResult.Valid)
        {
            foreach (var warning in AssemblyResult.Warnings)
                Console.WriteLine($"Warning: {warning}");

            string ShortName = Path.GetFileName(Options.Filename);
            Console.WriteLine($"Assembled {ShortName} => {ShortName[..^4]}.{sectionName}{ShortName[^4..]}");
            Console.WriteLine($"{AssemblyResult.FinalSections.Count} sections totalling {AssemblyResult.OutputLines.Count} line{(AssemblyResult.OutputLines.Count != 1 ? "s" : "")}");
            File.WriteAllText(Options.Filename[..^4] + "." + sectionName + Options.Filename[^4..], AssemblyResult.Output);

            File.Delete(Options.Filename[..^4] + "." + sectionName + ".sym");
            using var SymbolFile = File.OpenWrite(Options.Filename[..^4] + "." + sectionName + ".sym");
            using StreamWriter writer = new(SymbolFile);
            string last_section = "";
            foreach (var Symbol in AssemblyResult.Symbols)
            {
                if (!AssemblyResult.FinalSections.Contains(Symbol.Section))
                    continue;

                if (last_section != Symbol.Section.Name)
                    writer.WriteLine($"section {Symbol.Section.Name} offset {Symbol.Section.Offset}");

                last_section = Symbol.Section.Name;
                writer.WriteLine($"  {Symbol.SymbolName} offset {Symbol.Value}");
            }

        }
        else
        {
            wait = true;
            Console.WriteLine($"Failed to assemble {Options.Filename}");
            foreach (var warning in ParseResult.Warnings)
                Console.WriteLine($"Warning: {warning}");
            foreach (var warning in AssemblyResult.Warnings)
                Console.WriteLine($"Warning: {warning}");
            foreach (var error in AssemblyResult.Errors)
                Console.WriteLine($"Error: {error}");
        }
    }
    else
    {
        wait = true;
        Console.WriteLine($"Failed to parse file {Options.Filename}");
        foreach (var error in ParseResult.Errors)
            Console.WriteLine($"Error: {error}");
    }
    foreach (var warning in ParseResult.Warnings)
        Console.WriteLine($"Warning: {warning}");

    if (wait)
    {
        Console.Write("Press Enter to continue");
        Console.ReadLine();
    }
}