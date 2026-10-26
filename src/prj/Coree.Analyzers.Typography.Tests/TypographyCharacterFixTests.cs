using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Coree.Analyzers.Typography;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Coree.Analyzers.Typography.Tests
{
    [TestClass]
    public class TypographyCharacterFixTests
    {
        [TestMethod]
        public async Task EmDashAndEnDashFixToHyphen()
        {
            await VerifyAsync<EmDashAnalyzer>(EmDashAnalyzer.DiagnosticId, "class C { string s = \"a\u2014b\"; }", "class C { string s = \"a-b\"; }", '\u2014');
            await VerifyAsync<EmDashAnalyzer>(EmDashAnalyzer.DiagnosticId, "class C { string s = \"a\u2013b\"; }", "class C { string s = \"a-b\"; }", '\u2013');
        }

        [TestMethod]
        public async Task QuoteInCommentStaysRaw()
        {
            await VerifyAsync<SmartQuotesAnalyzer>(
                SmartQuotesAnalyzer.DiagnosticId,
                "class C { } // x\u201Cy",
                "class C { } // x\"y",
                '\u201C');
        }

        [TestMethod]
        public async Task QuoteInNormalStringIsEscaped()
        {
            await VerifyAsync<SmartQuotesAnalyzer>(
                SmartQuotesAnalyzer.DiagnosticId,
                "class C { string s = \"x\u201Cy\"; }",
                "class C { string s = \"x\\\"y\"; }",
                '\u201C');
        }

        [TestMethod]
        public async Task QuoteInVerbatimStringIsDoubled()
        {
            await VerifyAsync<SmartQuotesAnalyzer>(
                SmartQuotesAnalyzer.DiagnosticId,
                "class C { string s = @\"x\u201Cy\"; }",
                "class C { string s = @\"x\"\"y\"; }",
                '\u201C');
        }

        [TestMethod]
        public async Task SingleQuotationMarkInStringBecomesApostrophe()
        {
            await VerifyAsync<SmartQuotesAnalyzer>(
                SmartQuotesAnalyzer.DiagnosticId,
                "class C { string s = \"x\u2018y\"; }",
                "class C { string s = \"x'y\"; }",
                '\u2018');
        }

        [TestMethod]
        public async Task ApostropheInStringBecomesAsciiApostrophe()
        {
            await VerifyAsync<ApostropheAnalyzer>(
                ApostropheAnalyzer.DiagnosticId,
                "class C { string s = \"x\u2019y\"; }",
                "class C { string s = \"x'y\"; }",
                '\u2019');
        }

        [TestMethod]
        public async Task ApostropheInCharacterLiteralIsEscaped()
        {
            await VerifyAsync<ApostropheAnalyzer>(
                ApostropheAnalyzer.DiagnosticId,
                "class C { char c = '\u2019'; }",
                "class C { char c = '\\''; }",
                '\u2019');
        }

        [TestMethod]
        public async Task EllipsisMinusAndSpacesFixToAscii()
        {
            await VerifyAsync<EllipsisAnalyzer>(EllipsisAnalyzer.DiagnosticId, "class C { string s = \"x\u2026y\"; }", "class C { string s = \"x...y\"; }", '\u2026');
            await VerifyAsync<MinusAnalyzer>(MinusAnalyzer.DiagnosticId, "class C { string s = \"x\u2212y\"; }", "class C { string s = \"x-y\"; }", '\u2212');
            await VerifyAsync<NbspAnalyzer>(NbspAnalyzer.DiagnosticId, "class C { string s = \"x\u00A0y\"; }", "class C { string s = \"x y\"; }", '\u00A0');
            await VerifyAsync<NbspAnalyzer>(NbspAnalyzer.DiagnosticId, "class C { string s = \"x\u202Fy\"; }", "class C { string s = \"x y\"; }", '\u202F');
        }

        [TestMethod]
        public async Task TwoEmDashesFixTogether()
        {
            const string source = "class C { string s = \"a\u2014b\u2014c\"; }";
            const string fixedSource = "class C { string s = \"a-b-c\"; }";
            await CSharpCodeFixVerifier<EmDashAnalyzer, TypographyCharacterFixProvider, DefaultVerifier>
                .VerifyCodeFixAsync(source, new[] { Warning(EmDashAnalyzer.DiagnosticId, source, '\u2014'), Warning(EmDashAnalyzer.DiagnosticId, source, '\u2014', 1) }, fixedSource);
        }

        [TestMethod]
        public async Task TwoEllipsesFixFromTheEnd()
        {
            const string source = "class C { string s = \"a\u2026b\u2026c\"; }";
            const string fixedSource = "class C { string s = \"a...b...c\"; }";
            await CSharpCodeFixVerifier<EllipsisAnalyzer, TypographyCharacterFixProvider, DefaultVerifier>
                .VerifyCodeFixAsync(source, new[] { Warning(EllipsisAnalyzer.DiagnosticId, source, '\u2026'), Warning(EllipsisAnalyzer.DiagnosticId, source, '\u2026', 1) }, fixedSource);
        }

        [TestMethod]
        public async Task TwoQuotesInOneStringStayInsideTheLiteral()
        {
            const string source = "class C { string s = \"\u201Cx\u201D\"; }";
            const string fixedSource = "class C { string s = \"\\\"x\\\"\"; }";
            await CSharpCodeFixVerifier<SmartQuotesAnalyzer, TypographyCharacterFixProvider, DefaultVerifier>
                .VerifyCodeFixAsync(source, new[] { Warning(SmartQuotesAnalyzer.DiagnosticId, source, '\u201C'), Warning(SmartQuotesAnalyzer.DiagnosticId, source, '\u201D') }, fixedSource);
        }

        [TestMethod]
        public async Task QuoteInUtf8StringIsEscaped()
        {
            await VerifyUtf8Async<SmartQuotesAnalyzer>(
                SmartQuotesAnalyzer.DiagnosticId,
                "class C { void M() { var s = \"x\u201Cy\"u8; } }",
                "class C { void M() { var s = \"x\\\"y\"u8; } }",
                '\u201C');
        }

        [TestMethod]
        public async Task TwoQuotesInOneUtf8StringStayInsideTheLiteral()
        {
            const string source = "class C { void M() { var s = \"\u201Cx\u201D\"u8; } }";
            const string fixedSource = "class C { void M() { var s = \"\\\"x\\\"\"u8; } }";
            await VerifyUtf8Async<SmartQuotesAnalyzer>(
                SmartQuotesAnalyzer.DiagnosticId,
                source,
                fixedSource,
                '\u201C',
                '\u201D');
        }

        [TestMethod]
        public async Task EmDashInUtf8StringFixesToHyphen()
        {
            await VerifyUtf8Async<EmDashAnalyzer>(
                EmDashAnalyzer.DiagnosticId,
                "class C { void M() { var s = \"a\u2014b\"u8; } }",
                "class C { void M() { var s = \"a-b\"u8; } }",
                '\u2014');
        }

        [TestMethod]
        public async Task ApostropheInUtf8StringNeedsNoBackslash()
        {
            await VerifyUtf8Async<ApostropheAnalyzer>(
                ApostropheAnalyzer.DiagnosticId,
                "class C { void M() { var s = \"x\u2019y\"u8; } }",
                "class C { void M() { var s = \"x'y\"u8; } }",
                '\u2019');
        }

        [TestMethod]
        public async Task QuoteInVerbatimUtf8StringIsDoubled()
        {
            await VerifyUtf8Async<SmartQuotesAnalyzer>(
                SmartQuotesAnalyzer.DiagnosticId,
                "class C { void M() { var s = @\"x\u201Cy\"u8; } }",
                "class C { void M() { var s = @\"x\"\"y\"u8; } }",
                '\u201C');
        }

        [TestMethod]
        public async Task Utf8RawStringIsNotOfferedAFix()
        {
            await AssertUtf8RawNotOfferedAsync("class C { var s = \"\"\"x\u201Cy\"\"\"u8; }", SyntaxKind.Utf8SingleLineRawStringLiteralToken);
            await AssertUtf8RawNotOfferedAsync("class C { var s = \"\"\"\nx\u201Cy\n\"\"\"u8; }", SyntaxKind.Utf8MultiLineRawStringLiteralToken);
        }

        [TestMethod]
        public async Task RawStringIsNotOfferedAFix()
        {
            const string source = "class C { string s = \"\"\"\nx\u201Cy\n\"\"\"; }";
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
            SourceText text = tree.GetText();
            TextSpan span = new TextSpan(source.IndexOf('\u201C'), 1);
            Assert.AreSame(text, TypographyAsciiReplacement.Apply(text, span, tree.GetRoot()));

            using (AdhocWorkspace workspace = new AdhocWorkspace())
            {
                Project project = workspace.AddProject("p", LanguageNames.CSharp);
                Document document = workspace.AddDocument(project.Id, "A.cs", text);
                Location location = Location.Create(tree, span);
                Diagnostic diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(SmartQuotesAnalyzer.DiagnosticId, "t", "m", "Typography", DiagnosticSeverity.Warning, true),
                    location);
                var actions = new List<CodeAction>();
                var context = new CodeFixContext(
                    document,
                    diagnostic,
                    (action, _) => actions.Add(action),
                    CancellationToken.None);
                await new TypographyCharacterFixProvider().RegisterCodeFixesAsync(context);
                Assert.AreEqual(0, actions.Count);
            }
        }

        [TestMethod]
        public async Task QuoteInVerbatimInterpolationHoleIsEscaped()
        {
            await VerifyAsync<SmartQuotesAnalyzer>(
                SmartQuotesAnalyzer.DiagnosticId,
                "class C { string s = $@\"pre { \"x\u201Cy\" } post\"; }",
                "class C { string s = $@\"pre { \"x\\\"y\" } post\"; }",
                '\u201C');
        }

        [TestMethod]
        public async Task QuoteInRawInterpolationHoleIsEscaped()
        {
            await VerifyAsync<SmartQuotesAnalyzer>(
                SmartQuotesAnalyzer.DiagnosticId,
                "class C { string s = $\"\"\"pre { \"x\u201Cy\" } post\"\"\"; }",
                "class C { string s = $\"\"\"pre { \"x\\\"y\" } post\"\"\"; }",
                '\u201C');
        }

        [TestMethod]
        public async Task VerbatimStringInsideInterpolationHoleDoublesQuote()
        {
            await VerifyAsync<SmartQuotesAnalyzer>(
                SmartQuotesAnalyzer.DiagnosticId,
                "class C { string s = $@\"pre { @\"x\u201Cy\" } post\"; }",
                "class C { string s = $@\"pre { @\"x\"\"y\" } post\"; }",
                '\u201C');
        }

        [TestMethod]
        public async Task VerbatimStringInsideRawInterpolationHoleDoublesQuote()
        {
            await VerifyAsync<SmartQuotesAnalyzer>(
                SmartQuotesAnalyzer.DiagnosticId,
                "class C { string s = $\"\"\"pre { @\"x\u201Cy\" } post\"\"\"; }",
                "class C { string s = $\"\"\"pre { @\"x\"\"y\" } post\"\"\"; }",
                '\u201C');
        }

        [TestMethod]
        public async Task TwoQuotesInOneInterpolationHoleStayInsideTheInnerLiteral()
        {
            const string source = "class C { string s = $@\"pre { \"\u201Cx\u201D\" } post\"; }";
            const string fixedSource = "class C { string s = $@\"pre { \"\\\"x\\\"\" } post\"; }";
            await CSharpCodeFixVerifier<SmartQuotesAnalyzer, TypographyCharacterFixProvider, DefaultVerifier>
                .VerifyCodeFixAsync(
                    source,
                    new[]
                    {
                        Warning(SmartQuotesAnalyzer.DiagnosticId, source, '\u201C'),
                        Warning(SmartQuotesAnalyzer.DiagnosticId, source, '\u201D'),
                    },
                    fixedSource);
        }

        [TestMethod]
        public async Task RawInterpolatedTextIsNotOfferedAFix()
        {
            const string source = "class C { string s = $\"\"\"x\u201Cy\"\"\"; }";
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
            SourceText text = tree.GetText();
            TextSpan span = new TextSpan(source.IndexOf('\u201C'), 1);
            Assert.AreSame(text, TypographyAsciiReplacement.Apply(text, span, tree.GetRoot()));

            using (AdhocWorkspace workspace = new AdhocWorkspace())
            {
                Project project = workspace.AddProject("p", LanguageNames.CSharp);
                Document document = workspace.AddDocument(project.Id, "A.cs", text);
                Location location = Location.Create(tree, span);
                Diagnostic diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(SmartQuotesAnalyzer.DiagnosticId, "t", "m", "Typography", DiagnosticSeverity.Warning, true),
                    location);
                var actions = new List<CodeAction>();
                var context = new CodeFixContext(
                    document,
                    diagnostic,
                    (action, _) => actions.Add(action),
                    CancellationToken.None);
                await new TypographyCharacterFixProvider().RegisterCodeFixesAsync(context);
                Assert.AreEqual(0, actions.Count);
            }
        }

        [TestMethod]
        public void RawTextStaysWhileHoleQuoteIsEscaped()
        {
            const string source = "class C { string s = $\"\"\"x\u201Cy { \"\u201C\" } z\"\"\"; }";
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
            SourceText text = tree.GetText();
            int rawIndex = source.IndexOf('\u201C');
            int holeIndex = source.IndexOf('\u201C', rawIndex + 1);
            Assert.AreSame(text, TypographyAsciiReplacement.Apply(text, new TextSpan(rawIndex, 1), tree.GetRoot()));

            SourceText updated = TypographyAsciiReplacement.Apply(text, new TextSpan(holeIndex, 1), tree.GetRoot());
            Assert.AreEqual("class C { string s = $\"\"\"x\u201Cy { \"\\\"\" } z\"\"\"; }", updated.ToString());
        }

        [TestMethod]
        public void ReplacementCoversEveryFlaggedCharacterAndBadSpan()
        {
            SourceText plain = SourceText.From("ab");
            Assert.AreSame(plain, TypographyAsciiReplacement.Apply(plain, new TextSpan(0, 0), null));
            Assert.AreSame(plain, TypographyAsciiReplacement.Apply(plain, new TextSpan(0, 2), null));
            Assert.AreSame(plain, TypographyAsciiReplacement.Apply(plain, new TextSpan(2, 1), null));
            Assert.AreSame(plain, TypographyAsciiReplacement.Apply(plain, new TextSpan(0, 1), null));
            Assert.AreEqual("-", Replaced("a\u2013b", null));
            Assert.AreEqual("-", Replaced("a\u2014b", null));
            Assert.AreEqual("-", Replaced("a\u2212b", null));
            Assert.AreEqual("\"", Replaced("a\u201Cb", null));
            Assert.AreEqual("\"", Replaced("a\u201Db", null));
            Assert.AreEqual("\"", Replaced("a\u201Eb", null));
            Assert.AreEqual("\"", Replaced("a\u201Fb", null));
            Assert.AreEqual("\"", Replaced("a\u00ABb", null));
            Assert.AreEqual("\"", Replaced("a\u00BBb", null));
            Assert.AreEqual("'", Replaced("a\u2018b", null));
            Assert.AreEqual("'", Replaced("a\u201Ab", null));
            Assert.AreEqual("'", Replaced("a\u201Bb", null));
            Assert.AreEqual("'", Replaced("a\u2019b", null));
            Assert.AreEqual("...", Replaced("a\u2026b", null));
            Assert.AreEqual(" ", Replaced("a\u00A0b", null));
            Assert.AreEqual(" ", Replaced("a\u202Fb", null));
            Assert.IsNull(TypographyAsciiReplacement.ForCharacter('a'));
            Assert.AreEqual("Use ASCII hyphen-minus", TypographyAsciiReplacement.Title(EmDashAnalyzer.DiagnosticId));
            Assert.AreEqual("Use an ASCII quote or apostrophe", TypographyAsciiReplacement.Title(SmartQuotesAnalyzer.DiagnosticId));
            Assert.AreEqual("Use ASCII apostrophe", TypographyAsciiReplacement.Title(ApostropheAnalyzer.DiagnosticId));
            Assert.AreEqual("Use three ASCII periods", TypographyAsciiReplacement.Title(EllipsisAnalyzer.DiagnosticId));
            Assert.AreEqual("Use ASCII hyphen-minus", TypographyAsciiReplacement.Title(MinusAnalyzer.DiagnosticId));
            Assert.AreEqual("Use ASCII space", TypographyAsciiReplacement.Title(NbspAnalyzer.DiagnosticId));
            Assert.AreEqual("Use the ASCII character", TypographyAsciiReplacement.Title(null));
            Assert.AreEqual("Use the ASCII character", TypographyAsciiReplacement.Title("other"));
            Assert.AreSame(plain, TypographyAsciiReplacement.ApplyAll(plain, null, new TextSpan[0]));

            const string interpolated = "class C { string s = $\"x\u201Cy\"; string t = $@\"x\u201Dy\"; }";
            SyntaxNode interpolatedRoot = CSharpSyntaxTree.ParseText(interpolated).GetRoot();
            SourceText interpolatedText = SourceText.From(interpolated);
            Assert.AreEqual("\\\"", Piece(interpolatedText, interpolatedRoot, '\u201C'));
            Assert.AreEqual("\"\"", Piece(interpolatedText, interpolatedRoot, '\u201D'));

            const string documentation = "class C { /// x\u2014y\n void M() {} }";
            SyntaxNode documentationRoot = CSharpSyntaxTree.ParseText(documentation).GetRoot();
            Assert.AreEqual("-", Piece(SourceText.From(documentation), documentationRoot, '\u2014'));

            const string singleLineRaw = "class C { string s = \"\"\"x\u201Cy\"\"\"; }";
            SyntaxTree singleLineRawTree = CSharpSyntaxTree.ParseText(singleLineRaw);
            SourceText singleLineRawText = singleLineRawTree.GetText();
            TextSpan singleLineRawSpan = new TextSpan(singleLineRaw.IndexOf('\u201C'), 1);
            Assert.AreSame(singleLineRawText, TypographyAsciiReplacement.Apply(singleLineRawText, singleLineRawSpan, singleLineRawTree.GetRoot()));

            var scopes = new List<FixAllScope>();
            foreach (FixAllScope scope in new TypographyCharacterFixProvider().GetFixAllProvider().GetSupportedFixAllScopes())
            {
                scopes.Add(scope);
            }

            CollectionAssert.AreEquivalent(
                new[] { FixAllScope.Document, FixAllScope.Project, FixAllScope.Solution },
                scopes);
        }

        private sealed class Utf8FixTest<TAnalyzer> : CSharpCodeFixTest<TAnalyzer, TypographyCharacterFixProvider, DefaultVerifier>
            where TAnalyzer : DiagnosticAnalyzer, new()
        {
            protected override ParseOptions CreateParseOptions()
            {
                return new CSharpParseOptions(LanguageVersion.CSharp13, DocumentationMode.Diagnose);
            }
        }

        private static async Task VerifyUtf8Async<TAnalyzer>(
            string diagnosticId,
            string source,
            string fixedSource,
            char flagged,
            char? second = null)
            where TAnalyzer : DiagnosticAnalyzer, new()
        {
            var test = new Utf8FixTest<TAnalyzer>
            {
                TestCode = source,
                FixedCode = fixedSource,
            };

            test.ExpectedDiagnostics.Add(Warning(diagnosticId, source, flagged));
            if (second.HasValue)
            {
                test.ExpectedDiagnostics.Add(Warning(diagnosticId, source, second.Value));
            }

            await test.RunAsync(CancellationToken.None);
        }

        private static async Task AssertUtf8RawNotOfferedAsync(string source, SyntaxKind kind)
        {
            CSharpParseOptions options = new CSharpParseOptions(LanguageVersion.CSharp13);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source, options);
            SourceText text = tree.GetText();
            TextSpan span = new TextSpan(source.IndexOf('\u201C'), 1);
            SyntaxToken token = tree.GetRoot().FindToken(span.Start);
            Assert.IsTrue(token.IsKind(kind));
            Assert.AreSame(text, TypographyAsciiReplacement.Apply(text, span, tree.GetRoot()));

            using (AdhocWorkspace workspace = new AdhocWorkspace())
            {
                Project project = workspace.AddProject("p", LanguageNames.CSharp);
                DocumentId documentId = DocumentId.CreateNewId(project.Id);
                Solution solution = project.Solution
                    .WithProjectParseOptions(project.Id, options)
                    .AddDocument(documentId, "A.cs", text);
                Assert.IsTrue(workspace.TryApplyChanges(solution));
                Document document = workspace.CurrentSolution.GetDocument(documentId)!;
                SyntaxNode documentRoot = (await document.GetSyntaxRootAsync())!;
                Assert.IsTrue(documentRoot.FindToken(span.Start).IsKind(kind));
                Location location = Location.Create(tree, span);
                Diagnostic diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(SmartQuotesAnalyzer.DiagnosticId, "t", "m", "Typography", DiagnosticSeverity.Warning, true),
                    location);
                var actions = new List<CodeAction>();
                var context = new CodeFixContext(
                    document,
                    diagnostic,
                    (action, _) => actions.Add(action),
                    CancellationToken.None);
                await new TypographyCharacterFixProvider().RegisterCodeFixesAsync(context);
                Assert.AreEqual(0, actions.Count);
            }
        }

        private static string Replaced(string text, SyntaxNode? root)
        {
            SourceText source = SourceText.From(text);
            SourceText updated = TypographyAsciiReplacement.Apply(source, new TextSpan(1, 1), root);
            return updated.ToString().Substring(1, updated.Length - 2);
        }

        private static string Piece(SourceText text, SyntaxNode root, char character)
        {
            int index = text.ToString().IndexOf(character);
            SourceText updated = TypographyAsciiReplacement.Apply(text, new TextSpan(index, 1), root);
            int length = updated.Length - text.Length + 1;
            return updated.ToString().Substring(index, length);
        }

        private static async Task VerifyAsync<TAnalyzer>(string diagnosticId, string source, string fixedSource, char flagged)
            where TAnalyzer : DiagnosticAnalyzer, new()
        {
            DiagnosticResult expected = Warning(diagnosticId, source, flagged);
            await CSharpCodeFixVerifier<TAnalyzer, TypographyCharacterFixProvider, DefaultVerifier>
                .VerifyCodeFixAsync(source, expected, fixedSource);
        }

        private static DiagnosticResult Warning(string diagnosticId, string source, char flagged, int occurrence)
        {
            int index = -1;
            for (int seen = 0; seen <= occurrence; seen++)
            {
                index = source.IndexOf(flagged, index + 1);
            }

            int column = index + 1;
            return DiagnosticResult.CompilerWarning(diagnosticId).WithSpan(1, column, 1, column + 1);
        }

        private static DiagnosticResult Warning(string diagnosticId, string source, char flagged)
        {
            return Warning(diagnosticId, source, flagged, 0);
        }
    }
}
