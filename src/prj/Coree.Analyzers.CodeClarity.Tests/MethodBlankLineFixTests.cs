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
    public class MethodBlankLineFixTests
    {
        [TestMethod]
        public async Task AdjacentMethodsGainOneBlankLine()
        {
            const string source = "class C {\n    void A() { }\n    void B() { }\n}";
            const string fixedSource = "class C {\n    void A() { }\n\n    void B() { }\n}";
            await VerifyAsync(source, fixedSource, "B");
        }

        [TestMethod]
        public async Task CommentAndAttributeStayBelowTheInsertedLine()
        {
            const string comment = "class C {\n    void A() { }\n    // note\n    void B() { }\n}";
            const string commentFixed = "class C {\n    void A() { }\n\n    // note\n    void B() { }\n}";
            await VerifyAsync(comment, commentFixed, "B");

            const string attribute = "class C {\n    void A() { }\n    [System.Obsolete]\n    void B() { }\n}";
            const string attributeFixed = "class C {\n    void A() { }\n\n    [System.Obsolete]\n    void B() { }\n}";
            await VerifyAsync(attribute, attributeFixed, "B");
        }

        [TestMethod]
        public async Task ChainOfMethodsEachGainsABlankLine()
        {
            const string source = "class C {\n    void A() { }\n    void B() { }\n    void Later() { }\n}";
            const string fixedSource = "class C {\n    void A() { }\n\n    void B() { }\n\n    void Later() { }\n}";
            DiagnosticResult[] expected = new DiagnosticResult[2];
            expected[0] = WarningOn(source, "B");
            expected[1] = WarningOn(source, "Later");
            await CSharpCodeFixVerifier<MethodBlankLineAnalyzer, MethodBlankLineFixProvider, DefaultVerifier>
                .VerifyCodeFixAsync(source, expected, fixedSource);
        }

        [TestMethod]
        public async Task MethodsOnOneLineGainABlankLine()
        {
            const string source = "class C { void A() { } void B() { } }";
            const string fixedSource = "class C { void A() { }\n\nvoid B() { } }";
            await VerifyAsync(source, fixedSource, "B");
        }

        [TestMethod]
        public async Task SameLineCommentIsKept()
        {
            const string source = "class C { void A() { } /* note */ void B() { } }";
            const string fixedSource = "class C { void A() { }\n\n /* note */ void B() { } }";
            await VerifyAsync(source, fixedSource, "B");
        }

        [TestMethod]
        public async Task TrailingLineCommentStaysBelowTheBlankLine()
        {
            const string source = "class C {\n    void A() { } // stays\n    void B() { }\n}";
            const string fixedSource = "class C {\n    void A() { }\n\n // stays\n    void B() { }\n}";
            await VerifyAsync(source, fixedSource, "B");
        }

        [TestMethod]
        [DataRow("\n")]
        [DataRow("\r\n")]
        [DataRow("\r")]
        [DataRow("\u0085")]
        [DataRow("\u2028")]
        [DataRow("\u2029")]
        public void SeparatedMethodsKeepTheFileLineEnding(string lineEnding)
        {
            string source = "class C {" + lineEnding
                + "    void A() { }" + lineEnding
                + "    void B() { }" + lineEnding
                + "}";
            string expected = "class C {" + lineEnding
                + "    void A() { }" + lineEnding
                + lineEnding
                + "    void B() { }" + lineEnding
                + "}";
            Assert.AreEqual(expected, Applied(source, "B"));
        }

        [TestMethod]
        public void LineFeedAtTheStartOfTheFileStaysALineFeed()
        {
            const string source = "\nclass C { void A() { } void B() { } }";
            const string expected = "\nclass C { void A() { }\n\nvoid B() { } }";
            Assert.AreEqual(expected, Applied(source, "B"));
        }

        [TestMethod]
        public void ExistingBlankLineIsNotOfferedAFix()
        {
            const string source = "class C {\n    void A() { }\n\n    void B() { }\n}";
            AssertUnchanged(source, "B");
        }

        [TestMethod]
        public void FirstMethodAndPropertyNeighborStayUnchanged()
        {
            const string source = "class C {\n    void A() { }\n    int P => 1;\n    void B() { }\n}";
            AssertUnchanged(source, "A");
            AssertUnchanged(source, "B");
        }

        [TestMethod]
        public void SpanThatIsNotAMethodStaysUnchanged()
        {
            const string source = "class C {\n    void A() { }\n    void B() { }\n}";
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
            SourceText text = tree.GetText();
            TextSpan span = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single().Identifier.Span;
            Assert.AreSame(text, MethodBlankLineInsertion.Apply(text, tree.GetRoot(), span));
        }

        [TestMethod]
        public void DetachedMethodHasNoInsertion()
        {
            MethodDeclarationSyntax method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                "M");
            TextSpan span;
            string insertion;
            Assert.IsFalse(MethodBlankLineInsertion.TryCreate(method, out span, out insertion));
        }

        [TestMethod]
        public void ProviderListsTheBlankLineDiagnosticAndFixAll()
        {
            var provider = new MethodBlankLineFixProvider();
            bool found = false;
            foreach (string id in provider.FixableDiagnosticIds)
            {
                if (id == MethodBlankLineAnalyzer.DiagnosticId)
                {
                    found = true;
                }
            }

            Assert.IsTrue(found);
            Assert.IsNotNull(provider.GetFixAllProvider());
        }

        [TestMethod]
        public async Task SeparatedMethodsRegisterNoAction()
        {
            const string source = "class C {\n    void A() { }\n\n    void B() { }\n}";
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
            TextSpan span = MethodNamed(tree, "B").Identifier.Span;
            int actions = await ActionCountAsync(tree, span);
            Assert.AreEqual(0, actions);
        }

        private static async Task VerifyAsync(string source, string fixedSource, string methodName)
        {
            DiagnosticResult expected = WarningOn(source, methodName);
            await CSharpCodeFixVerifier<MethodBlankLineAnalyzer, MethodBlankLineFixProvider, DefaultVerifier>
                .VerifyCodeFixAsync(source, expected, fixedSource);
        }

        private static DiagnosticResult WarningOn(string source, string methodName)
        {
            int index = source.IndexOf("void " + methodName);
            int name = index + "void ".Length;
            int line = 1;
            int column = 1;
            for (int position = 0; position < name; position++)
            {
                if (source[position] == '\n')
                {
                    line++;
                    column = 1;
                    continue;
                }

                if (source[position] != '\r')
                {
                    column++;
                }
            }

            return DiagnosticResult.CompilerWarning(MethodBlankLineAnalyzer.DiagnosticId)
                .WithSpan(line, column, line, column + methodName.Length);
        }

        private static string Applied(string source, string methodName)
        {
            return AppliedText(source, methodName).ToString();
        }

        private static void AssertUnchanged(string source, string methodName)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
            SourceText text = tree.GetText();
            TextSpan span = MethodNamed(tree, methodName).Identifier.Span;
            Assert.AreSame(text, MethodBlankLineInsertion.Apply(text, tree.GetRoot(), span));
        }

        private static SourceText AppliedText(string source, string methodName)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
            SourceText text = tree.GetText();
            TextSpan span = MethodNamed(tree, methodName).Identifier.Span;
            return MethodBlankLineInsertion.Apply(text, tree.GetRoot(), span);
        }

        private static MethodDeclarationSyntax MethodNamed(SyntaxTree tree, string methodName)
        {
            foreach (MethodDeclarationSyntax method in tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                if (method.Identifier.ValueText == methodName)
                {
                    return method;
                }
            }

            Assert.Fail("Missing method " + methodName);
            return null!;
        }

        private static async Task<int> ActionCountAsync(SyntaxTree tree, TextSpan span)
        {
            using (AdhocWorkspace workspace = new AdhocWorkspace())
            {
                Project project = workspace.AddProject("p", LanguageNames.CSharp);
                Document document = workspace.AddDocument(project.Id, "A.cs", tree.GetText());
                Location location = Location.Create(tree, span);
                Diagnostic diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(MethodBlankLineAnalyzer.DiagnosticId, "t", "m", "Clarity", DiagnosticSeverity.Warning, true),
                    location);
                var actions = new System.Collections.Generic.List<CodeAction>();
                var context = new CodeFixContext(
                    document,
                    diagnostic,
                    (action, _) => actions.Add(action),
                    CancellationToken.None);
                await new MethodBlankLineFixProvider().RegisterCodeFixesAsync(context);
                return actions.Count;
            }
        }
    }
}
