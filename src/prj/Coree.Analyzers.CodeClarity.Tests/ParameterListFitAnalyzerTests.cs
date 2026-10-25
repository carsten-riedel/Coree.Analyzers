using Coree.Analyzers.CodeClarity;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Coree.Analyzers.CodeClarity.Tests
{
    [TestClass]
    public class ParameterListFitAnalyzerTests
    {
        [TestMethod]
        public async Task OneLineAndLongBreaksStayQuiet()
        {
            const string test = @"
namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }

class Plain
{
    void None() { }
    void One(int a) { }
    void Flat(float a, float b, float c) { }
    void Long(
        float amplitude,
        float frequencyX,
        float frequencyZ,
        float detailScale,
        int seed,
        int octave,
        float secondaryAmplitudeXYZ
    ) { }

    int this[int left,
        int right] => right;

    void Run()
    {
        System.Action<int, int> handler = (int left,
            int right) => { };
        handler(1,
            2);
    }
}
";
            await CSharpAnalyzerVerifier<ParameterListFitAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task ShortBreaksReport()
        {
            const string test = @"
namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }

class Terrain
{
    public void M{|#0:(
        float a,
        float b,
        float c
    )|}
    {
    }

    public void Padded{|#1:(
        float                                        a,
        float                                        b
    )|}
    {
    }

    public void WrappedType{|#2:(
        float
            a,
        float
            b
    )|}
    {
    }

    public void Empty{|#3:(
    )|}
    {
    }

    public Terrain{|#4:(
        int a,
        int b
    )|}
    {
    }

    public void Outer()
    {
        void Inner{|#5:(
            int a,
            int b
        )|}
        {
        }
    }
}

class Primary{|#6:(
    int a,
    int b
)|};

public record Profile{|#7:(
    float a,
    float b,
    float c
)|};
";
            DiagnosticResult[] expected = new DiagnosticResult[8];
            for (int index = 0; index < expected.Length; index++)
            {
                expected[index] = DiagnosticResult
                    .CompilerWarning(ParameterListFitAnalyzer.DiagnosticId)
                    .WithLocation(index);
            }

            await CSharpAnalyzerVerifier<ParameterListFitAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task LowerMaximumAllowsTheShortBreak()
        {
            const string source = "class C { void M(\n    float a,\n    float b,\n    float c\n) { } }";
            await VerifyAsync(source, "warning", "10");
        }

        [TestMethod]
        [DataRow("\"left    right\"")]
        [DataRow("@\"left\t\t right\"")]
        [DataRow("\"\"\"left    right\"\"\"")]
        public async Task LiteralWhitespaceCountsAtTheConfiguredBoundary(string literal)
        {
            string parameter = "string value = " + literal;
            string source = "class C { void M{|#0:(\n    " + parameter + "\n)|} { } }";
            int length = parameter.Length;
            await VerifyAsync(source, "warning", (length - 1).ToString());

            var expected = DiagnosticResult
                .CompilerWarning(ParameterListFitAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments(length, length);
            await VerifyAsync(source, "warning", length.ToString(), expected);

            var above = DiagnosticResult
                .CompilerWarning(ParameterListFitAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments(length, length + 1);
            await VerifyAsync(source, "warning", (length + 1).ToString(), above);
        }

        [TestMethod]
        public async Task LongLiteralWhitespaceDoesNotFitTheDefaultMaximum()
        {
            string source = "class C { void M(\n    string value = \"left" + new string(' ', 120) + "right\"\n) { } }";
            await CSharpAnalyzerVerifier<ParameterListFitAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(source);
        }

        [TestMethod]
        public async Task CommentWhitespaceStillCollapsesWhileLiteralWhitespaceIsPreserved()
        {
            const string parameter = "string /* keep    default */ value = \"left    right\"";
            const string normalized = "string /* keep default */ value = \"left    right\"";
            string source = "class C { void M{|#0:(\n    " + parameter + "\n)|} { } }";
            int length = normalized.Length;
            var expected = DiagnosticResult
                .CompilerWarning(ParameterListFitAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments(length, length);
            await VerifyAsync(source, "warning", length.ToString(), expected);
        }

        [TestMethod]
        public async Task HigherMaximumReportsTheLongBreak()
        {
            const string source = "class C { void M(\n    float amplitude,\n    float frequencyX,\n    float frequencyZ,\n    float detailScale,\n    int seed,\n    int octave,\n    float secondaryAmplitudeXYZ\n) { } }";
            var expected = DiagnosticResult
                .CompilerWarning(ParameterListFitAnalyzer.DiagnosticId)
                .WithSpan(1, 17, 9, 2);
            await VerifyAsync(source, "warning", "130", expected);
        }

        [TestMethod]
        public async Task ZeroMaximumStaysQuiet()
        {
            const string source = "class C { void M(\n    int a,\n    int b\n) { } }";
            await VerifyAsync(source, "warning", "0");
        }

        [TestMethod]
        public async Task InvalidMaximumKeepsDefault()
        {
            const string source = "class C { void M(\n    float a,\n    float b,\n    float c\n) { } }";
            var expected = DiagnosticResult
                .CompilerWarning(ParameterListFitAnalyzer.DiagnosticId)
                .WithSpan(1, 17, 5, 2);
            await VerifyAsync(source, "warning", "nope", expected);
            await VerifyAsync(source, "warning", "-1", expected);
            await VerifyAsync(source, "warning", "  ", expected);
        }

        [TestMethod]
        public async Task LineCommentInsideParameterStaysQuiet()
        {
            const string source = "class C { void M(\n    int // amplitude\n    a,\n    int b\n) { } }";
            await VerifyAsync(source, "warning", null);
        }

        [TestMethod]
        public async Task LineCommentBetweenParametersStaysQuiet()
        {
            const string source = "class C { void M(\n    int a, // amplitude\n    int b\n) { } }";
            await VerifyAsync(source, "warning", null);
        }

        [TestMethod]
        public async Task PreprocessorIfAroundParameterStaysQuiet()
        {
            const string source = "class C { void M(\n    int a,\n#if true\n    int debugFlag,\n#endif\n    int b\n) { } }";
            await VerifyAsync(source, "warning", null);
        }

        [TestMethod]
        public async Task PreprocessorRegionAroundParameterStaysQuiet()
        {
            const string source = "class C { void M(\n    int a,\n#region flags\n    int b,\n#endregion\n    int c\n) { } }";
            await VerifyAsync(source, "warning", null);
        }

        [TestMethod]
        public async Task HashIfInsideStringStillReports()
        {
            const string source = "class C { void M(\n    int b,\n    string option = \"#if true\"\n) { } }";
            var expected = DiagnosticResult
                .CompilerWarning(ParameterListFitAnalyzer.DiagnosticId)
                .WithSpan(1, 17, 4, 2);
            await VerifyAsync(source, "warning", null, expected);
        }

        [TestMethod]
        public async Task SlashSlashInsideStringStillReports()
        {
            const string source = "class C { void M(\n    string scheme,\n    string url = \"http://example\"\n) { } }";
            var expected = DiagnosticResult
                .CompilerWarning(ParameterListFitAnalyzer.DiagnosticId)
                .WithSpan(1, 17, 4, 2)
                .WithMessage("This parameter list is broken across lines, but the parameters fit in 44 characters (limit 120). Write it on one line, as in M(a, b, c).");
            await VerifyAsync(source, "warning", null, expected);
        }

        [TestMethod]
        public async Task BlockCommentStillReports()
        {
            const string source = "class C { void M(\n    int /* amplitude */ a,\n    int b\n) { } }";
            var expected = DiagnosticResult
                .CompilerWarning(ParameterListFitAnalyzer.DiagnosticId)
                .WithSpan(1, 17, 4, 2)
                .WithMessage("This parameter list is broken across lines, but the parameters fit in 28 characters (limit 120). Write it on one line, as in M(a, b, c).");
            await VerifyAsync(source, "warning", null, expected);
        }

        [TestMethod]
        public async Task SeverityErrorReportsError()
        {
            const string source = "class C { void M(\n    int a,\n    int b\n) { } }";
            var expected = DiagnosticResult
                .CompilerError(ParameterListFitAnalyzer.DiagnosticId)
                .WithSpan(1, 17, 4, 2);
            await VerifyAsync(source, "error", null, expected);
        }

        [TestMethod]
        public async Task SeverityMessageReportsInfo()
        {
            const string source = "class C { void M(\n    int a,\n    int b\n) { } }";
            var expected = new DiagnosticResult(ParameterListFitAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
                .WithSpan(1, 17, 4, 2);
            await VerifyAsync(source, "message", null, expected);
        }

        [TestMethod]
        public async Task SeverityOffReportsNothing()
        {
            const string source = "class C { void M(\n    int a,\n    int b\n) { } }";
            await VerifyAsync(source, "off", null);
        }

        private static async Task VerifyAsync(string source, string severity, string? maximum, params DiagnosticResult[] expected)
        {
            var test = new CSharpAnalyzerTest<ParameterListFitAnalyzer, DefaultVerifier>
            {
                TestCode = source,
            };
            test.ExpectedDiagnostics.AddRange(expected);
            string config = "is_global = true\n"
                + "build_property." + ParameterListFitAnalyzer.SeverityPropertyName + " = " + severity + "\n";
            if (maximum != null)
            {
                config += "build_property." + ParameterListFitAnalyzer.MaximumPropertyName + " = " + maximum + "\n";
            }

            test.TestState.AnalyzerConfigFiles.Add(("/.globalconfig", config));
            await test.RunAsync();
        }
    }
}
