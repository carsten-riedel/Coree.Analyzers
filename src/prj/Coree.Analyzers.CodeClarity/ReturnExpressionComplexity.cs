#nullable enable
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Coree.Analyzers.CodeClarity
{
    internal static class ReturnExpressionComplexity
    {
        internal static bool Exceeds(ExpressionSyntax expression, SemanticModel model, int maximum)
        {
            return Score(expression, model) > maximum;
        }

        private static int Score(SyntaxNode node, SemanticModel model)
        {
            if (node is AnonymousFunctionExpressionSyntax)
            {
                return 0;
            }

            int score = 0;
            if (node is InvocationExpressionSyntax invocation && !IsNameof(invocation))
            {
                score++;
            }
            else if (node is ConditionalExpressionSyntax)
            {
                score++;
            }
            else if (node is BinaryExpressionSyntax binary && IsCalculation(binary, model))
            {
                score++;
            }

            foreach (SyntaxNode child in node.ChildNodes())
            {
                score += Score(child, model);
            }

            return score;
        }

        private static bool IsNameof(InvocationExpressionSyntax invocation)
        {
            IdentifierNameSyntax? name = invocation.Expression as IdentifierNameSyntax;
            return name != null && name.Identifier.ValueText == "nameof";
        }

        private static bool IsCalculation(BinaryExpressionSyntax binary, SemanticModel model)
        {
            SyntaxKind kind = binary.Kind();
            bool arithmetic = kind == SyntaxKind.AddExpression
                || kind == SyntaxKind.SubtractExpression
                || kind == SyntaxKind.MultiplyExpression
                || kind == SyntaxKind.DivideExpression
                || kind == SyntaxKind.ModuloExpression
                || kind == SyntaxKind.LeftShiftExpression
                || kind == SyntaxKind.RightShiftExpression;
            if (!arithmetic)
            {
                return false;
            }

            if (kind == SyntaxKind.AddExpression && IsStringConcatenation(binary, model))
            {
                return false;
            }

            return true;
        }

        private static bool IsStringConcatenation(BinaryExpressionSyntax binary, SemanticModel model)
        {
            return HasStringType(binary.Left, model) || HasStringType(binary.Right, model);
        }

        private static bool HasStringType(ExpressionSyntax expression, SemanticModel model)
        {
            return model.GetTypeInfo(expression).Type is { SpecialType: SpecialType.System_String };
        }
    }
}
