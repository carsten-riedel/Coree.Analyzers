#nullable enable
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Coree.Analyzers.CodeClarity
{
    internal static class ParameterListLayout
    {
        internal static bool IsMixed(ParameterListSyntax parameterList)
        {
            SeparatedSyntaxList<ParameterSyntax> parameters = parameterList.Parameters;
            if (parameters.Count < 2)
            {
                return false;
            }

            int firstLine = StartLine(parameters[0]);
            int currentLine = firstLine;
            bool packedLine = false;
            bool laterLine = false;
            for (int index = 1; index < parameters.Count; index++)
            {
                int line = StartLine(parameters[index]);
                if (line == currentLine)
                {
                    packedLine = true;
                }
                else
                {
                    currentLine = line;
                }

                if (line != firstLine)
                {
                    laterLine = true;
                }
            }

            return packedLine && laterLine;
        }

        private static int StartLine(ParameterSyntax parameter)
        {
            FileLinePositionSpan span = parameter.GetLocation().GetLineSpan();
            return span.StartLinePosition.Line;
        }
    }
}
