using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Coree.Analyzers.CodeClarity
{
    /// <summary>
    /// Warns when a parameter list is broken across lines even though the parameters fit in the configured length.
    /// The default limit is 120 characters. Formatting whitespace collapses; literal text is preserved.
    /// A single-line comment, a documentation comment, a preprocessor directive, or a literal line break keeps the list quiet, because that text cannot stay on one line.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ParameterListFitAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Diagnostic identifier for a broken parameter list whose parameters would fit on one line.
        /// </summary>
        public const string DiagnosticId = "CCCPL002";

        internal const string SeverityPropertyName = "ParameterListFitAnalyzerSeverity";

        internal const string MaximumPropertyName = "ParameterListFitAnalyzerMaximum";

        internal const int DefaultMaximum = 120;

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            "Parameter list breaks although it fits",
            "This parameter list is broken across lines, but the parameters fit in {0} characters (limit {1}). Write it on one line, as in M(a, b, c).",
            "Clarity",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Joins parameters with a comma and a space after collapsing whitespace outside tokens, preserving literal text, then compares that length with ParameterListFitAnalyzerMaximum. Parentheses are not counted. A single-line comment, a documentation comment, a preprocessor directive, or a literal line break is not treated as fitting on one line. The default limit is 120. An empty, negative, or non-numeric maximum keeps that default.");

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
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.OperatorDeclaration,
                SyntaxKind.ConversionOperatorDeclaration);
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
            ReportIfFits(context, method.ParameterList);
        }

        private static void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
        {
            LocalFunctionStatementSyntax localFunction = (LocalFunctionStatementSyntax)context.Node;
            ReportIfFits(context, localFunction.ParameterList);
        }

        private static void AnalyzeType(SyntaxNodeAnalysisContext context)
        {
            TypeDeclarationSyntax type = (TypeDeclarationSyntax)context.Node;
            ReportIfFits(context, type.ParameterList);
        }

        private static void ReportIfFits(SyntaxNodeAnalysisContext context, ParameterListSyntax? parameterList)
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

            int maximum = ReadMaximum(options);
            if (!ParameterListFit.UsesBreakButFits(parameterList, maximum))
            {
                return;
            }

            int length = ParameterListFit.JoinedLength(parameterList.Parameters);
            DiagnosticDescriptor rule = AnalyzerSeverity.SelectDescriptor(Rule, ErrorRule, InfoRule, severity.Value);
            context.ReportDiagnostic(Diagnostic.Create(rule, parameterList.GetLocation(), length, maximum));
        }

        internal static int ReadMaximum(AnalyzerConfigOptions options)
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
