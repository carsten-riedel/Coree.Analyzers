using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Coree.Analyzers.Typography
{
    /// <summary>
    /// Warns when C# source or matching files under the project directory contain typographic quotation marks
    /// (curly double quotes, guillemets, and single quotation marks U+2018, U+201A, and U+201B), not ASCII quotes.
    /// U+2019 stays on the apostrophe analyzer so a closing single quote is not reported twice.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SmartQuotesAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Diagnostic identifier for the typographic quote rule.
        /// </summary>
        public const string DiagnosticId = "CTYQM001";

        internal const string SeverityPropertyName = "SmartQuotesAnalyzerSeverity";

        internal const string IncludesPropertyName = "SmartQuotesAnalyzerIncludes";

        internal const string ExcludesPropertyName = "SmartQuotesAnalyzerExcludes";

        internal const string AdditionalExcludesPropertyName = "SmartQuotesAnalyzerAdditionalExcludes";

        // U+2019 is intentionally absent: ApostropheAnalyzer already reports that closing mark.
        private const string TypographicQuoteCharacters = "\u201C\u201D\u201E\u201F\u00AB\u00BB\u2018\u201A\u201B";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            "Source contains a typographic quote",
            "Source contains a typographic quotation mark. Use an ASCII quote or apostrophe.",
            "Typography",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Copy-paste from word processors often inserts curly quotes or guillemets instead of ASCII quotes. This includes double quotes U+201C, U+201D, U+201E, and U+201F, guillemets U+00AB and U+00BB, and single quotation marks U+2018, U+201A, and U+201B. U+2019 is reported by CTYAP001 instead.");

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
                TypographicQuoteCharacters,
                SeverityPropertyName,
                IncludesPropertyName,
                ExcludesPropertyName,
                AdditionalExcludesPropertyName);
        }
    }
}
