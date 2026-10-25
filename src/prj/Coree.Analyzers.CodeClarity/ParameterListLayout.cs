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
            int previousLine = firstLine;
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

                previousLine = line;
                previousSpan = span;
            }

            if (oneLine)
            {
                return false;
            }

            // The list form puts the closing parenthesis on the line after the last parameter.
            if (!onOpenLine && !sharesLine && closeLine == previousLine + 1)
            {
                return false;
            }

            return true;
        }

        private static int Line(Location location)
        {
            return location.GetLineSpan().StartLinePosition.Line;
        }

        private static bool SpansShareLine(FileLinePositionSpan earlier, FileLinePositionSpan later)
        {
            // Parameters are visited in source order, so a shared line is a later parameter
            // that begins on or above the earlier parameter's last line.
            return later.StartLinePosition.Line <= earlier.EndLinePosition.Line;
        }
    }
}
