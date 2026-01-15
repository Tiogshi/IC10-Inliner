using JetBrains.Annotations;
using System.Globalization;

namespace IC10_Inliner
{
    public class Macro
    {
        public string Name { get; private set; }
        public int SourceLine { get; private set; }
        public List<string> ParamNames { get; private set; }
        private List<string> BodyLines = new();

        public Macro(string Name, IEnumerable<string> Params, int sourceLine)
        {
            this.Name = Name;
            ParamNames = Params.ToList();
            SourceLine = sourceLine;
        }

        public void Add(string Line)
        {
            BodyLines.Add(Line);
        }

        public IEnumerable<string> Invoke(List<string> Params)
        {
            return BodyLines.Select(ln =>
            {
                for (var i = 0; i < ParamNames.Count; i++)
                {
                    //TODO: whitespace-tokenize the line to prevent clbuttic substitution
                    ln = ln.Replace(ParamNames[i], Params[i]);
                }
                return ln;
            });
        }
    }

    public class IC10Program
    {
        public List<ProgramSection> Sections { get; } = [];
        public Dictionary<string, Macro> Macros { get; } = new();

        public HashSet<string> Symbols { get; } = [];
        public HashSet<string> Aliases { get; } = [];

        public class ProgramSection(string SectionName, List<string>? ReqSections = null)
        {
            public Dictionary<string, Symbol> Symbols { get; } = [];
            public Dictionary<string, string> Aliases { get; } = [];
            public List<ProgramLine> Lines { get; set; } = [];
            public int Offset { get; set; }
            public int Size => Lines.Count;
            public string Name { get; init; } = SectionName;
            public List<string> RequiredSections { get; init; } = ReqSections ?? [];

            public bool IsEmpty => Size == 0 && Aliases.Count == 0 && Symbols.Count == 0;
        }

        public record Symbol
        {
            public SymbolKind SymbolType { get; init; }

            public string EnumValue { get; init; }

            public double? Value { get; init; }

            public string SymbolName { get; init; }

            public ProgramSection Section { get; init; } // Section this symbol is a part of (for labels)

            public bool IsValidConstant => Value is not null || SymbolType == SymbolKind.Label || IC10Assembler.Macro().IsMatch(EnumValue);

            public string Resolve(ProgramSection FromSection)
            {
                return SymbolType switch
                {
                    SymbolKind.Label => ((Value ?? 0.0) + FromSection.Offset).ToString(),
                    _ => Value?.ToString() ?? EnumValue,
                };
            }

            public static bool TryParseBinary(string input, out ulong Parsed)
            {
                if (input.StartsWith("0b", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ulong.TryParse(input[2..], NumberStyles.BinaryNumber, CultureInfo.InvariantCulture, out Parsed);
                }
                Parsed = 0;
                return false;
            }

            public static bool TryParseHex(string input, out ulong Parsed)
            {
                if (input.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ulong.TryParse(input[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out Parsed);
                }
                else if (input.StartsWith('$'))
                {
                    return ulong.TryParse(input[1..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out Parsed);
                }
                Parsed = 0;
                return false;
            }

            public Symbol(ProgramSection CurrentSection, string Name, string TextValue, SymbolKind Type)
            {
                Section = CurrentSection;
                SymbolName = Name;
                SymbolType = Type;

                switch (Type)
                {
                    case SymbolKind.Constant:
                        if (TryParseHex(TextValue, out var ValueInt) || TryParseBinary(TextValue, out ValueInt))
                            Value = ValueInt;
                        else if (double.TryParse(TextValue, out var NewValue))
                            Value = NewValue;
                        EnumValue = TextValue;

                        break;
                    case SymbolKind.Label:
                        Value = CurrentSection.Size;
                        EnumValue = Name;
                        break;
                    default:
                        EnumValue = TextValue;
                        Value = null;
                        break;
                }
            }

            public enum SymbolKind
            {
                Constant, // Constant symbol, i.e. number or LogicType enum, or STR("")/HASH("") construct
                Label, // Specifically for labels
            }
        }

        public readonly struct ProgramLine()
        {
            public string? OpCode { get; init; } = string.Empty;

            public string Directive { get; init; } = string.Empty;

            public List<string> Params { get; init; } = [];

            public string Comment { get; init; } = string.Empty;

            public int OriginalCodeLine { get; init; } = 0;

            public int SectionOffset { get; init; } = 0;

        }
    }
}
