using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Coree.Analyzers.Typography
{
    /// <summary>
    /// Warns when C# source or matching files under the project directory contain a horizontal
    /// ellipsis (U+2026), not three ASCII periods.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class EllipsisAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Diagnostic identifier for the horizontal ellipsis rule.
        /// </summary>
        public const string DiagnosticId = "CTYEL001";

        internal const string SeverityPropertyName = "EllipsisAnalyzerSeverity";

        internal const string IncludesPropertyName = "EllipsisAnalyzerIncludes";

        internal const string ExcludesPropertyName = "EllipsisAnalyzerExcludes";

        internal const string AdditionalExcludesPropertyName = "EllipsisAnalyzerAdditionalExcludes";

        // …
        private const string EllipsisCharacters = "\u2026";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            "Source contains a horizontal ellipsis",
            "Source contains a horizontal ellipsis (U+2026). Use three ASCII periods.",
            "Typography",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Copy-paste from word processors often inserts U+2026 instead of three ASCII periods.");

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
            SourceCharacterScanner.Register(
                context,
                Rule,
                ErrorRule,
                InfoRule,
                EllipsisCharacters,
                SeverityPropertyName,
                IncludesPropertyName,
                ExcludesPropertyName,
                AdditionalExcludesPropertyName);
        }
    }
}
