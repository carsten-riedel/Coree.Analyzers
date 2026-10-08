using Coree.Analyzers.CodeClarity;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Coree.Analyzers.CodeClarity.Tests
{
    [TestClass]
    public class ParameterListLayoutAnalyzerTests
    {
        [TestMethod]
        public async Task AcceptedLayoutsStayQuiet()
        {
            const string test = @"
namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }

[System.AttributeUsage(System.AttributeTargets.Parameter)]
class MarkAttribute : System.Attribute { }

delegate void Handler(int left, int right, int extra);

class Plain
{
    void None() { }
    void One(int a) { }
    void Flat(int a, int b, int c) { }
    void Pair(int a,
        int b) { }
    void Head(int a,
        int b,
        int c) { }
    void Next(
        int a, int b, int c) { }
    void Expanded(
        int a,
        int b,
        int c
    ) { }
    void Attributed(
        [Mark] int a,
        int b) { }

    public Plain(int a,
        int b,
        int c)
    {
    }

    int this[int left, int right,
        int extra] => extra;

    void Run()
    {
        Handler handler = (int left, int right,
            int extra) => { };
        handler(1, 2,
            3);
    }
}

class PackedPrimary(
    int a, int b, int c);

record EmptyRecord
{
}

public record TerrainProfile(float Amplitude, float FrequencyX, float FrequencyZ, float DetailAmplitude, float DetailX, float DetailZ);

public record TerrainProfileExpanded(
    float Amplitude,
    float FrequencyX,
    float FrequencyZ,
    float DetailAmplitude,
    float DetailX,
    float DetailZ);
";
            await CSharpAnalyzerVerifier<ParameterListLayoutAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task MixedLayoutsReport()
        {
            const string test = @"
namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }

[System.AttributeUsage(System.AttributeTargets.Parameter)]
class MarkAttribute : System.Attribute { }

class Terrain
{
    public void Mixed{|#0:(float amplitude, float frequencyX, float frequencyZ,
        float detailAmplitude, float detailX, float detailZ)|}
    {
    }

    public Terrain{|#1:(float amplitude, float frequencyX,
        float detailAmplitude)|}
    {
    }

    public void Outer()
    {
        void Inner{|#2:(int a, int b,
            int c)|}
        {
        }
    }

    public void TailPacked{|#3:(
        int a,
        int b, int c)|}
    {
    }

    public void AttributedMixed{|#4:([Mark] int a, int b,
        int c)|}
    {
    }
}

public record TerrainProfile{|#5:(float Amplitude, float FrequencyX, float FrequencyZ,
    float DetailAmplitude, float DetailX, float DetailZ)|};

class Primary{|#6:(int a, int b,
    int c)|};

struct PrimaryStruct{|#7:(int a, int b,
    int c)|};

record struct PrimaryRecordStruct{|#8:(int a, int b,
    int c)|};
";
            DiagnosticResult[] expected = new DiagnosticResult[9];
            for (int index = 0; index < expected.Length; index++)
            {
                expected[index] = DiagnosticResult
                    .CompilerWarning(ParameterListLayoutAnalyzer.DiagnosticId)
                    .WithLocation(index);
            }

            await CSharpAnalyzerVerifier<ParameterListLayoutAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task SeverityErrorReportsError()
        {
            const string source = "class C { void M(int a, int b,\n    int c) { } }";
            var expected = DiagnosticResult
                .CompilerError(ParameterListLayoutAnalyzer.DiagnosticId)
                .WithSpan(1, 17, 2, 11);
            await VerifyWithSeverityAsync(source, "error", expected);
        }

        [TestMethod]
        public async Task SeverityMessageReportsInfo()
        {
            const string source = "class C { void M(int a, int b,\n    int c) { } }";
            var expected = new DiagnosticResult(ParameterListLayoutAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
                .WithSpan(1, 17, 2, 11);
            await VerifyWithSeverityAsync(source, "message", expected);
        }

        [TestMethod]
        public async Task SeverityOffReportsNothing()
        {
            const string source = "class C { void M(int a, int b,\n    int c) { } }";
            await VerifyWithSeverityAsync(source, "off");
        }

        private static async Task VerifyWithSeverityAsync(string source, string severity, params DiagnosticResult[] expected)
        {
            var test = new CSharpAnalyzerTest<ParameterListLayoutAnalyzer, DefaultVerifier>
            {
                TestCode = source,
            };
            test.ExpectedDiagnostics.AddRange(expected);
            string config = "is_global = true\n"
                + "build_property." + ParameterListLayoutAnalyzer.SeverityPropertyName + " = " + severity + "\n";
            test.TestState.AnalyzerConfigFiles.Add(("/.globalconfig", config));
            await test.RunAsync();
        }
    }
}
