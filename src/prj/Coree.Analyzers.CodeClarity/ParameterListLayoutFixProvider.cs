using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Coree.Analyzers.CodeClarity
{
    /// <summary>
    /// Puts each parameter on its own line when CCCPL001 reports and CCCPL002 does not ask for one line.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ParameterListLayoutFixProvider))]
    [Shared]
    public sealed class ParameterListLayoutFixProvider : CodeFixProvider
    {
        /// <inheritdoc />
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(ParameterListLayoutAnalyzer.DiagnosticId);

        /// <inheritdoc />
        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <inheritdoc />
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SourceText text = await context.Document.GetTextAsync(context.CancellationToken).ConfigureAwait(false);
            SyntaxNode root = (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false))!;
            int maximum = ReadMaximum(context.Document);
            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                if (ReferenceEquals(text, ParameterListLayoutFix.Apply(text, root, diagnostic.Location.SourceSpan, maximum)))
                {
                    continue;
                }

                Diagnostic captured = diagnostic;
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Put each parameter on its own line",
                        cancellationToken => ExpandAsync(context.Document, captured, cancellationToken),
                        ParameterListLayoutAnalyzer.DiagnosticId),
                    captured);
            }
        }

        private static async Task<Document> ExpandAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            SourceText text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;
            int maximum = ReadMaximum(document);
            SourceText updated = ParameterListLayoutFix.Apply(text, root, diagnostic.Location.SourceSpan, maximum);
            return document.WithText(updated);
        }

        private static int ReadMaximum(Document document)
        {
            AnalyzerConfigOptions options = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GlobalOptions;
            return ParameterListFitAnalyzer.ReadMaximum(options);
        }
    }
}
