#nullable enable
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Coree.Analyzers.CodeClarity
{
    /// <summary>
    /// Rewrites a CCCPL001 parameter list into one parameter per line.
    /// Lists that CCCPL002 would put on one line are left alone.
    /// </summary>
    internal static class ParameterListLayoutFix
    {
        internal static SourceText Apply(SourceText text, SyntaxNode root, TextSpan span, int maximum)
        {
            ParameterListSyntax? list = FindList(root, span);
            if (list == null || !TryBuild(list, text, maximum, out string replacement))
            {
                return text;
            }

            TextSpan edit = TextSpan.FromBounds(list.OpenParenToken.SpanStart, list.CloseParenToken.Span.End);
            return text.Replace(edit, replacement);
        }

        private static ParameterListSyntax? FindList(SyntaxNode root, TextSpan span)
        {
            SyntaxNode? node = root.FindNode(span);
            while (node != null)
            {
                if (node is ParameterListSyntax list)
                {
                    return list;
                }

                node = node.Parent;
            }

            return null;
        }

        private static bool TryBuild(ParameterListSyntax list, SourceText text, int maximum, out string replacement)
        {
            replacement = string.Empty;
            if (list.OpenParenToken.IsMissing || list.CloseParenToken.IsMissing)
            {
                return false;
            }

            if (!ParameterListLayout.IsDisallowed(list))
            {
                return false;
            }

            // Expanding a list that still fits would leave CCCPL002 asking for one line.
            if (ParameterListFit.UsesBreakButFits(list, maximum))
            {
                return false;
            }

            // A comment or directive between parameters sits outside those parameter spans.
            if (HasTextOutsideParameters(list))
            {
                return false;
            }

            replacement = Build(list, text);
            return true;
        }

        private static bool HasTextOutsideParameters(ParameterListSyntax list)
        {
            foreach (SyntaxTrivia trivia in list.DescendantTrivia())
            {
                if (InsideParameter(trivia, list))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(trivia.ToString()))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool InsideParameter(SyntaxTrivia trivia, ParameterListSyntax list)
        {
            foreach (ParameterSyntax parameter in list.Parameters)
            {
                if (parameter.Span.Contains(trivia.Span))
                {
                    return true;
                }
            }

            return false;
        }

        private static string Build(ParameterListSyntax list, SourceText text)
        {
            string newline = LineEndingBefore(text, list.OpenParenToken.SpanStart);
            string baseIndent = LineIndent(text, list.OpenParenToken.SpanStart);
            string parameterIndent = baseIndent + IndentUnit(baseIndent);
            var builder = new StringBuilder();
            builder.Append('(');
            SeparatedSyntaxList<ParameterSyntax> parameters = list.Parameters;
            for (int index = 0; index < parameters.Count; index++)
            {
                builder.Append(newline);
                builder.Append(parameterIndent);
                builder.Append(parameters[index].ToString());
                if (index < parameters.Count - 1)
                {
                    builder.Append(',');
                }
            }

            builder.Append(newline);
            builder.Append(baseIndent);
            builder.Append(')');
            return builder.ToString();
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

        internal static string LineIndent(SourceText text, int position)
        {
            TextLine line = text.Lines.GetLineFromPosition(position);
            string content = line.ToString();
            int count = 0;
            while (count < content.Length && (content[count] == ' ' || content[count] == '\t'))
            {
                count++;
            }

            return content.Substring(0, count);
        }

        private static string IndentUnit(string baseIndent)
        {
            if (baseIndent.Length > 0)
            {
                foreach (char character in baseIndent)
                {
                    if (character != '\t')
                    {
                        return "    ";
                    }
                }

                return "\t";
            }

            return "    ";
        }
    }
}
