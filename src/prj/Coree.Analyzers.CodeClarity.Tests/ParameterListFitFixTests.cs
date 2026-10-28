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
    public class ParameterListFitFixTests
    {
        [TestMethod]
        public async Task ShortWrappedListJoinsOntoOneLine()
        {
            const string source = "public class C {\n    public void M{|#0:(\n        float a,\n        float b,\n        float c)|}\n    {\n    }\n}";
            const string fixedSource = "public class C {\n    public void M(float a, float b, float c)\n    {\n    }\n}";
            await VerifyAsync(source, fixedSource);
        }

        [TestMethod]
        public async Task ConstructorLocalFunctionAndPrimaryConstructorJoin()
        {
            const string source = @"
namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }

class Terrain
{
    public Terrain{|#0:(
        int a,
        int b)|}
    {
        void Inner{|#1:(
            int a,
            int b)|}
        {
        }
    }
}

class Primary{|#2:(
    int a,
    int b)|};

public record Profile{|#3:(
    float a,
    float b,
    float c)|};
";
            const string fixedSource = @"
namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }

class Terrain
{
    public Terrain(int a, int b)
    {
        void Inner(int a, int b)
        {
        }
    }
}

class Primary(int a, int b);

public record Profile(float a, float b, float c);
";
            await VerifyAsync(source, fixedSource);
        }

        [TestMethod]
        public async Task FormattingWhitespaceCollapsesAndLiteralWhitespaceStays()
        {
            const string source = "class C {\n    void Padded{|#0:(\n        float                                        a,\n        string value = \"left    right\"\n    )|} { }\n}";
            const string fixedSource = "class C {\n    void Padded(float a, string value = \"left    right\") { }\n}";
            await VerifyAsync(source, fixedSource);
        }

        [TestMethod]
        public async Task BlockCommentInsideAParameterStays()
        {
            const string source = "class C {\n    void M{|#0:(\n        int /* amplitude */ a,\n        int b\n    )|} { }\n}";
            const string fixedSource = "class C {\n    void M(int /* amplitude */ a, int b) { }\n}";
            await VerifyAsync(source, fixedSource);

            const string multiline = "class C {\n    void M{|#0:(\n        int /* amplitude\n        scale */ a,\n        int b\n    )|} { }\n}";
            const string multilineFixed = "class C {\n    void M(int /* amplitude scale */ a, int b) { }\n}";
            await VerifyAsync(multiline, multilineFixed);
        }

        [TestMethod]
        public void AttributeOnItsOwnLineStaysWithTheParameter()
        {
            const string source = "class C { void M(\n    int a,\n    [System.Obsolete]\n    int b) { } }";
            const string expected = "class C { void M(int a, [System.Obsolete] int b) { } }";
            Assert.AreEqual(expected, Applied(source, ParameterListFitAnalyzer.DefaultMaximum));
        }

        [TestMethod]
        public async Task ModifiersAndStringSlashesStayOnTheOneLine()
        {
            const string source = "class C {\n    void M{|#0:(\n        ref int a,\n        string url = \"http://example\"\n    )|} { }\n}";
            const string fixedSource = "class C {\n    void M(ref int a, string url = \"http://example\") { }\n}";
            await VerifyAsync(source, fixedSource);
        }

        [TestMethod]
        public async Task EmptyAndSingleParameterListsJoin()
        {
            const string empty = "class C { void M{|#0:(\n)|} { } }";
            const string emptyFixed = "class C { void M() { } }";
            await VerifyAsync(empty, emptyFixed);

            const string single = "class C { void M{|#0:(\n    int a\n)|} { } }";
            const string singleFixed = "class C { void M(int a) { } }";
            await VerifyAsync(single, singleFixed);
        }

        [TestMethod]
        public async Task TwoListsInOneDocumentEachJoin()
        {
            const string source = "class C {\n    void A{|#0:(\n        int a,\n        int b)|} { }\n    void B{|#1:(\n        int a,\n        int b)|} { }\n}";
            const string fixedSource = "class C {\n    void A(int a, int b) { }\n    void B(int a, int b) { }\n}";
            await VerifyAsync(source, fixedSource);
        }

        [TestMethod]
        public void CommentThatTheMeasurementOmitsIsNotDeleted()
        {
            const string between = "class C {\n    void M(\n        int a,\n        /* note */\n        int b) { }\n}";
            AssertUnchanged(between, ParameterListFitAnalyzer.DefaultMaximum);

            const string empty = "class C {\n    void M(\n        /* note */\n    ) { }\n}";
            AssertUnchanged(empty, ParameterListFitAnalyzer.DefaultMaximum);
        }

        [TestMethod]
        public async Task CommentThatTheMeasurementOmitsRegistersNoFix()
        {
            const string source = "class C {\n    void M(\n        int a,\n        /* note */\n        int b) { }\n}";
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
            TextSpan span = tree.GetRoot().DescendantNodes().OfType<ParameterListSyntax>().Single().Span;
            int actions = await ActionCountAsync(tree, span);
            Assert.AreEqual(0, actions);

            const string marked = "class C {\n    void M{|#0:(\n        int a,\n        /* note */\n        int b)|} { }\n}";
            DiagnosticResult expected = DiagnosticResult
                .CompilerWarning(ParameterListFitAnalyzer.DiagnosticId)
                .WithLocation(0);
            await CSharpAnalyzerVerifier<ParameterListFitAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(marked, expected);
        }

        [TestMethod]
        public void QuietListsAndASpanOutsideTheListStayUnchanged()
        {
            AssertUnchanged("class C { void M(int a, int b) { } }", ParameterListFitAnalyzer.DefaultMaximum);
            AssertUnchanged("class C { void M(\n    int a, // note\n    int b) { } }", ParameterListFitAnalyzer.DefaultMaximum);
            AssertUnchanged("class C { void M(\n    int a,\n#if true\n    int b\n#endif\n) { } }", ParameterListFitAnalyzer.DefaultMaximum);
            AssertUnchanged("class C { void M(\n    int a,\n    int b) { } }", 3);
            Assert.AreEqual(
                "class C { void M(int a, int b) { } }",
                Applied("class C { void M(\n\tint\ta,\n\tint\tb) { } }", ParameterListFitAnalyzer.DefaultMaximum));
            Assert.AreEqual(
                "class C { void M\n    (int a, int b) { } }",
                Applied("class C { void M\n    (\n    int a,\n    int b) { } }", ParameterListFitAnalyzer.DefaultMaximum));

            const string source = "class C { void M(\n    int a,\n    int b) { } }";
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
            SourceText text = tree.GetText();
            TextSpan name = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single().Identifier.Span;
            Assert.AreSame(text, ParameterListJoin.Apply(text, tree.GetRoot(), name, ParameterListFitAnalyzer.DefaultMaximum));
        }

        [TestMethod]
        public void InnerParameterSpanStillJoinsTheWholeList()
        {
            const string source = "class C { void M(\n    int a,\n    int b) { } }";
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
            TextSpan parameter = tree.GetRoot().DescendantNodes().OfType<ParameterSyntax>().First().Span;
            SourceText updated = ParameterListJoin.Apply(
                tree.GetText(),
                tree.GetRoot(),
                parameter,
                ParameterListFitAnalyzer.DefaultMaximum);
            Assert.AreEqual("class C { void M(int a, int b) { } }", updated.ToString());
        }

        [TestMethod]
        public void CarriageReturnLineEndingsOutsideTheListStay()
        {
            const string source = "class C {\r\n    void M(\r\n        int a,\r\n        int b\r\n    ) { }\r\n    int after = 1;\r\n}";
            const string expected = "class C {\r\n    void M(int a, int b) { }\r\n    int after = 1;\r\n}";
            Assert.AreEqual(expected, Applied(source, ParameterListFitAnalyzer.DefaultMaximum));
        }

        [TestMethod]
        public void ProviderListsTheFitDiagnosticAndFixAllScopes()
        {
            var provider = new ParameterListFitFixProvider();
            bool found = false;
            foreach (string id in provider.FixableDiagnosticIds)
            {
                if (id == ParameterListFitAnalyzer.DiagnosticId)
                {
                    found = true;
                }
            }

            Assert.IsTrue(found);
            var scopes = new List<FixAllScope>();
            foreach (FixAllScope scope in provider.GetFixAllProvider().GetSupportedFixAllScopes())
            {
                scopes.Add(scope);
            }

            CollectionAssert.IsSubsetOf(
                new[] { FixAllScope.Document, FixAllScope.Project, FixAllScope.Solution },
                scopes);
        }

        private static async Task VerifyAsync(string source, string fixedSource)
        {
            int diagnostics = 0;
            while (source.Contains("{|#" + diagnostics + ":", System.StringComparison.Ordinal))
            {
                diagnostics++;
            }

            DiagnosticResult[] expected = new DiagnosticResult[diagnostics];
            for (int index = 0; index < diagnostics; index++)
            {
                expected[index] = DiagnosticResult
                    .CompilerWarning(ParameterListFitAnalyzer.DiagnosticId)
                    .WithLocation(index);
            }

            await CSharpCodeFixVerifier<ParameterListFitAnalyzer, ParameterListFitFixProvider, DefaultVerifier>
                .VerifyCodeFixAsync(source, expected, fixedSource);
        }

        private static void AssertUnchanged(string source, int maximum)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
            SourceText text = tree.GetText();
            TextSpan span = tree.GetRoot().DescendantNodes().OfType<ParameterListSyntax>().First().Span;
            Assert.AreSame(text, ParameterListJoin.Apply(text, tree.GetRoot(), span, maximum));
        }

        private static string Applied(string source, int maximum)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
            TextSpan span = tree.GetRoot().DescendantNodes().OfType<ParameterListSyntax>().First().Span;
            return ParameterListJoin.Apply(tree.GetText(), tree.GetRoot(), span, maximum).ToString();
        }

        private static async Task<int> ActionCountAsync(SyntaxTree tree, TextSpan span)
        {
            using (AdhocWorkspace workspace = new AdhocWorkspace())
            {
                Project project = workspace.AddProject("p", LanguageNames.CSharp);
                Document document = workspace.AddDocument(project.Id, "A.cs", tree.GetText());
                Location location = Location.Create(tree, span);
                Diagnostic diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(ParameterListFitAnalyzer.DiagnosticId, "t", "m", "Clarity", DiagnosticSeverity.Warning, true),
                    location);
                var actions = new List<CodeAction>();
                var context = new CodeFixContext(
                    document,
                    diagnostic,
                    (action, _) => actions.Add(action),
                    CancellationToken.None);
                await new ParameterListFitFixProvider().RegisterCodeFixesAsync(context);
                return actions.Count;
            }
        }
    }
}
