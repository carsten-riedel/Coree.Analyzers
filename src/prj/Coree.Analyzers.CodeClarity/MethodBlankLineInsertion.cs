using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Coree.Analyzers.CodeClarity
{
    /// <summary>
    /// Builds the blank line CCCMB001 asks for, immediately after the previous method.
    /// </summary>
    internal static class MethodBlankLineInsertion
    {
        internal static SourceText Apply(SourceText text, SyntaxNode root, TextSpan span)
        {
            SyntaxToken token = root.FindToken(span.Start);
            if (token.Parent is not MethodDeclarationSyntax method)
            {
                return text;
            }

            TextSpan edit;
            string insertion;
            if (!TryCreate(method, out edit, out insertion))
            {
                return text;
            }

            return text.Replace(edit, insertion);
        }

        internal static bool TryCreate(MethodDeclarationSyntax method, out TextSpan span, out string insertion)
        {
            span = default;
            insertion = string.Empty;
            MethodDeclarationSyntax? previous = PreviousMethod(method);
            if (previous == null || !MethodBlankLine.MissingBefore(previous, method))
            {
                return false;
            }

            SourceText text = method.SyntaxTree.GetText();
            string gap = text.ToString(TextSpan.FromBounds(previous.Span.End, method.Span.Start));
            string newline = LineEndingBefore(text, previous.Span.End);
            if (HasLineBreak(gap))
            {
                span = new TextSpan(previous.Span.End, 0);
                insertion = newline;
                return true;
            }

            span = new TextSpan(previous.Span.End, 0);
            insertion = newline + newline;
            if (IsWhitespace(gap))
            {
                span = TextSpan.FromBounds(previous.Span.End, method.Span.Start);
            }

            return true;
        }

        private static MethodDeclarationSyntax? PreviousMethod(MethodDeclarationSyntax method)
        {
            if (method.Parent is not TypeDeclarationSyntax type)
            {
                return null;
            }

            MemberDeclarationSyntax? prior = null;
            foreach (MemberDeclarationSyntax member in type.Members)
            {
                if (member == method)
                {
                    break;
                }

                prior = member;
            }

            return prior as MethodDeclarationSyntax;
        }

        private static string LineEndingBefore(SourceText text, int position)
        {
            for (int index = position - 1; index >= 0; index--)
            {
                char character = text[index];
                if (character == '\n')
                {
                    if (index > 0 && text[index - 1] == '\r')
                    {
                        return "\r\n";
                    }

                    return "\n";
                }

                if (character == '\r' || character == '\u0085' || character == '\u2028' || character == '\u2029')
                {
                    return character.ToString();
                }
            }

            return "\n";
        }

        private static bool HasLineBreak(string gap)
        {
            foreach (char character in gap)
            {
                if (character == '\n' || character == '\r' || character == '\u0085' || character == '\u2028' || character == '\u2029')
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsWhitespace(string gap)
        {
            foreach (char character in gap)
            {
                if (!char.IsWhiteSpace(character))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
