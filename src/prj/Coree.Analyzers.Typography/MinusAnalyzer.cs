using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Coree.Analyzers.Typography
{
    /// <summary>
    /// Warns when C# source or matching files under the project directory contain a minus sign
    /// (U+2212), not ASCII hyphen-minus.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MinusAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Diagnostic identifier for the minus sign rule.
        /// </summary>
        public const string DiagnosticId = "CTYMN001";

        internal const string SeverityPropertyName = "MinusAnalyzerSeverity";

        internal const string IncludesPropertyName = "MinusAnalyzerIncludes";

        internal const string ExcludesPropertyName = "MinusAnalyzerExcludes";

        internal const string AdditionalExcludesPropertyName = "MinusAnalyzerAdditionalExcludes";

        // −
        private const string MinusCharacters = "\u2212";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            "Source contains a minus sign",
            "Source contains a minus sign (U+2212). Use ASCII hyphen-minus.",
            "Typography",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Copy-paste from word processors often inserts U+2212 instead of ASCII hyphen-minus (U+002D).");

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
                MinusCharacters,
                SeverityPropertyName,
                IncludesPropertyName,
                ExcludesPropertyName,
                AdditionalExcludesPropertyName);
        }
    }
}
