using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Coree.Analyzers.Typography
{
    /// <summary>
    /// Warns when C# source or matching files under the project directory contain a typographic apostrophe
    /// / closing single quotation mark (U+2019), not ASCII <c>'</c>.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ApostropheAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Diagnostic identifier for the typographic apostrophe rule.
        /// </summary>
        public const string DiagnosticId = "CTYAP001";

        internal const string SeverityPropertyName = "ApostropheAnalyzerSeverity";

        internal const string IncludesPropertyName = "ApostropheAnalyzerIncludes";

        internal const string ExcludesPropertyName = "ApostropheAnalyzerExcludes";

        internal const string AdditionalExcludesPropertyName = "ApostropheAnalyzerAdditionalExcludes";

        // ’
        private const string ApostropheCharacters = "\u2019";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            "Source contains a typographic apostrophe",
            "Source contains a typographic apostrophe or closing single quotation mark (U+2019). Use ASCII apostrophe.",
            "Typography",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Copy-paste from word processors often inserts U+2019 instead of ASCII apostrophe (U+0027).");

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
                ApostropheCharacters,
                SeverityPropertyName,
                IncludesPropertyName,
                ExcludesPropertyName,
                AdditionalExcludesPropertyName);
        }
    }
}
