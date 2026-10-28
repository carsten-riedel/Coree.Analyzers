using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace Coree.Analyzers.CodeClarity
{
    /// <summary>
    /// Writes the one-line parameter list that CCCPL002 asks for.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ParameterListFitFixProvider))]
    [Shared]
    public sealed class ParameterListFitFixProvider : CodeFixProvider
    {
        /// <inheritdoc />
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(ParameterListFitAnalyzer.DiagnosticId);

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
            int maximum = ParameterListFitAnalyzer.ReadMaximum(
                context.Document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GlobalOptions);
            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                if (ReferenceEquals(text, ParameterListJoin.Apply(text, root, diagnostic.Location.SourceSpan, maximum)))
                {
                    continue;
                }

                Diagnostic captured = diagnostic;
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Write this parameter list on one line",
                        cancellationToken => JoinAsync(context.Document, captured, cancellationToken),
                        ParameterListFitAnalyzer.DiagnosticId),
                    captured);
            }
        }

        private static async Task<Document> JoinAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            SourceText text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;
            int maximum = ParameterListFitAnalyzer.ReadMaximum(
                document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GlobalOptions);
            SourceText updated = ParameterListJoin.Apply(text, root, diagnostic.Location.SourceSpan, maximum);
            return document.WithText(updated);
        }
    }
}
