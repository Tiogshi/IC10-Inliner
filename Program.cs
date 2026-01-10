using CommandLine;
using static IC10_Inliner.IC10Assembler;

var sectionName = "min";

var result = Parser.Default.ParseArguments<AssemblyOptions>(args);

if (!result.Errors.Any() && args.Length > 0)
{
    var Failed = false;
    var Options = result.Value;

    if (Options.IncludeSections?.Any() ?? false)
        sectionName = Options.IncludeSections.First();


    var ParseResult = Parse(File.ReadAllText(Options.Filename));

    if (ParseResult.Valid)
    {
        var AssemblyResult = Assemble(ParseResult, Options);


        if (AssemblyResult.Valid)
        {
            foreach (var warning in AssemblyResult.Warnings)
                Console.Error.WriteLine($"Warning: {warning}");

            var Extension = Path.GetExtension(Options.Filename);
            var ShortName = Path.GetFileNameWithoutExtension(Options.Filename);
            var LongFilename = Options.Filename[..^Extension.Length];

            if (!Options.OutputToSTDOUT)
            {
                Console.Out.WriteLine($"Assembled {ShortName} => {ShortName}.{sectionName}{Extension}");
                Console.Out.WriteLine(
                    $"{AssemblyResult.FinalSections.Count} sections totalling {AssemblyResult.OutputLines.Count} line{(AssemblyResult.OutputLines.Count != 1 ? "s" : "")} of code");
                File.WriteAllText($"{LongFilename}.{sectionName}{Extension}", AssemblyResult.Output);


                if (Options.EmitSymbolFile)
                {
                    File.Delete($"{LongFilename}.{sectionName}.sym");
                    using var SymbolFile = File.OpenWrite($"{LongFilename}.{sectionName}.sym");
                    using StreamWriter writer = new(SymbolFile);
                    var last_section = "";
                    foreach (var Symbol in AssemblyResult.Symbols.Where(Symbol => AssemblyResult.FinalSections.Contains(Symbol.Section)))
                    {
                        if (last_section != Symbol.Section.Name)
                            writer.WriteLine($"section {Symbol.Section.Name} offset {Symbol.Section.Offset}");

                        last_section = Symbol.Section.Name;
                        writer.WriteLine($"  {Symbol.SymbolName} offset {Symbol.Value}");
                    }
                }
            }
            else
            {
                Console.Out.Write(AssemblyResult.Output);
            }
        }
        else
        {
            Failed = true;
            Console.Error.WriteLine($"Failed to assemble {Options.Filename}");
            foreach (var warning in ParseResult.Warnings)
                Console.Error.WriteLine($"Warning: {warning}");
            foreach (var warning in AssemblyResult.Warnings)
                Console.Error.WriteLine($"Warning: {warning}");
            foreach (var error in AssemblyResult.Errors)
                Console.Error.WriteLine($"Error: {error}");
        }
    }
    else
    {
        Failed = true;
        Console.Error.WriteLine($"Failed to parse file {Options.Filename}");
        foreach (var error in ParseResult.Errors)
            Console.Error.WriteLine($"Error: {error}");
    }

    foreach (var warning in ParseResult.Warnings)
        Console.Error.WriteLine($"Warning: {warning}");

    return Failed ? 1 : 0;
}

return -1;