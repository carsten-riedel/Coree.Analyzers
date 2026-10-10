#nullable enable
using System.Text;
using Microsoft.CodeAnalysis;
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

            return JoinedLength(parameterList.Parameters) <= maximum;
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

                length += Normalize(parameters[index].ToString()).Length;
            }

            return length;
        }

        private static bool SpansMoreThanOneLine(ParameterListSyntax parameterList)
        {
            FileLinePositionSpan span = parameterList.GetLocation().GetLineSpan();
            return span.StartLinePosition.Line != span.EndLinePosition.Line;
        }

        private static string Normalize(string text)
        {
            var builder = new StringBuilder(text.Length);
            bool pendingSpace = false;
            bool any = false;
            for (int index = 0; index < text.Length; index++)
            {
                char character = text[index];
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

            return builder.ToString();
        }
    }
}
