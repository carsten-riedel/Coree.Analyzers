using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Coree.Analyzers.CodeClarity
{
    /// <summary>
    /// Rewrites the parameter list CCCPL002 asks for onto one line.
    /// </summary>
    internal static class ParameterListJoin
    {
        internal static SourceText Apply(SourceText text, SyntaxNode root, TextSpan span, int maximum)
        {
            ParameterListSyntax? list = Find(root, span);
            if (list == null)
            {
                return text;
            }

            if (!ParameterListFit.UsesBreakButFits(list, maximum))
            {
                return text;
            }

            string joined = ParameterListFit.OneLineList(list);
            if (!ParameterListFit.OneLineKeepsText(list, joined))
            {
                return text;
            }

            return text.Replace(list.Span, joined);
        }

        private static ParameterListSyntax? Find(SyntaxNode root, TextSpan span)
        {
            SyntaxNode node = root.FindNode(span);
            ParameterListSyntax? list = node as ParameterListSyntax;
            if (list != null)
            {
                return list;
            }

            return node.FirstAncestorOrSelf<ParameterListSyntax>();
        }
    }
}
