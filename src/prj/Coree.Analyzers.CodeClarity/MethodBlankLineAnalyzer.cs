using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Coree.Analyzers.CodeClarity
{
    /// <summary>
    /// Warns when two methods with a body sit next to each other in a type without a blank line between them.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MethodBlankLineAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Diagnostic identifier for a method that is not separated from the previous method by a blank line.
        /// </summary>
        public const string DiagnosticId = "CCCMB001";

        internal const string SeverityPropertyName = "MethodBlankLineAnalyzerSeverity";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            "Method is not separated by a blank line",
            // No format arguments, so these braces stay literal. Doubling them would show {{ }} in the diagnostic.
            "Put a blank line before this method. End the previous method, leave one empty line, then start this method, as in void Walkable() {}\\n\\nvoid StaticSegmentClear() {}.",
            "Clarity",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Reports the later of two methods with a body that are consecutive members of the same type and have no blank line between them. A whitespace-only line counts. A comment or documentation line does not. Constructors, fields, properties, operators, indexers, local functions, and methods without a body stay quiet.");

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
                AnalyzeType,
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.RecordDeclaration,
                SyntaxKind.RecordStructDeclaration,
                SyntaxKind.InterfaceDeclaration);
        }

        private static void AnalyzeType(SyntaxNodeAnalysisContext context)
        {
            AnalyzerConfigOptions options = context.Options.AnalyzerConfigOptionsProvider.GlobalOptions;
            DiagnosticSeverity? severity = AnalyzerSeverity.Read(options, SeverityPropertyName);
            if (!severity.HasValue)
            {
                return;
            }

            TypeDeclarationSyntax type = (TypeDeclarationSyntax)context.Node;
            DiagnosticDescriptor rule = AnalyzerSeverity.SelectDescriptor(Rule, ErrorRule, InfoRule, severity.Value);
            MemberDeclarationSyntax? previous = null;
            foreach (MemberDeclarationSyntax member in type.Members)
            {
                if (member is MethodDeclarationSyntax method
                    && previous is MethodDeclarationSyntax prior
                    && MethodBlankLine.MissingBefore(prior, method))
                {
                    context.ReportDiagnostic(Diagnostic.Create(rule, method.Identifier.GetLocation()));
                }

                previous = member;
            }
        }
    }
}
