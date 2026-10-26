using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Coree.Analyzers.CodeClarity
{
    /// <summary>
    /// Warns when a <c>return</c>, <c>yield return</c>, or returned arrow expression exceeds the allowed number of calculation steps.
    /// The default maximum is one. A block names the extra steps as locals. An arrow has to become a block before it can do that.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ReturnTypeComplexityAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Diagnostic identifier for a return, yield return, or returned arrow expression that exceeds the complexity maximum.
        /// </summary>
        public const string DiagnosticId = "CCCRC001";

        internal const string SeverityPropertyName = "ReturnTypeComplexityAnalyzerSeverity";

        internal const string MaximumPropertyName = "ReturnTypeComplexityAnalyzerMaximum";

        internal const int DefaultMaximum = 1;

        private const string BlockAdvice = "Extract intermediate calculations into local variables";

        // An arrow expression cannot declare a local. The block sentence would tell the reader
        // to extract locals in a form that has nowhere to put them.
        private const string ArrowAdvice = "Move this arrow into a block and extract the extra steps into local variables there";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            "Return expression is too complex",
            "Return expression is too complex. {0}.",
            "Clarity",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Counts arithmetic operators (except string concatenation), method calls other than nameof, and conditional operators in a return, yield return, or returned arrow expression. A user-defined operator + counts even when one operand is a string. A void arrow, a constructor, a setter, a lambda, and an async method that returns a non-generic Task or ValueTask are not scored. A return or yield return says to extract the extra steps into local variables. A returned arrow says to move into a block first, because an arrow cannot declare a local. ReturnTypeComplexityAnalyzerMaximum is the allowed score; the default is 1.");

        private static readonly DiagnosticDescriptor ErrorRule =
            AnalyzerSeverity.WithSeverity(Rule, DiagnosticSeverity.Error);

        private static readonly DiagnosticDescriptor InfoRule =
            AnalyzerSeverity.WithSeverity(Rule, DiagnosticSeverity.Info);

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(Rule); }
        }

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            // An iterator produces its value with yield return, not return. Registering only
            // ReturnStatement left yield return a + b + c silent at the default maximum.
            // Roslyn 4.14 models that statement as YieldStatementSyntax. yield break has no expression.
            context.RegisterSyntaxNodeAction(
                AnalyzeReturn,
                SyntaxKind.ReturnStatement,
                SyntaxKind.YieldReturnStatement);
            // int M() => a + b + c is the same returned value as return a + b + c.
            // A void arrow and an async Task or ValueTask arrow are statements, not returned values.
            context.RegisterSyntaxNodeAction(AnalyzeMethodArrow, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeLocalFunctionArrow, SyntaxKind.LocalFunctionStatement);
            context.RegisterSyntaxNodeAction(AnalyzePropertyArrow, SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeIndexerArrow, SyntaxKind.IndexerDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeGetAccessorArrow, SyntaxKind.GetAccessorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeOperatorArrow, SyntaxKind.OperatorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeConversionArrow, SyntaxKind.ConversionOperatorDeclaration);
        }

        private static void AnalyzeReturn(SyntaxNodeAnalysisContext context)
        {
            ExpressionSyntax? expression = null;
            if (context.Node is ReturnStatementSyntax statement)
            {
                expression = statement.Expression;
            }

            if (context.Node is YieldStatementSyntax yieldStatement)
            {
                expression = yieldStatement.Expression;
            }

            if (expression == null)
            {
                return;
            }

            ReportIfExceeds(context, expression, BlockAdvice);
        }

        private static void AnalyzeMethodArrow(SyntaxNodeAnalysisContext context)
        {
            MethodDeclarationSyntax method = (MethodDeclarationSyntax)context.Node;
            ReportArrow(context, ValueExpression(method.ExpressionBody, context.SemanticModel.GetDeclaredSymbol(method) as IMethodSymbol));
        }

        private static void AnalyzeLocalFunctionArrow(SyntaxNodeAnalysisContext context)
        {
            LocalFunctionStatementSyntax localFunction = (LocalFunctionStatementSyntax)context.Node;
            ReportArrow(context, ValueExpression(localFunction.ExpressionBody, context.SemanticModel.GetDeclaredSymbol(localFunction) as IMethodSymbol));
        }

        private static void AnalyzePropertyArrow(SyntaxNodeAnalysisContext context)
        {
            ReportArrow(context, ((PropertyDeclarationSyntax)context.Node).ExpressionBody?.Expression);
        }

        private static void AnalyzeIndexerArrow(SyntaxNodeAnalysisContext context)
        {
            ReportArrow(context, ((IndexerDeclarationSyntax)context.Node).ExpressionBody?.Expression);
        }

        private static void AnalyzeGetAccessorArrow(SyntaxNodeAnalysisContext context)
        {
            ReportArrow(context, ((AccessorDeclarationSyntax)context.Node).ExpressionBody?.Expression);
        }

        private static void AnalyzeOperatorArrow(SyntaxNodeAnalysisContext context)
        {
            ReportArrow(context, ((OperatorDeclarationSyntax)context.Node).ExpressionBody?.Expression);
        }

        private static void AnalyzeConversionArrow(SyntaxNodeAnalysisContext context)
        {
            ReportArrow(context, ((ConversionOperatorDeclarationSyntax)context.Node).ExpressionBody?.Expression);
        }

        private static void ReportArrow(SyntaxNodeAnalysisContext context, ExpressionSyntax? expression)
        {
            if (expression == null)
            {
                return;
            }

            ReportIfExceeds(context, expression, ArrowAdvice);
        }

        private static void ReportIfExceeds(SyntaxNodeAnalysisContext context, ExpressionSyntax expression, string advice)
        {
            AnalyzerConfigOptions options = context.Options.AnalyzerConfigOptionsProvider.GlobalOptions;
            DiagnosticSeverity? severity = AnalyzerSeverity.Read(options, SeverityPropertyName);
            if (!severity.HasValue)
            {
                return;
            }

            int maximum = ReadMaximum(options);
            if (!ReturnExpressionComplexity.Exceeds(expression, context.SemanticModel, maximum))
            {
                return;
            }

            DiagnosticDescriptor rule = AnalyzerSeverity.SelectDescriptor(Rule, ErrorRule, InfoRule, severity.Value);
            context.ReportDiagnostic(Diagnostic.Create(rule, expression.GetLocation(), advice));
        }

        private static ExpressionSyntax? ValueExpression(ArrowExpressionClauseSyntax? arrow, IMethodSymbol? method)
        {
            if (arrow == null || !ReturnsValue(method))
            {
                return null;
            }

            return arrow.Expression;
        }

        // A block body reports only a return expression. void M() { Write(a + b + c); } stays quiet,
        // and so does async Task M() { Write(a + b + c); }. Those arrows are statements, not returned
        // values. Scoring them would warn where the block form does not. A missing symbol stays quiet.
        internal static bool ReturnsValue(IMethodSymbol? method)
        {
            if (method == null || method.ReturnsVoid)
            {
                return false;
            }

            if (!method.IsAsync)
            {
                return true;
            }

            // Task<int> displays as System.Threading.Tasks.Task<int>, so that arrow is still scored.
            string fullName = method.ReturnType.ToDisplayString();
            return fullName != "System.Threading.Tasks.Task"
                && fullName != "System.Threading.Tasks.ValueTask";
        }

        private static int ReadMaximum(AnalyzerConfigOptions options)
        {
            string? read;
            if (!options.TryGetValue("build_property." + MaximumPropertyName, out read) || string.IsNullOrWhiteSpace(read))
            {
                return DefaultMaximum;
            }

            int value;
            if (!int.TryParse(read.Trim(), out value) || value < 0)
            {
                return DefaultMaximum;
            }

            return value;
        }
    }
}
