#nullable enable
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Coree.Analyzers.CodeClarity
{
    internal static class ParameterListFit
    {
        internal static bool UsesBreakButFits(ParameterListSyntax parameterList, int maximum)
        {
            if (!SpansMoreThanOneLine(parameterList))
            {
                return false;
            }

            // A line comment, a documentation comment, or a preprocessor directive cannot be kept in the one-line spelling this diagnostic recommends.
            if (ContainsLineComment(parameterList) || ContainsPreprocessorDirective(parameterList))
            {
                return false;
            }

            // A line break inside a token is literal text. The one-line spelling cannot keep it.
            if (ContainsLiteralLineBreak(parameterList.Parameters))
            {
                return false;
            }

            return JoinedLength(parameterList.Parameters) <= maximum;
        }

        private static bool ContainsLineComment(ParameterListSyntax parameterList)
        {
            foreach (SyntaxTrivia trivia in parameterList.DescendantTrivia())
            {
                // /// is documentation trivia, not a // comment, and its span starts after the slashes.
                // A check for // never sees it. The one-line form still comments out the rest of the signature.
                if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                    || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsPreprocessorDirective(ParameterListSyntax parameterList)
        {
            foreach (SyntaxTrivia trivia in parameterList.DescendantTrivia())
            {
                if (trivia.IsDirective)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsLiteralLineBreak(SeparatedSyntaxList<ParameterSyntax> parameters)
        {
            foreach (ParameterSyntax parameter in parameters)
            {
                foreach (SyntaxToken token in parameter.DescendantTokens())
                {
                    if (HasSourceLineBreak(token.Text))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool HasSourceLineBreak(string text)
        {
            // token.Text is the source spelling. ValueText would decode a regular "\n"
            // into a line break and silence a list that still fits on one line.
            foreach (char character in text)
            {
                if (character == '\r'
                    || character == '\n'
                    || character == '\u0085'
                    || character == '\u2028'
                    || character == '\u2029')
                {
                    return true;
                }
            }

            return false;
        }

        internal static int JoinedLength(SeparatedSyntaxList<ParameterSyntax> parameters)
        {
            int length = 0;
            for (int index = 0; index < parameters.Count; index++)
            {
                if (index > 0)
                {
                    length += 2;
                }

                length += Normalize(parameters[index]).Length;
            }

            return length;
        }

        private static bool SpansMoreThanOneLine(ParameterListSyntax parameterList)
        {
            FileLinePositionSpan span = parameterList.GetLocation().GetLineSpan();
            return span.StartLinePosition.Line != span.EndLinePosition.Line;
        }

        private static string Normalize(ParameterSyntax parameter)
        {
            string text = parameter.ToString();
            var builder = new StringBuilder(text.Length);
            bool pendingSpace = false;
            bool any = false;
            int position = 0;
            foreach (SyntaxToken token in parameter.DescendantTokens())
            {
                int tokenStart = token.SpanStart - parameter.SpanStart;
                for (; position < tokenStart; position++)
                {
                    char character = text[position];
                    if (char.IsWhiteSpace(character))
                    {
                        pendingSpace = any;
                        continue;
                    }

                    if (pendingSpace)
                    {
                        builder.Append(' ');
                        pendingSpace = false;
                    }

                    builder.Append(character);
                    any = true;
                }

                if (pendingSpace)
                {
                    builder.Append(' ');
                    pendingSpace = false;
                }

                // Literal whitespace is part of the default value, not removable formatting.
                builder.Append(token.Text);
                any = true;
                position = tokenStart + token.Span.Length;
            }

            return builder.ToString();
        }
    }
}
