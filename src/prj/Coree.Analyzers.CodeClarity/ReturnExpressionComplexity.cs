#nullable enable
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

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
            if (node is InvocationExpressionSyntax invocation && !IsNameof(invocation, model))
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

        private static bool IsNameof(InvocationExpressionSyntax invocation, SemanticModel model)
        {
            IdentifierNameSyntax? name = invocation.Expression as IdentifierNameSyntax;
            // This spelling can also bind to a user-defined method, including an escaped @nameof.
            return name != null && name.Identifier.ValueText == "nameof"
                && model.GetOperation(invocation) is INameOfOperation;
        }

        private static bool IsCalculation(BinaryExpressionSyntax binary, SemanticModel model)
        {
            // && and || are conditional logical operators, not the conditional operator ?:.
            // They stay uncounted. SingleCalculationStepIsSimple locks both forms.
            SyntaxKind kind = binary.Kind();
            bool arithmetic = kind == SyntaxKind.AddExpression
                || kind == SyntaxKind.SubtractExpression
                || kind == SyntaxKind.MultiplyExpression
                || kind == SyntaxKind.DivideExpression
                || kind == SyntaxKind.ModuloExpression
                || kind == SyntaxKind.LeftShiftExpression
                || kind == SyntaxKind.RightShiftExpression
                || kind == SyntaxKind.UnsignedRightShiftExpression;
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
            // A string operand is not enough: a user-defined operator + is an arithmetic step, not concatenation.
            if (model.GetSymbolInfo(binary).Symbol is IMethodSymbol method
                && method.MethodKind == MethodKind.UserDefinedOperator)
            {
                return false;
            }

            bool leftIsString = HasStringType(binary.Left, model);
            bool rightIsString = HasStringType(binary.Right, model);
            return leftIsString || rightIsString;
        }

        private static bool HasStringType(ExpressionSyntax expression, SemanticModel model)
        {
            return model.GetTypeInfo(expression).Type is { SpecialType: SpecialType.System_String };
        }
    }
}
