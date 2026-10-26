#nullable enable
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Coree.Analyzers.Typography
{
    /// <summary>
    /// Replaces one flagged typographic character with the ASCII text the warning already names.
    /// A quote or apostrophe inside a literal is escaped so the literal still parses.
    /// </summary>
    internal static class TypographyAsciiReplacement
    {
        internal static SourceText Apply(SourceText text, TextSpan span, SyntaxNode? root)
        {
            if (span.Length != 1 || span.End > text.Length)
            {
                return text;
            }

            string? raw = ForCharacter(text[span.Start]);
            if (raw == null)
            {
                return text;
            }

            string? replacement = Escape(root, span, raw);
            if (replacement == null)
            {
                return text;
            }

            return text.Replace(span, replacement);
        }

        internal static SourceText ApplyAll(SourceText text, SyntaxNode? root, IEnumerable<TextSpan> spans)
        {
            var ordered = new List<TextSpan>();
            foreach (TextSpan span in spans)
            {
                ordered.Add(span);
            }

            ordered.Sort(CompareEndFirst);
            SourceText updated = text;
            foreach (TextSpan span in ordered)
            {
                // Later spans stay valid when an earlier ellipsis or escaped quote grows.
                updated = Apply(updated, span, root);
            }

            return updated;
        }

        internal static string? ForCharacter(char character)
        {
            switch (character)
            {
                case '\u2013':
                case '\u2014':
                case '\u2212':
                    return "-";
                case '\u201C':
                case '\u201D':
                case '\u201E':
                case '\u201F':
                case '\u00AB':
                case '\u00BB':
                    return "\"";
                case '\u2018':
                case '\u201A':
                case '\u201B':
                case '\u2019':
                    return "'";
                case '\u2026':
                    return "...";
                case '\u00A0':
                case '\u202F':
                    return " ";
                default:
                    return null;
            }
        }

        internal static string Title(string? diagnosticId)
        {
            if (diagnosticId == EmDashAnalyzer.DiagnosticId)
            {
                return "Use ASCII hyphen-minus";
            }

            if (diagnosticId == SmartQuotesAnalyzer.DiagnosticId)
            {
                return "Use an ASCII quote or apostrophe";
            }

            if (diagnosticId == ApostropheAnalyzer.DiagnosticId)
            {
                return "Use ASCII apostrophe";
            }

            if (diagnosticId == EllipsisAnalyzer.DiagnosticId)
            {
                return "Use three ASCII periods";
            }

            if (diagnosticId == MinusAnalyzer.DiagnosticId)
            {
                return "Use ASCII hyphen-minus";
            }

            if (diagnosticId == NbspAnalyzer.DiagnosticId)
            {
                return "Use ASCII space";
            }

            return "Use the ASCII character";
        }

        private static string? Escape(SyntaxNode? root, TextSpan span, string replacement)
        {
            if (root == null)
            {
                return replacement;
            }

            SyntaxToken token = root.FindToken(span.Start, findInsideTrivia: true);
            if (!token.Span.Contains(span.Start))
            {
                return replacement;
            }

            if (IsRawString(token))
            {
                // A raw string's delimiter is a run of quotes. Inserting one more can close it early.
                // """..."""u8 is a raw string too, but Roslyn gives it a Utf8 raw token kind.
                return null;
            }

            if (token.IsKind(SyntaxKind.CharacterLiteralToken))
            {
                string escaped = replacement.Replace("'", "\\'");
                // U+2026 becomes "...", which is not a character literal.
                if (escaped.Length == 1 || escaped == "\\'")
                {
                    return escaped;
                }

                return null;
            }

            // "..."u8 and @"..."u8 are Utf8StringLiteralToken, not StringLiteralToken.
            // Without that kind, a curly quote is inserted as a raw " and the literal splits.
            if (token.IsKind(SyntaxKind.StringLiteralToken)
                || token.IsKind(SyntaxKind.Utf8StringLiteralToken)
                || token.IsKind(SyntaxKind.InterpolatedStringTextToken))
            {
                if (IsVerbatim(token))
                {
                    return replacement.Replace("\"", "\"\"");
                }

                return replacement.Replace("\"", "\\\"");
            }

            return replacement;
        }

        private static bool IsRawString(SyntaxToken token)
        {
            if (token.IsKind(SyntaxKind.SingleLineRawStringLiteralToken)
                || token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken)
                || token.IsKind(SyntaxKind.Utf8SingleLineRawStringLiteralToken)
                || token.IsKind(SyntaxKind.Utf8MultiLineRawStringLiteralToken))
            {
                return true;
            }

            InterpolatedStringExpressionSyntax? interpolated = OwningInterpolatedString(token);
            return interpolated != null && interpolated.StringStartToken.Text.Contains("\"\"\"");
        }

        private static bool IsVerbatim(SyntaxToken token)
        {
            if (token.IsVerbatimStringLiteral())
            {
                return true;
            }

            InterpolatedStringExpressionSyntax? interpolated = OwningInterpolatedString(token);
            return interpolated != null && interpolated.StringStartToken.Text.IndexOf('@') >= 0;
        }

        private static InterpolatedStringExpressionSyntax? OwningInterpolatedString(SyntaxToken token)
        {
            // A hole is its own expression. The outer string's quotes do not wrap the token inside it.
            // Using that outer string doubles a quote in $@"{ "..." } and skips a fixable quote in $"""{ "..." }.
            bool insideHole = false;
            for (SyntaxNode? node = token.Parent; node != null; node = node.Parent)
            {
                if (node is InterpolationSyntax)
                {
                    insideHole = true;
                    continue;
                }

                if (node is not InterpolatedStringExpressionSyntax interpolated)
                {
                    continue;
                }

                if (insideHole)
                {
                    insideHole = false;
                    continue;
                }

                return interpolated;
            }

            return null;
        }

        private static int CompareEndFirst(TextSpan left, TextSpan right)
        {
            return right.Start.CompareTo(left.Start);
        }
    }
}
