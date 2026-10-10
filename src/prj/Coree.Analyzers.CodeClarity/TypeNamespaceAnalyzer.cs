using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Coree.Analyzers.CodeClarity
{
    /// <summary>
    /// Warns when a class, struct, record, interface, enum, or delegate is declared outside a namespace.
    /// Nested types are left to the outer type. A block namespace and a file-scoped namespace both count.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TypeNamespaceAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Diagnostic identifier for a type declared outside a namespace.
        /// </summary>
        public const string DiagnosticId = "CCCNS001";

        internal const string SeverityPropertyName = "TypeNamespaceAnalyzerSeverity";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            "Type is not in a namespace",
            // No format arguments, so these braces stay literal. Doubling them would show {{ }} in the diagnostic.
            "Put this type in a namespace. Use a block, as in namespace Game { class Terrain {} }, or a file-scoped namespace, as in namespace Game; class Terrain {}.",
            "Clarity",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Reports a class, struct, record, interface, enum, or delegate whose declaration is not inside a namespace. A nested type is not reported on its own. A block namespace and a file-scoped namespace both count.");

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
            context.RegisterSyntaxNodeAction(AnalyzeEnum, SyntaxKind.EnumDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeDelegate, SyntaxKind.DelegateDeclaration);
        }

        private static void AnalyzeType(SyntaxNodeAnalysisContext context)
        {
            TypeDeclarationSyntax type = (TypeDeclarationSyntax)context.Node;
            ReportIfGlobal(context, type, type.Identifier);
        }

        private static void AnalyzeEnum(SyntaxNodeAnalysisContext context)
        {
            EnumDeclarationSyntax enumeration = (EnumDeclarationSyntax)context.Node;
            ReportIfGlobal(context, enumeration, enumeration.Identifier);
        }

        private static void AnalyzeDelegate(SyntaxNodeAnalysisContext context)
        {
            DelegateDeclarationSyntax declaration = (DelegateDeclarationSyntax)context.Node;
            ReportIfGlobal(context, declaration, declaration.Identifier);
        }

        private static void ReportIfGlobal(SyntaxNodeAnalysisContext context, SyntaxNode declaration, SyntaxToken identifier)
        {
            AnalyzerConfigOptions options = context.Options.AnalyzerConfigOptionsProvider.GlobalOptions;
            DiagnosticSeverity? severity = AnalyzerSeverity.Read(options, SeverityPropertyName);
            if (!severity.HasValue)
            {
                return;
            }

            if (declaration.Parent is not CompilationUnitSyntax)
            {
                return;
            }

            DiagnosticDescriptor rule = AnalyzerSeverity.SelectDescriptor(Rule, ErrorRule, InfoRule, severity.Value);
            context.ReportDiagnostic(Diagnostic.Create(rule, identifier.GetLocation()));
        }
    }
}
