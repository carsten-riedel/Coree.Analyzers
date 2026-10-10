using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Coree.Analyzers.CodeClarity
{
    /// <summary>
    /// Warns when a parameter list spans more than one line and one of those lines still starts more than one parameter.
    /// Every parameter on one line is fine. Each parameter starting on its own line is fine.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ParameterListLayoutAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Diagnostic identifier for a parameter list whose parameters start on more than one line while one of those lines starts more than one parameter.
        /// </summary>
        public const string DiagnosticId = "CCCPL001";

        internal const string SeverityPropertyName = "ParameterListLayoutAnalyzerSeverity";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            "Parameter list is split across packed lines",
            "This parameter list is split across packed lines. Keep every parameter on one line, or start each parameter on its own line.",
            "Clarity",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Reports a method, constructor, or primary-constructor parameter list that starts parameters on more than one line while one of those lines starts more than one parameter.");

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

            if (!ParameterListLayout.IsMixed(parameterList))
            {
                return;
            }

            DiagnosticDescriptor rule = AnalyzerSeverity.SelectDescriptor(Rule, ErrorRule, InfoRule, severity.Value);
            context.ReportDiagnostic(Diagnostic.Create(rule, parameterList.GetLocation()));
        }
    }
}
