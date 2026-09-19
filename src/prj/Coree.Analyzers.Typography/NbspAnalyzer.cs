using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Coree.Analyzers.Typography
{
    /// <summary>
    /// Warns when C# source or matching files under the project directory contain a no-break space
    /// (U+00A0) or narrow no-break space (U+202F), not ASCII space.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NbspAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Diagnostic identifier for the no-break space rule.
        /// </summary>
        public const string DiagnosticId = "CTYNB001";

        internal const string SeverityPropertyName = "NbspAnalyzerSeverity";

        internal const string IncludesPropertyName = "NbspAnalyzerIncludes";

        internal const string ExcludesPropertyName = "NbspAnalyzerExcludes";

        internal const string AdditionalExcludesPropertyName = "NbspAnalyzerAdditionalExcludes";

        // NBSP, NNBSP
        private const string NbspCharacters = "\u00A0\u202F";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            "Source contains a no-break space",
            "Source contains a no-break space or narrow no-break space (U+00A0 / U+202F). Use ASCII space.",
            "Typography",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Copy-paste from word processors and chat models often inserts U+00A0 or U+202F instead of ASCII space (U+0020).");

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
                NbspCharacters,
                SeverityPropertyName,
                IncludesPropertyName,
                ExcludesPropertyName,
                AdditionalExcludesPropertyName);
        }
    }
}
