using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Coree.Analyzers.CodeClarity
{
    /// <summary>
    /// Decides whether two consecutive methods are missing a blank line between them.
    /// </summary>
    internal static class MethodBlankLine
    {
        internal static bool MissingBefore(MethodDeclarationSyntax previous, MethodDeclarationSyntax next)
        {
            if (previous.Body == null && previous.ExpressionBody == null)
            {
                return false;
            }

            if (next.Body == null && next.ExpressionBody == null)
            {
                return false;
            }

            string gap = next.SyntaxTree.GetText().ToString(TextSpan.FromBounds(previous.Span.End, next.Span.Start));
            return !HasBlankLine(gap);
        }

        internal static bool HasBlankLine(string gap)
        {
            // Use compiler line boundaries for CR-only and Unicode endings, keeping CRLF as one break.
            TextLineCollection lines = SourceText.From(gap).Lines;
            for (int index = 1; index < lines.Count - 1; index++)
            {
                if (string.IsNullOrWhiteSpace(lines[index].ToString()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
