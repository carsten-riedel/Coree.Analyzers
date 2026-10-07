using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Coree.Analyzers.Typography
{
    /// <summary>
    /// Warns when C# source or matching files under the project directory contain a typographic
    /// en dash (U+2013) or em dash (U+2014), not ASCII hyphen-minus.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class EmDashAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Diagnostic identifier for the typographic dash rule.
        /// </summary>
        public const string DiagnosticId = "CTYED001";

        internal const string SeverityPropertyName = "EmDashAnalyzerSeverity";

        internal const string IncludesPropertyName = "EmDashAnalyzerIncludes";

        internal const string ExcludesPropertyName = "EmDashAnalyzerExcludes";

        internal const string AdditionalExcludesPropertyName = "EmDashAnalyzerAdditionalExcludes";

        private const string EmDashCharacters = "\u2013\u2014";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            "Source contains a typographic dash",
            "Source contains a typographic en dash or em dash (U+2013 / U+2014). Use ASCII hyphen-minus.",
            "Typography",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Copy-paste from word processors often inserts en or em dashes instead of ASCII hyphen-minus (U+002D).");

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
                EmDashCharacters,
                SeverityPropertyName,
                IncludesPropertyName,
                ExcludesPropertyName,
                AdditionalExcludesPropertyName);
        }
    }
}
