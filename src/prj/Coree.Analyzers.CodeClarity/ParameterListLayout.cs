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
    }
}
