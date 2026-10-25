using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Coree.Analyzers.CodeClarity
{
    /// <summary>
    /// Warns when a <c>return</c> or <c>yield return</c> expression exceeds the allowed number of calculation steps.
    /// The default maximum is one. Extract the extra steps into named locals.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ReturnTypeComplexityAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Diagnostic identifier for a return or yield return expression that exceeds the complexity maximum.
        /// </summary>
        public const string DiagnosticId = "CCCRC001";

        internal const string SeverityPropertyName = "ReturnTypeComplexityAnalyzerSeverity";

        internal const string MaximumPropertyName = "ReturnTypeComplexityAnalyzerMaximum";

        internal const int DefaultMaximum = 1;

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            "Return expression is too complex",
            "Return expression is too complex. Extract intermediate calculations into local variables.",
            "Clarity",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Counts arithmetic operators (except string concatenation), method calls other than nameof, and conditional operators in a return or yield return expression. A user-defined operator + counts even when one operand is a string. ReturnTypeComplexityAnalyzerMaximum is the allowed score; the default is 1.");

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
            context.ReportDiagnostic(Diagnostic.Create(rule, expression.GetLocation()));
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
