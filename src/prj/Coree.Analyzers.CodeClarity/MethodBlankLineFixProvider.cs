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
    /// Inserts the blank line that CCCMB001 asks for before a method.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MethodBlankLineFixProvider))]
    [Shared]
    public sealed class MethodBlankLineFixProvider : CodeFixProvider
    {
        /// <inheritdoc />
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(MethodBlankLineAnalyzer.DiagnosticId);

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
            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                if (ReferenceEquals(text, MethodBlankLineInsertion.Apply(text, root, diagnostic.Location.SourceSpan)))
                {
                    continue;
                }

                Diagnostic captured = diagnostic;
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Insert a blank line before this method",
                        cancellationToken => InsertAsync(context.Document, captured, cancellationToken),
                        MethodBlankLineAnalyzer.DiagnosticId),
                    captured);
            }
        }

        private static async Task<Document> InsertAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            SourceText text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;
            SourceText updated = MethodBlankLineInsertion.Apply(text, root, diagnostic.Location.SourceSpan);
            return document.WithText(updated);
        }
    }
}
