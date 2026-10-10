using Coree.Analyzers.CodeClarity;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Coree.Analyzers.CodeClarity.Tests
{
    [TestClass]
    public class TypeNamespaceAnalyzerTests
    {
        [TestMethod]
        public async Task GlobalTypesReport()
        {
            const string test = @"
namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }

class {|#0:Terrain|}
{
    class Inner { }
    enum InnerKind { A }
    delegate void InnerHandler();
}

struct {|#1:Value|} { }
interface {|#2:ITerrain|} { }
enum {|#3:Kind|} { A }
delegate void {|#4:Handler|}();
record {|#5:Profile|}(float a);
record struct {|#6:Point|}(float x, float y);
file class {|#7:Helper|} { }
partial class {|#8:Split|} { }
partial class {|#9:Split|} { }
class {|#10:Seeded|}(int seed) { }
";
            DiagnosticResult[] expected = new DiagnosticResult[11];
            for (int index = 0; index < expected.Length; index++)
            {
                expected[index] = DiagnosticResult
                    .CompilerWarning(TypeNamespaceAnalyzer.DiagnosticId)
                    .WithLocation(index);
            }

            await CSharpAnalyzerVerifier<TypeNamespaceAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task BlockNamespacesStayQuiet()
        {
            const string test = @"
namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }

namespace Game
{
    class Terrain
    {
        class Inner { }
        struct NestedValue { }
        interface INested { }
        enum NestedKind { A }
        delegate void NestedHandler();
        record NestedProfile(float a);
    }

    struct Value { }
    interface ITerrain { }
    enum Kind { A }
    delegate void Handler();
    record Profile(float a);
    record struct Point(float x, float y);
    file class Helper { }
    partial class Split { }
    partial class Split { }
}

namespace Outer.Inner
{
    class Terrain { }
}
";
            await CSharpAnalyzerVerifier<TypeNamespaceAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task FileScopedNamespaceStaysQuiet()
        {
            const string test = @"
namespace Game;

class Terrain
{
    class Inner { }
}

struct Value { }
interface ITerrain { }
enum Kind { A }
delegate void Handler();
file class Helper { }
";
            await CSharpAnalyzerVerifier<TypeNamespaceAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TypeAfterTopLevelStatementsReports()
        {
            const string source = "System.Console.WriteLine(1);\nclass {|#0:After|} { }";
            var expected = DiagnosticResult
                .CompilerWarning(TypeNamespaceAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithMessage("Put this type in a namespace. Use a block, as in namespace Game { class Terrain {} }, or a file-scoped namespace, as in namespace Game; class Terrain {}.");
            await VerifyAsync(source, null, OutputKind.ConsoleApplication, expected);
        }

        [TestMethod]
        public async Task NamespacedTypeAfterTopLevelStatementsStaysQuiet()
        {
            const string source = "System.Console.WriteLine(1);\nnamespace Game { class After { } }";
            await VerifyAsync(source, null, OutputKind.ConsoleApplication);
        }

        [TestMethod]
        public async Task SeverityErrorReportsError()
        {
            const string source = "class {|#0:C|} { }";
            var expected = DiagnosticResult
                .CompilerError(TypeNamespaceAnalyzer.DiagnosticId)
                .WithLocation(0);
            await VerifyAsync(source, "error", null, expected);
        }

        [TestMethod]
        public async Task SeverityMessageReportsInfo()
        {
            const string source = "class {|#0:C|} { }";
            var expected = new DiagnosticResult(TypeNamespaceAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
                .WithLocation(0);
            await VerifyAsync(source, "message", null, expected);
        }

        [TestMethod]
        public async Task SeverityOffReportsNothing()
        {
            const string source = "class C { }";
            await VerifyAsync(source, "off", null);
        }

        private static async Task VerifyAsync(string source, string? severity, OutputKind? outputKind, params DiagnosticResult[] expected)
        {
            var test = new CSharpAnalyzerTest<TypeNamespaceAnalyzer, DefaultVerifier>
            {
                TestCode = source,
            };
            test.ExpectedDiagnostics.AddRange(expected);
            if (outputKind.HasValue)
            {
                test.TestState.OutputKind = outputKind.Value;
            }

            if (severity != null)
            {
                string config = "is_global = true\n"
                    + "build_property." + TypeNamespaceAnalyzer.SeverityPropertyName + " = " + severity + "\n";
                test.TestState.AnalyzerConfigFiles.Add(("/.globalconfig", config));
            }

            await test.RunAsync();
        }
    }
}
