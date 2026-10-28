using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Coree.Analyzers.CodeClarity;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Coree.Analyzers.CodeClarity.Tests
{
    [TestClass]
    public class ParameterListLayoutFixTests
    {
        [TestMethod]
        public async Task LongPackedMethodGainsOneParameterPerLine()
        {
            string source = Join(
                "class C",
                "{",
                "    public void M(",
                "        int parameterName01, int parameterName02,",
                "        int parameterName03,",
                "        int parameterName04,",
                "        int parameterName05,",
                "        int parameterName06)",
                "    {",
                "    }",
                "}");
            string fixedSource = Join(
                "class C",
                "{",
                "    public void M(",
                "        int parameterName01,",
                "        int parameterName02,",
                "        int parameterName03,",
                "        int parameterName04,",
                "        int parameterName05,",
                "        int parameterName06",
                "    )",
                "    {",
                "    }",
                "}");
            AssertDoesNotFit(source);
            await VerifyAsync(source, fixedSource);
        }

        [TestMethod]
        public async Task ConstructorLocalFunctionAndRecordExpand()
        {
            string source = Join(
                "namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }",
                "class C",
                "{",
                "    public C(",
                "        int parameterName01, int parameterName02,",
                "        int parameterName03,",
                "        int parameterName04,",
                "        int parameterName05,",
                "        int parameterName06)",
                "    {",
                "    }",
                "    void Outer()",
                "    {",
                "        void Inner(",
                "            int parameterName01, int parameterName02,",
                "            int parameterName03,",
                "            int parameterName04,",
                "            int parameterName05,",
                "            int parameterName06)",
                "        {",
                "        }",
                "    }",
                "}",
                "record R(",
                "    int parameterName01, int parameterName02,",
                "    int parameterName03,",
                "    int parameterName04,",
                "    int parameterName05,",
                "    int parameterName06);");
            string fixedSource = Join(
                "namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }",
                "class C",
                "{",
                "    public C(",
                "        int parameterName01,",
                "        int parameterName02,",
                "        int parameterName03,",
                "        int parameterName04,",
                "        int parameterName05,",
                "        int parameterName06",
                "    )",
                "    {",
                "    }",
                "    void Outer()",
                "    {",
                "        void Inner(",
                "            int parameterName01,",
                "            int parameterName02,",
                "            int parameterName03,",
                "            int parameterName04,",
                "            int parameterName05,",
                "            int parameterName06",
                "        )",
                "        {",
                "        }",
                "    }",
                "}",
                "record R(",
                "    int parameterName01,",
                "    int parameterName02,",
                "    int parameterName03,",
                "    int parameterName04,",
                "    int parameterName05,",
                "    int parameterName06",
                ");");
            await VerifyAsync(source, fixedSource);
        }

        [TestMethod]
        public async Task LowerMaximumExpandsAShortList()
        {
            const string source = "class C { void M(\n    int a, int b) { } }";
            const string fixedSource = "class C { void M(\n    int a,\n    int b\n) { } }";
            var test = new CSharpCodeFixTest<ParameterListLayoutAnalyzer, ParameterListLayoutFixProvider, DefaultVerifier>
            {
                TestCode = source,
                FixedCode = fixedSource,
            };
            test.ExpectedDiagnostics.Add(ListDiagnostic(source, 0));
            test.TestState.AnalyzerConfigFiles.Add((
                "/.globalconfig",
                "is_global = true\nbuild_property." + ParameterListFitAnalyzer.MaximumPropertyName + " = 10\n"));
            await test.RunAsync();
        }

        [TestMethod]
        public async Task BlockCommentInsideAParameterStays()
        {
            string source = Join(
                "class C",
                "{",
                "    void M(",
                "        int /* amplitude */ parameterName01, int parameterName02,",
                "        int parameterName03,",
                "        int parameterName04,",
                "        int parameterName05,",
                "        int parameterName06)",
                "    {",
                "    }",
                "}");
            string fixedSource = Join(
                "class C",
                "{",
                "    void M(",
                "        int /* amplitude */ parameterName01,",
                "        int parameterName02,",
                "        int parameterName03,",
                "        int parameterName04,",
                "        int parameterName05,",
                "        int parameterName06",
                "    )",
                "    {",
                "    }",
                "}");
            await VerifyAsync(source, fixedSource);
        }

        [TestMethod]
        public async Task LiteralLineBreakStaysInsideTheParameter()
        {
            string source = Join(
                "class C",
                "{",
                "    void M(",
                "        int parameterName01,",
                "        string value = @\"select",
                "        id\")",
                "    {",
                "    }",
                "}");
            string fixedSource = Join(
                "class C",
                "{",
                "    void M(",
                "        int parameterName01,",
                "        string value = @\"select",
                "        id\"",
                "    )",
                "    {",
                "    }",
                "}");
            await VerifyAsync(source, fixedSource);
        }

        [TestMethod]
        public async Task DocumentationMarkInsideAStringStays()
        {
            string source = Join(
                "class C",
                "{",
                "    void M(",
                "        int parameterName01, string name = \"/// width\",",
                "        int parameterName02,",
                "        int parameterName03,",
                "        int parameterName04,",
                "        int parameterName05,",
                "        int parameterName06)",
                "    {",
                "    }",
                "}");
            string updated = Applied(source, ParameterListFitAnalyzer.DefaultMaximum);
            StringAssert.Contains(updated, "\"/// width\"");
            Assert.IsFalse(ParameterListLayout.IsDisallowed(OnlyList(updated)));
        }

        [TestMethod]
        public async Task ShortListThatFitsRegistersNoAction()
        {
            const string source = "class C { void M(\n    int a, int b) { } }";
            AssertUnchanged(source);
            Assert.AreEqual(0, await ActionCountAsync(source));
        }

        [TestMethod]
        public async Task ShortOperatorThatFitsRegistersNoExpandAction()
        {
            const string source = "struct Meter { public static bool operator ==(\n    Meter left, Meter right) { return true; } }";
            AssertUnchanged(source);
            Assert.AreEqual(0, await ActionCountAsync(source));
        }

        [TestMethod]
        public void CommentBetweenParametersIsNotOfferedAFix()
        {
            AssertUnchanged("class C { void M(\n    int a, // width\n    int b) { } }");
        }

        [TestMethod]
        public void DocumentationCommentBetweenParametersIsNotOfferedAFix()
        {
            AssertUnchanged("class C { void M(\n    /// <param name=\"a\">A.</param>\n    int a, int b) { } }");
        }

        [TestMethod]
        public void DirectiveBetweenParametersIsNotOfferedAFix()
        {
            AssertUnchanged("class C { void M(\n    int a, int b,\n#if true\n    int c) { } }");
        }

        [TestMethod]
        public void LegalListIsNotRewritten()
        {
            AssertUnchanged(Join(
                "class C",
                "{",
                "    void M(",
                "        int parameterName01,",
                "        int parameterName02,",
                "        int parameterName03,",
                "        int parameterName04,",
                "        int parameterName05,",
                "        int parameterName06",
                "    )",
                "    {",
                "    }",
                "}"));
        }

        [TestMethod]
        public void SpanOnAParameterStillFindsTheList()
        {
            string source = Join(
                "class C",
                "{",
                "    void M(",
                "        int parameterName01, int parameterName02,",
                "        int parameterName03,",
                "        int parameterName04,",
                "        int parameterName05,",
                "        int parameterName06)",
                "    {",
                "    }",
                "}");
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
            SourceText text = tree.GetText();
            ParameterListSyntax list = OnlyList(source);
            string fromList = ParameterListLayoutFix.Apply(text, tree.GetRoot(), list.Span, ParameterListFitAnalyzer.DefaultMaximum).ToString();
            string fromParameter = ParameterListLayoutFix.Apply(
                text,
                tree.GetRoot(),
                list.Parameters[0].Span,
                ParameterListFitAnalyzer.DefaultMaximum).ToString();
            Assert.AreEqual(fromList, fromParameter);
        }

        [TestMethod]
        public void SpanOutsideAParameterListStaysUnchanged()
        {
            const string source = "class C { void M(int a, int b) { } }";
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
            TextSpan name = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single().Identifier.Span;
            Assert.AreSame(
                tree.GetText(),
                ParameterListLayoutFix.Apply(tree.GetText(), tree.GetRoot(), name, ParameterListFitAnalyzer.DefaultMaximum));
        }

        [TestMethod]
        public void MissingCloseParenIsNotRewritten()
        {
            AssertMissingTokenIsUnchanged(SyntaxKind.CloseParenToken);
        }

        [TestMethod]
        public void MissingOpenParenIsNotRewritten()
        {
            AssertMissingTokenIsUnchanged(SyntaxKind.OpenParenToken);
        }

        [TestMethod]
        [DataRow("\n")]
        [DataRow("\r\n")]
        [DataRow("\r")]
        [DataRow("\u0085")]
        [DataRow("\u2028")]
        [DataRow("\u2029")]
        public void ExpandedListUsesTheLineEndingBeforeTheParen(string lineEnding)
        {
            string source = "class C" + lineEnding + "{    void M(" + lineEnding
                + "    int parameterName01, int parameterName02," + lineEnding
                + "    int parameterName03," + lineEnding
                + "    int parameterName04," + lineEnding
                + "    int parameterName05," + lineEnding
                + "    int parameterName06) { } }";
            string updated = Applied(source, ParameterListFitAnalyzer.DefaultMaximum);
            StringAssert.Contains(updated, "int parameterName01," + lineEnding + "    int parameterName02");
        }

        [TestMethod]
        public void FirstLineParenUsesLineFeedWhenNoEarlierBreakExists()
        {
            string source = Join(
                "void M(",
                "    int parameterName01, int parameterName02,",
                "    int parameterName03,",
                "    int parameterName04,",
                "    int parameterName05,",
                "    int parameterName06)");
            string updated = Applied(source, ParameterListFitAnalyzer.DefaultMaximum);
            StringAssert.Contains(updated, "void M(\n    int parameterName01,");
        }

        [TestMethod]
        public void LineFeedAtTheStartOfTheFileStaysALineFeed()
        {
            string source = "\n" + Join(
                "void M(",
                "    int parameterName01, int parameterName02,",
                "    int parameterName03,",
                "    int parameterName04,",
                "    int parameterName05,",
                "    int parameterName06)");
            string updated = Applied(source, ParameterListFitAnalyzer.DefaultMaximum);
            StringAssert.Contains(updated, "\nvoid M(\n    int parameterName01,");
        }

        [TestMethod]
        public void TabIndentAddsOneTab()
        {
            const string source = "\tvoid M(\n\t\tint parameterName01, int parameterName02,\n\t\tint parameterName03,\n\t\tint parameterName04,\n\t\tint parameterName05,\n\t\tint parameterName06)";
            string updated = Applied(source, ParameterListFitAnalyzer.DefaultMaximum);
            StringAssert.Contains(updated, "\tvoid M(\n\t\tint parameterName01,\n\t\tint parameterName02,");
            StringAssert.Contains(updated, "\n\t)");
        }

        [TestMethod]
        public void EmptyLineHasNoIndent()
        {
            Assert.AreEqual(string.Empty, ParameterListLayoutFix.LineIndent(SourceText.From("\n"), 0));
        }

        [TestMethod]
        public void MixedIndentAddsFourSpaces()
        {
            const string source = " \tvoid M(\n \t\tint parameterName01, int parameterName02,\n \t\tint parameterName03,\n \t\tint parameterName04,\n \t\tint parameterName05,\n \t\tint parameterName06)";
            string updated = Applied(source, ParameterListFitAnalyzer.DefaultMaximum);
            StringAssert.Contains(updated, " \t    int parameterName01,");
        }

        [TestMethod]
        public void ProviderListsTheLayoutDiagnosticAndFixAll()
        {
            var provider = new ParameterListLayoutFixProvider();
            CollectionAssert.AreEqual(
                new[] { ParameterListLayoutAnalyzer.DiagnosticId },
                provider.FixableDiagnosticIds.ToArray());
            FixAllProvider fixAll = provider.GetFixAllProvider();
            Assert.IsNotNull(fixAll);
            FixAllScope[] scopes = fixAll.GetSupportedFixAllScopes().ToArray();
            CollectionAssert.Contains(scopes, FixAllScope.Document);
            CollectionAssert.Contains(scopes, FixAllScope.Project);
            CollectionAssert.Contains(scopes, FixAllScope.Solution);
        }

        private static async Task VerifyAsync(string source, string fixedSource)
        {
            await CSharpCodeFixVerifier<ParameterListLayoutAnalyzer, ParameterListLayoutFixProvider, DefaultVerifier>
                .VerifyCodeFixAsync(source, new[] { ListDiagnostic(source, 0) }.Concat(ExtraLists(source)).ToArray(), fixedSource);
        }

        private static IEnumerable<DiagnosticResult> ExtraLists(string source)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
            int index = 0;
            foreach (ParameterListSyntax list in tree.GetRoot().DescendantNodes().OfType<ParameterListSyntax>())
            {
                if (index > 0 && ParameterListLayout.IsDisallowed(list))
                {
                    yield return ListDiagnostic(source, index);
                }

                index++;
            }
        }

        private static DiagnosticResult ListDiagnostic(string source, int occurrence)
        {
            ParameterListSyntax list = Lists(source).ElementAt(occurrence);
            FileLinePositionSpan span = list.GetLocation().GetLineSpan();
            return DiagnosticResult.CompilerWarning(ParameterListLayoutAnalyzer.DiagnosticId)
                .WithSpan(
                    span.StartLinePosition.Line + 1,
                    span.StartLinePosition.Character + 1,
                    span.EndLinePosition.Line + 1,
                    span.EndLinePosition.Character + 1);
        }

        private static void AssertDoesNotFit(string source)
        {
            ParameterListSyntax list = OnlyList(source);
            Assert.IsTrue(ParameterListFit.JoinedLength(list.Parameters) > ParameterListFitAnalyzer.DefaultMaximum);
            Assert.IsFalse(ParameterListFit.UsesBreakButFits(list, ParameterListFitAnalyzer.DefaultMaximum));
        }

        private static string Applied(string source, int maximum)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
            return ParameterListLayoutFix.Apply(tree.GetText(), tree.GetRoot(), OnlyList(source).Span, maximum).ToString();
        }

        private static void AssertUnchanged(string source)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
            SourceText text = tree.GetText();
            ParameterListSyntax list = tree.GetRoot().DescendantNodes().OfType<ParameterListSyntax>().Single();
            SourceText updated = ParameterListLayoutFix.Apply(text, tree.GetRoot(), list.Span, ParameterListFitAnalyzer.DefaultMaximum);
            Assert.AreSame(text, updated, updated.ToString());
        }

        private static void AssertMissingTokenIsUnchanged(SyntaxKind kind)
        {
            SyntaxToken open = kind == SyntaxKind.OpenParenToken
                ? SyntaxFactory.MissingToken(SyntaxKind.OpenParenToken)
                : SyntaxFactory.Token(SyntaxKind.OpenParenToken);
            SyntaxToken close = kind == SyntaxKind.CloseParenToken
                ? SyntaxFactory.MissingToken(SyntaxKind.CloseParenToken)
                : SyntaxFactory.Token(SyntaxKind.CloseParenToken);
            ParameterListSyntax list = SyntaxFactory.ParameterList(
                open,
                SyntaxFactory.SeparatedList(new[]
                {
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("a")),
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("b")),
                }),
                close);
            SourceText text = SourceText.From("void M(int a, int b)");
            Assert.AreSame(text, ParameterListLayoutFix.Apply(text, list, list.FullSpan, ParameterListFitAnalyzer.DefaultMaximum));
        }

        private static ParameterListSyntax OnlyList(string source)
        {
            return Lists(source).Single();
        }

        private static IEnumerable<ParameterListSyntax> Lists(string source)
        {
            return CSharpSyntaxTree.ParseText(source).GetRoot().DescendantNodes().OfType<ParameterListSyntax>();
        }

        private static string Join(params string[] lines)
        {
            return string.Join("\n", lines);
        }

        private static async Task<int> ActionCountAsync(string source)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
            ParameterListSyntax list = tree.GetRoot().DescendantNodes().OfType<ParameterListSyntax>().Single();
            using (AdhocWorkspace workspace = new AdhocWorkspace())
            {
                Project project = workspace.AddProject("p", LanguageNames.CSharp);
                Document document = workspace.AddDocument(project.Id, "A.cs", tree.GetText());
                Diagnostic diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(
                        ParameterListLayoutAnalyzer.DiagnosticId,
                        "t",
                        "m",
                        "Clarity",
                        DiagnosticSeverity.Warning,
                        true),
                    Location.Create(tree, list.Span));
                var actions = new List<CodeAction>();
                var context = new CodeFixContext(
                    document,
                    diagnostic,
                    (action, _) => actions.Add(action),
                    CancellationToken.None);
                await new ParameterListLayoutFixProvider().RegisterCodeFixesAsync(context);
                return actions.Count;
            }
        }
    }
}
