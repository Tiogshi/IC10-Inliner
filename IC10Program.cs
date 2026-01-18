namespace IC10_Inliner;

public class IC10Program
{
    public List<ProgramSection> Sections { get; } = [];

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