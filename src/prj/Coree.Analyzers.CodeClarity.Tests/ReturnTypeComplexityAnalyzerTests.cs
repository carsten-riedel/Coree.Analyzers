using Coree.Analyzers.CodeClarity;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Coree.Analyzers.CodeClarity.Tests
{
    [TestClass]
    public class ReturnTypeComplexityAnalyzerTests
    {
        [TestMethod]
        public async Task PreferredGroundHeightIsSimple()
        {
            const string test = @"
class Terrain
{
    public float Amplitude;
    public float FrequencyX;
    public float FrequencyZ;
    public float DetailAmplitude;
    public float DetailX;
    public float DetailZ;
    public float Ground(float x, float z)
    {
        float baseWave = System.MathF.Sin(x * FrequencyX);
        float heightVariation = System.MathF.Cos(z * FrequencyZ);
        float baseHeight = Amplitude * baseWave * heightVariation;
        float detailWave = System.MathF.Sin(x * DetailX + z * DetailZ);
        float detailHeight = DetailAmplitude * detailWave;
        float groundHeight = baseHeight + detailHeight;
        return groundHeight;
    }
}";
            await CSharpAnalyzerVerifier<ReturnTypeComplexityAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task InlinedGroundHeightIsTooComplex()
        {
            const string test = @"
class Terrain
{
    public float Amplitude;
    public float FrequencyX;
    public float FrequencyZ;
    public float DetailAmplitude;
    public float DetailX;
    public float DetailZ;
    public float Ground(float x, float z)
    {
        return {|#0:Amplitude * System.MathF.Sin(x * FrequencyX)
            * System.MathF.Cos(z * FrequencyZ)
            + DetailAmplitude
            * System.MathF.Sin(x * DetailX + z * DetailZ)|};
    }
}";
            var expected = DiagnosticResult
                .CompilerWarning(ReturnTypeComplexityAnalyzer.DiagnosticId)
                .WithLocation(0);
            await CSharpAnalyzerVerifier<ReturnTypeComplexityAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task SingleCalculationStepIsSimple()
        {
            const string test = @"
class C
{
    int Bar(int x) { return x; }
    int M(int a, int b)
    {
        if (a > b) return a - b;
        if (a > b) return a * b;
        if (a > b) return a / b;
        if (a > b) return a % b;
        if (a > b) return a << b;
        if (a > b) return a >> b;
        return a + b;
    }
    int Call(int a) { return Bar(a); }
    int Member(int a) { return this.Bar(a); }
    bool Compare(int a, int b) { return a > b && a >= b; }
    string Join(string a, string b) { return a + "" "" + b; }
    string Name() { return nameof(C); }
    int Identity(int height) { return height; }
    int Negative(int height) { return -height; }
    float Cast(int height) { return (float)height; }
    int Choice(bool ready, int a, int b) { return ready ? a : b; }
    System.Func<int> Deferred(int a, int b, int c) { return () => a * b + c; }
    void None() { return; }
    int Body(int a, int b, int c) => a + b + c;
}";
            await CSharpAnalyzerVerifier<ReturnTypeComplexityAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task NestedCallAndSecondOperatorAreTooComplex()
        {
            const string test = @"
class C
{
    int Bar(int x) { return x; }
    int Nested(int a, int b) { return {|#0:Bar(a * b)|}; }
    int TwoCalls(int a) { return {|#1:Bar(Bar(a))|}; }
    int TwoAdds(int a, int b, int c) { return {|#2:a + b + c|}; }
    int Ternary(bool ready, int a, int b) { return {|#3:ready ? a + b : a|}; }
    async System.Threading.Tasks.Task<int> Awaited(int a, int b)
    {
        return {|#4:await System.Threading.Tasks.Task.FromResult(a + b)|};
    }
}";
            var expected0 = DiagnosticResult.CompilerWarning(ReturnTypeComplexityAnalyzer.DiagnosticId).WithLocation(0);
            var expected1 = DiagnosticResult.CompilerWarning(ReturnTypeComplexityAnalyzer.DiagnosticId).WithLocation(1);
            var expected2 = DiagnosticResult.CompilerWarning(ReturnTypeComplexityAnalyzer.DiagnosticId).WithLocation(2);
            var expected3 = DiagnosticResult.CompilerWarning(ReturnTypeComplexityAnalyzer.DiagnosticId).WithLocation(3);
            var expected4 = DiagnosticResult.CompilerWarning(ReturnTypeComplexityAnalyzer.DiagnosticId).WithLocation(4);
            await CSharpAnalyzerVerifier<ReturnTypeComplexityAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(
                test,
                expected0,
                expected1,
                expected2,
                expected3,
                expected4);
        }

        [TestMethod]
        public async Task MethodGroupAdditionIsNotStringConcatenation()
        {
            const string test = @"
class C
{
    void M() { }
    int N() { return M + 1; }
}";
            var analyzerTest = new CSharpAnalyzerTest<ReturnTypeComplexityAnalyzer, DefaultVerifier>
            {
                TestCode = test,
                CompilerDiagnostics = CompilerDiagnostics.None,
            };
            await analyzerTest.RunAsync();
        }

        [TestMethod]
        public async Task HigherMaximumAllowsTwoAdditions()
        {
            const string source = "class C { int M(int a, int b, int c) { return a + b + c; } }";
            await VerifyWithConfigAsync(source, null, "2");
        }

        [TestMethod]
        public async Task ZeroMaximumFlagsOneAddition()
        {
            const string source = "class C { int M(int a, int b) { return a + b; } }";
            var expected = DiagnosticResult
                .CompilerWarning(ReturnTypeComplexityAnalyzer.DiagnosticId)
                .WithSpan(1, 40, 1, 45);
            await VerifyWithConfigAsync(source, null, "0", expected);
        }

        [TestMethod]
        public async Task InvalidMaximumKeepsDefault()
        {
            const string source = "class C { int M(int a, int b, int c) { return a + b + c; } }";
            var expected = DiagnosticResult
                .CompilerWarning(ReturnTypeComplexityAnalyzer.DiagnosticId)
                .WithSpan(1, 47, 1, 56);
            await VerifyWithConfigAsync(source, null, "nope", expected);
            await VerifyWithConfigAsync(source, null, "-1", expected);
            await VerifyWithConfigAsync(source, null, "   ", expected);
        }

        [TestMethod]
        public async Task SeverityErrorReportsError()
        {
            const string source = "class C { int M(int a, int b, int c) { return a + b + c; } }";
            var expected = DiagnosticResult
                .CompilerError(ReturnTypeComplexityAnalyzer.DiagnosticId)
                .WithSpan(1, 47, 1, 56);
            await VerifyWithSeverityAsync(source, "error", expected);
        }

        [TestMethod]
        public async Task SeverityMessageReportsInfo()
        {
            const string source = "class C { int M(int a, int b, int c) { return a + b + c; } }";
            var expected = new DiagnosticResult(ReturnTypeComplexityAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
                .WithSpan(1, 47, 1, 56);
            await VerifyWithSeverityAsync(source, "message", expected);
        }

        [TestMethod]
        public async Task SeverityOffReportsNothing()
        {
            const string source = "class C { int M(int a, int b, int c) { return a + b + c; } }";
            await VerifyWithSeverityAsync(source, "off");
        }

        private static async Task VerifyWithSeverityAsync(string source, string severity, params DiagnosticResult[] expected)
        {
            await VerifyWithConfigAsync(source, severity, null, expected);
        }

        private static async Task VerifyWithConfigAsync(string source, string? severity, string? maximum, params DiagnosticResult[] expected)
        {
            var test = new CSharpAnalyzerTest<ReturnTypeComplexityAnalyzer, DefaultVerifier>
            {
                TestCode = source,
            };
            test.ExpectedDiagnostics.AddRange(expected);
            string config = "is_global = true\n";
            if (severity != null)
            {
                config += "build_property." + ReturnTypeComplexityAnalyzer.SeverityPropertyName + " = " + severity + "\n";
            }

            if (maximum != null)
            {
                config += "build_property." + ReturnTypeComplexityAnalyzer.MaximumPropertyName + " = " + maximum + "\n";
            }

            test.TestState.AnalyzerConfigFiles.Add(("/.globalconfig", config));
            await test.RunAsync();
        }
    }
}
