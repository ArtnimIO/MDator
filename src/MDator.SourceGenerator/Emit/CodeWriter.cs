using System.Text;

namespace MDator.SourceGenerator;

/// <summary>
/// Minimal indented source text builder. The generator could use
/// <c>SyntaxFactory</c> here, but for emit we want tight control over whitespace
/// so the generated files are readable in IDEs when users GoToDefinition into them.
/// </summary>
internal sealed class CodeWriter
{
    private readonly StringBuilder _sb = new();
    private int _indent;

    public CodeWriter Indent() { _indent++; return this; }
    public CodeWriter Dedent() { _indent--; return this; }

    public CodeWriter Line(string text = "")
    {
        if (text.Length == 0)
        {
            _sb.AppendLine();
            return this;
        }
        for (var i = 0; i < _indent; i++) _sb.Append("    ");
        _sb.AppendLine(text);
        return this;
    }

    public CodeWriter OpenBrace()
    {
        Line("{");
        return Indent();
    }

    public CodeWriter CloseBrace()
    {
        return Dedent().Line("}");
    }

    public CodeWriter CloseBraceWithSemicolon() => Dedent().Line("};");

    public override string ToString() => _sb.ToString();
}
