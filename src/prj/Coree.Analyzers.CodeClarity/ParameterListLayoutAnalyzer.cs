using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Coree.Analyzers.CodeClarity
{
    /// <summary>
    /// Warns when a parameter list is neither one line nor one parameter per line.
    /// One line includes both parentheses. The list form starts each parameter on its own line and puts the closing parenthesis on the line after the last parameter ends.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ParameterListLayoutAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Diagnostic identifier for a parameter list that is neither entirely on one line nor one parameter per line with the closing parenthesis on the following line.
        /// </summary>
        public const string DiagnosticId = "CCCPL001";

        internal const string SeverityPropertyName = "ParameterListLayoutAnalyzerSeverity";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            "Parameter list is neither one line nor one parameter per line",
            "Use one line, as in M(a, b, c). Or end the signature with (, put one parameter on each following line, and put ) alone on the line after the last parameter, as in M(\\n    a,\\n    b,\\n    c\\n).",
            "Clarity",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Reports a method, constructor, or primary-constructor parameter list that is neither M(a, b, c) nor M(\\n    a,\\n    b,\\n    c\\n). The one-line form keeps both parentheses on that line. The list form ends the signature with (, starts one parameter on each following line, and puts ) alone on the line after the last line of the last parameter. A parameter may span lines.");

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
            context.RegisterSyntaxNodeAction(
                AnalyzeMethod,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.ConstructorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeLocalFunction, SyntaxKind.LocalFunctionStatement);
            context.RegisterSyntaxNodeAction(
                AnalyzeType,
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.RecordDeclaration,
                SyntaxKind.RecordStructDeclaration);
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            BaseMethodDeclarationSyntax method = (BaseMethodDeclarationSyntax)context.Node;
            ReportIfMixed(context, method.ParameterList);
        }

        private static void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
        {
            LocalFunctionStatementSyntax localFunction = (LocalFunctionStatementSyntax)context.Node;
            ReportIfMixed(context, localFunction.ParameterList);
        }

        private static void AnalyzeType(SyntaxNodeAnalysisContext context)
        {
            TypeDeclarationSyntax type = (TypeDeclarationSyntax)context.Node;
            ReportIfMixed(context, type.ParameterList);
        }

        private static void ReportIfMixed(SyntaxNodeAnalysisContext context, ParameterListSyntax? parameterList)
        {
            if (parameterList == null)
            {
                return;
            }

            AnalyzerConfigOptions options = context.Options.AnalyzerConfigOptionsProvider.GlobalOptions;
            DiagnosticSeverity? severity = AnalyzerSeverity.Read(options, SeverityPropertyName);
            if (!severity.HasValue)
            {
                return;
            }

            if (!ParameterListLayout.IsDisallowed(parameterList))
            {
                return;
            }

            DiagnosticDescriptor rule = AnalyzerSeverity.SelectDescriptor(Rule, ErrorRule, InfoRule, severity.Value);
            context.ReportDiagnostic(Diagnostic.Create(rule, parameterList.GetLocation()));
        }
    }
}
