using CommandLine;
using JetBrains.Annotations;

namespace IC10_Inliner;


// ReSharper disable once ClassNeverInstantiated.Global
// (This is instantiated as a generic in the CommandLine library)
public static partial class IC10Assembler
{
    [UsedImplicitly]
    public class AssemblerOptions
    {
        [Value(0, Required = true)]
        public string Filename { get; set; } = "";

        [Option('c', "comments", Default = false)]
        public bool IncludeComments { get; set; } = false;

        [Option('s', "sections")]
        public IEnumerable<string>? IncludeSections { get; set; } = [];

        [Option('m', "keep-macros", Default = false)]
        public bool ElideMacros { get; set; } = false;

        [Option('S', "symbols", Default = false)]
        public bool EmitSymbolFile { get; set; } = false;
        
        [Option('o', "stdout", Default = false)]
        public bool OutputToSTDOUT { get; set; } = false;
        
        [Option('z', "size", Default = false)]
        public bool ReportSizeInfo { get; set; } = false;
    }
}