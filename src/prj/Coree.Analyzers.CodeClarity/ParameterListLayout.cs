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
            int firstLine = Line(parameters[0].GetLocation());
            bool oneLine = firstLine == openLine && firstLine == closeLine;
            bool onOpenLine = firstLine == openLine;
            bool sharesLine = false;
            int previousLine = firstLine;

            for (int index = 1; index < parameters.Count; index++)
            {
                int line = Line(parameters[index].GetLocation());
                if (line != openLine)
                {
                    oneLine = false;
                }

                if (line != closeLine)
                {
                    oneLine = false;
                }

                if (line == previousLine)
                {
                    sharesLine = true;
                }

                if (line == openLine)
                {
                    onOpenLine = true;
                }

                previousLine = line;
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
    }
}
