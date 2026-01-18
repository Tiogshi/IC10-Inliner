namespace IC10_Inliner;


public readonly struct ParseResult
{
    public readonly List<string> Warnings = [];
    public readonly List<string> Errors = [];
    public readonly List<string> SectionNames = [];

    public readonly IC10Program Program;

    public readonly bool Valid => Errors.Count == 0;

    internal ParseResult(IC10Program Program)
    {
        this.Program = Program;
    }
}