#nullable enable
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace Coree.Analyzers.Typography
{
    /// <summary>
    /// Replaces a flagged Typography character in a C# document with the ASCII character named by that warning.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TypographyCharacterFixProvider))]
    [Shared]
    public sealed class TypographyCharacterFixProvider : CodeFixProvider
    {
        /// <inheritdoc />
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            EmDashAnalyzer.DiagnosticId,
            SmartQuotesAnalyzer.DiagnosticId,
            ApostropheAnalyzer.DiagnosticId,
            EllipsisAnalyzer.DiagnosticId,
            MinusAnalyzer.DiagnosticId,
            NbspAnalyzer.DiagnosticId);

        /// <inheritdoc />
        public override FixAllProvider GetFixAllProvider()
        {
            return TypographyDocumentFixAll.Instance;
        }

        /// <inheritdoc />
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SourceText text = await context.Document.GetTextAsync(context.CancellationToken).ConfigureAwait(false);
            SyntaxNode root = (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false))!;
            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                TextSpan span = diagnostic.Location.SourceSpan;
                if (ReferenceEquals(text, TypographyAsciiReplacement.Apply(text, span, root)))
                {
                    continue;
                }

                Diagnostic captured = diagnostic;
                context.RegisterCodeFix(
                    CodeAction.Create(
                        TypographyAsciiReplacement.Title(captured.Id),
                        cancellationToken => ReplaceAsync(context.Document, captured, cancellationToken),
                        captured.Id),
                    captured);
            }
        }

        private static async Task<Document> ReplaceAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            SourceText text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;
            SourceText updated = TypographyAsciiReplacement.Apply(text, diagnostic.Location.SourceSpan, root);
            return document.WithText(updated);
        }

        private sealed class TypographyDocumentFixAll : FixAllProvider
        {
            internal static readonly TypographyDocumentFixAll Instance = new TypographyDocumentFixAll();

            private TypographyDocumentFixAll()
            {
            }

            public override IEnumerable<FixAllScope> GetSupportedFixAllScopes()
            {
                yield return FixAllScope.Document;
                yield return FixAllScope.Project;
                yield return FixAllScope.Solution;
            }

            public override Task<CodeAction?> GetFixAsync(FixAllContext context)
            {
                string? equivalenceKey = context.CodeActionEquivalenceKey;
                string title = TypographyAsciiReplacement.Title(equivalenceKey);
                CodeAction action = CodeAction.Create(
                    title,
                    cancellationToken => ReplaceSolutionAsync(context, cancellationToken),
                    equivalenceKey);
                return Task.FromResult<CodeAction?>(action);
            }

            private static async Task<Solution> ReplaceSolutionAsync(FixAllContext context, CancellationToken cancellationToken)
            {
                Solution solution = context.Solution;
                foreach (Document document in Documents(context))
                {
                    solution = await ReplaceOneDocumentAsync(context, solution, document, cancellationToken).ConfigureAwait(false);
                }

                return solution;
            }

            private static async Task<Solution> ReplaceOneDocumentAsync(
                FixAllContext context,
                Solution solution,
                Document document,
                CancellationToken cancellationToken)
            {
                IEnumerable<Diagnostic> diagnostics = await context.GetDocumentDiagnosticsAsync(document).ConfigureAwait(false);
                SourceText text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
                SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;
                // GetDocumentDiagnosticsAsync already limits this list to the fix-all equivalence key.
                var spans = new List<TextSpan>();
                foreach (Diagnostic diagnostic in diagnostics)
                {
                    spans.Add(diagnostic.Location.SourceSpan);
                }

                SourceText updated = TypographyAsciiReplacement.ApplyAll(text, root, spans);
                return solution.WithDocumentText(document.Id, updated);
            }

            private static IEnumerable<Document> Documents(FixAllContext context)
            {
                if (context.Scope == FixAllScope.Document)
                {
                    if (context.Document != null)
                    {
                        yield return context.Document;
                    }

                    yield break;
                }

                if (context.Scope == FixAllScope.Project)
                {
                    foreach (Document document in context.Project.Documents)
                    {
                        yield return document;
                    }

                    yield break;
                }

                foreach (Project project in context.Solution.Projects)
                {
                    foreach (Document document in project.Documents)
                    {
                        yield return document;
                    }
                }
            }
        }
    }
}
