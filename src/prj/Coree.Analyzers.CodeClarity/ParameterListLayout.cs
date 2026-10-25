#nullable enable
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Coree.Analyzers.CodeClarity
{
    internal static class ParameterListLayout
    {
        internal static bool IsDisallowed(ParameterListSyntax parameterList)
        {
            SeparatedSyntaxList<ParameterSyntax> parameters = parameterList.Parameters;
            if (parameters.Count < 2)
            {
                return false;
            }

            int openLine = Line(parameterList.OpenParenToken.GetLocation());
            int closeLine = Line(parameterList.CloseParenToken.GetLocation());
            FileLinePositionSpan firstSpan = parameters[0].GetLocation().GetLineSpan();
            int firstLine = firstSpan.StartLinePosition.Line;
            bool oneLine = firstLine == openLine && firstLine == closeLine;
            bool onOpenLine = firstLine == openLine;
            bool sharesLine = false;
            FileLinePositionSpan previousSpan = firstSpan;

            for (int index = 1; index < parameters.Count; index++)
            {
                FileLinePositionSpan span = parameters[index].GetLocation().GetLineSpan();
                int line = span.StartLinePosition.Line;
                if (line != openLine)
                {
                    oneLine = false;
                }

                if (line != closeLine)
                {
                    oneLine = false;
                }

                if (SpansShareLine(previousSpan, span))
                {
                    sharesLine = true;
                }

                if (line == openLine)
                {
                    onOpenLine = true;
                }

                previousSpan = span;
            }

            if (oneLine)
            {
                return false;
            }

            // Follow the last line of the last parameter, not its first line.
            // A start-line check warns when that parameter legally spans lines, and it
            // misses a closing parenthesis that sits on the parameter's last line.
            int lastEndLine = EndLine(parameters[parameters.Count - 1].GetLocation());
            if (!onOpenLine && !sharesLine && closeLine == lastEndLine + 1)
            {
                return false;
            }

            return true;
        }

        private static int Line(Location location)
        {
            return location.GetLineSpan().StartLinePosition.Line;
        }

        private static int EndLine(Location location)
        {
            return location.GetLineSpan().EndLinePosition.Line;
        }

        private static bool SpansShareLine(FileLinePositionSpan earlier, FileLinePositionSpan later)
        {
            // Parameters are visited in source order, so a shared line is a later parameter
            // that begins on or above the earlier parameter's last line.
            return later.StartLinePosition.Line <= earlier.EndLinePosition.Line;
        }
    }
}
