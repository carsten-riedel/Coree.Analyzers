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
            const string blockMessage = "Return expression is too complex. Extract intermediate calculations into local variables.";
            var expected0 = DiagnosticResult.CompilerWarning(ReturnTypeComplexityAnalyzer.DiagnosticId).WithLocation(0).WithMessage(blockMessage);
            var expected1 = DiagnosticResult.CompilerWarning(ReturnTypeComplexityAnalyzer.DiagnosticId).WithLocation(1).WithMessage(blockMessage);
            var expected2 = DiagnosticResult.CompilerWarning(ReturnTypeComplexityAnalyzer.DiagnosticId).WithLocation(2).WithMessage(blockMessage);
            var expected3 = DiagnosticResult.CompilerWarning(ReturnTypeComplexityAnalyzer.DiagnosticId).WithLocation(3).WithMessage(blockMessage);
            var expected4 = DiagnosticResult.CompilerWarning(ReturnTypeComplexityAnalyzer.DiagnosticId).WithLocation(4).WithMessage(blockMessage);
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
        [DataRow("@nameof(a + b)")]
        [DataRow("nameof(a + b)")]
        [DataRow("this.nameof(a + b)")]
        [DataRow("@nameof(@nameof(a))")]
        [DataRow("nameof(nameof(a))")]
        public async Task NameofMethodCallsWithAnotherStepReport(string expression)
        {
            string source = "class C { int @nameof(int x) { return x; } "
                + "int M(int a, int b) { return {|#0:" + expression + "|}; } }";
            var expected = DiagnosticResult.CompilerWarning(ReturnTypeComplexityAnalyzer.DiagnosticId)
                .WithLocation(0);
            await CSharpAnalyzerVerifier<ReturnTypeComplexityAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(source, expected);
        }

        [TestMethod]
        [DataRow("@nameof(a)")]
        [DataRow("nameof(a)")]
        [DataRow("this.nameof(a)")]
        public async Task SingleNameofMethodCallHonorsMaximum(string expression)
        {
            const string prefix = "class C { int @nameof(int x) { return x; } int M(int a) { return ";
            string source = prefix + expression + "; } }";
            await VerifyWithConfigAsync(source, null, "1");

            string markedSource = prefix + "{|#0:" + expression + "|}; } }";
            var expected = DiagnosticResult.CompilerWarning(ReturnTypeComplexityAnalyzer.DiagnosticId)
                .WithLocation(0);
            await VerifyWithConfigAsync(markedSource, null, "0", expected);
        }

        [TestMethod]
        public async Task GenuineNameofHasNoCostAtZeroMaximum()
        {
            const string source = @"
class Callable
{
    public int @nameof(int value) { return value; }
}
class C
{
    string TypeName() { return nameof(Callable); }
    string MemberName() { return nameof(Callable.@nameof); }
}";
            await VerifyWithConfigAsync(source, null, "0");
        }

        [TestMethod]
        public async Task UnresolvedNameofExpressionsDoNotCrash()
        {
            const string source = @"
class C
{
    string Name() { return nameof(Missing); }
    int Call(int a, int b) { return {|#0:@nameof(a + b)|}; }
}";
            var test = new CSharpAnalyzerTest<ReturnTypeComplexityAnalyzer, DefaultVerifier>
            {
                TestCode = source,
                CompilerDiagnostics = CompilerDiagnostics.None,
            };
            test.ExpectedDiagnostics.Add(DiagnosticResult.CompilerWarning(ReturnTypeComplexityAnalyzer.DiagnosticId)
                .WithLocation(0));
            await test.RunAsync();
        }

        [TestMethod]
        [DataRow("a >>> b", "0", true)]
        [DataRow("a >>> b", "1", false)]
        [DataRow("a >>> (b + c)", "1", true)]
        [DataRow("(a >>> b) + c", "1", true)]
        [DataRow("a >>> b >>> c", "1", true)]
        [DataRow("F(a) >>> b", "1", true)]
        [DataRow("a >>> (b + c)", "2", false)]
        [DataRow("(a >>> b) + c", "2", false)]
        [DataRow("a >>> b >>> c", "2", false)]
        [DataRow("F(a) >>> b", "2", false)]
        [DataRow("a >> b", "0", true)]
        [DataRow("a >> b", "1", false)]
        [DataRow("a & b | c", "0", false)]
        public async Task UnsignedRightShiftUsesTheSameCalculationThreshold(string expression, string maximum, bool reports)
        {
            string returned = reports ? "{|#0:" + expression + "|}" : expression;
            string source = "class C { int F(int x) { return x; } int M(int a, int b, int c) { return " + returned + "; } }";
            DiagnosticResult[] expected = reports
                ? new[] { DiagnosticResult.CompilerWarning(ReturnTypeComplexityAnalyzer.DiagnosticId).WithLocation(0) }
                : System.Array.Empty<DiagnosticResult>();
            await VerifyWithConfigAsync(source, null, maximum, expected);
        }

        [TestMethod]
        public async Task DeferredUnsignedShiftsStayOutsideTheReturnCalculation()
        {
            const string source = "class C { System.Func<int> M(int a, int b, int c) { return () => a >>> b >>> c; } }";
            await VerifyWithConfigAsync(source, null, "0");
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

        [TestMethod]
        public async Task UserDefinedAdditionWithStringOperandCounts()
        {
            const string test = @"
struct Meter
{
    public int Raw;
    public static int operator +(string label, Meter value) => value.Raw;
    public static int operator +(Meter value, string label) => value.Raw;
}

class C
{
    int Named(string label, Meter value) { return label + value; }
    int FromValue(Meter value, string label) { return value + label; }
    int Combined(string label, Meter value, int extra) { return {|#0:label + value + extra|}; }
    string Concat(string left, string right) { return left + "" "" + right; }
}";
            var expected = DiagnosticResult
                .CompilerWarning(ReturnTypeComplexityAnalyzer.DiagnosticId)
                .WithLocation(0);
            await CSharpAnalyzerVerifier<ReturnTypeComplexityAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task UserDefinedAdditionReportsAtZeroMaximum()
        {
            const string source = @"struct Meter
{
    public int Raw;
    public static int operator +(string label, Meter value) => value.Raw;
    public static int operator +(Meter value, string label) => value.Raw;
}

class C
{
    int Named(string label, Meter value) { return label + value; }
    int FromValue(Meter value, string label) { return value + label; }
}";
            var named = DiagnosticResult
                .CompilerWarning(ReturnTypeComplexityAnalyzer.DiagnosticId)
                .WithSpan(10, 51, 10, 64);
            var fromValue = DiagnosticResult
                .CompilerWarning(ReturnTypeComplexityAnalyzer.DiagnosticId)
                .WithSpan(11, 55, 11, 68);
            await VerifyWithConfigAsync(source, null, "0", named, fromValue);
        }

        [TestMethod]
        public async Task UserDefinedAdditionAllowsTwoStepsAtHigherMaximum()
        {
            const string source = @"struct Meter
{
    public int Raw;
    public static int operator +(string label, Meter value) => value.Raw;
}

class C
{
    int Combined(string label, Meter value, int extra) { return label + value + extra; }
}";
            await VerifyWithConfigAsync(source, null, "2");
        }

        [TestMethod]
        public async Task BuiltInStringConcatenationStaysQuietAtZeroMaximum()
        {
            const string source = "class C { string M(string a, string b, string c) { return a + \" \" + b + c; } }";
            await VerifyWithConfigAsync(source, null, "0");
        }

        [TestMethod]
        public async Task TwoStepYieldReturnReports()
        {
            const string source = "class C { System.Collections.Generic.IEnumerable<int> M(int a, int b, int c) { yield return {|#0:a + b + c|}; } }";
            var expected = DiagnosticResult.CompilerWarning(ReturnTypeComplexityAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithMessage("Return expression is too complex. Extract intermediate calculations into local variables.");
            await CSharpAnalyzerVerifier<ReturnTypeComplexityAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(source, expected);
        }

        [TestMethod]
        public async Task OneStepYieldReturnStaysQuiet()
        {
            const string source = "class C { System.Collections.Generic.IEnumerable<int> M(int a, int b) { yield return a + b; } }";
            await CSharpAnalyzerVerifier<ReturnTypeComplexityAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(source);
        }

        [TestMethod]
        public async Task YieldReturnNameofStaysQuietAtZeroMaximum()
        {
            const string source = "class C { System.Collections.Generic.IEnumerable<string> M() { yield return nameof(C); } }";
            await VerifyWithConfigAsync(source, null, "0");
        }

        [TestMethod]
        public async Task YieldBreakStaysQuiet()
        {
            const string source = "class C { System.Collections.Generic.IEnumerable<int> M() { yield break; } }";
            await CSharpAnalyzerVerifier<ReturnTypeComplexityAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(source);
        }

        [TestMethod]
        public async Task ExpressionBodiedReturnedValuesReport()
        {
            const string source = @"
class Meter
{
    public int Raw;
    int Method(int a, int b, int c) => {|#0:a + b + c|};
    int Local(int a, int b, int c)
    {
        int Inner(int x, int y, int z) => {|#1:x + y + z|};
        return Inner(a, b, c);
    }
    int Property => {|#2:Raw + Raw + 1|};
    int this[int a, int b] => {|#3:a + b + Raw|};
    int Read { get => {|#4:Raw + Raw + 1|}; }
    public static int operator +(Meter left, Meter right) => {|#5:left.Raw + right.Raw + 1|};
    public static explicit operator int(Meter value) => {|#6:value.Raw + value.Raw + 1|};
    async System.Threading.Tasks.Task<int> Sum(int a, int b, int c) => {|#7:a + b + c|};
    async System.Threading.Tasks.ValueTask<int> SumValue(int a, int b, int c) => {|#8:a + b + c|};
}";
            const string arrowMessage = "Return expression is too complex. Move this arrow into a block and extract the extra steps into local variables there.";
            DiagnosticResult[] expected = new DiagnosticResult[9];
            for (int index = 0; index < expected.Length; index++)
            {
                expected[index] = DiagnosticResult
                    .CompilerWarning(ReturnTypeComplexityAnalyzer.DiagnosticId)
                    .WithLocation(index)
                    .WithMessage(arrowMessage);
            }

            await CSharpAnalyzerVerifier<ReturnTypeComplexityAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(source, expected);
        }

        [TestMethod]
        public async Task ExpressionBodiedNonValuesAndSingleStepsStayQuiet()
        {
            const string source = @"
class Meter
{
    public int Raw;
    int Method(int a, int b) => a + b;
    int Local(int a, int b)
    {
        int Inner(int x, int y) => x + y;
        int Block(int x, int y) { return x + y; }
        return Inner(a, b);
    }
    int Property => Raw + 1;
    int BlockProperty { get { return Raw + 1; } }
    int this[int a] => a + Raw;
    int this[int a, int b] { get { return a + b; } }
    int Read { get => Raw + 1; }
    int BlockRead { get { return Raw; } set { Raw = value + value + value; } }
    public static int operator +(Meter left, Meter right) => left.Raw + right.Raw;
    public static int operator -(Meter left, Meter right) { return left.Raw + right.Raw; }
    public static explicit operator int(Meter value) => value.Raw + 1;
    public static explicit operator long(Meter value) { return value.Raw + 1; }
    void Log(int a, int b, int c) => Write(a + b + c);
    async System.Threading.Tasks.Task Delay(int a, int b, int c) => System.Threading.Tasks.Task.Delay(a + b + c);
    async System.Threading.Tasks.ValueTask Clear(int a, int b, int c) => new System.Threading.Tasks.ValueTask(System.Threading.Tasks.Task.Delay(a + b + c));
    async System.Threading.Tasks.Task<int> One(int a, int b) => a + b;
    System.Func<int> Deferred(int a, int b, int c) => () => a + b + c;
    Meter(int a, int b, int c) => Raw = a + b + c;
    void Write(int value) { }
}";
            await CSharpAnalyzerVerifier<ReturnTypeComplexityAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(source);
        }

        [TestMethod]
        public async Task ExpressionBodiedNameofStaysQuietAtZeroMaximum()
        {
            const string source = "class C { string Name() => nameof(C); }";
            await VerifyWithConfigAsync(source, null, "0");
        }

        [TestMethod]
        public void MissingMethodSymbolDoesNotCountAsReturnedValue()
        {
            Assert.IsFalse(ReturnTypeComplexityAnalyzer.ReturnsValue(null));
        }

        [TestMethod]
        public void DescriptorNamesTheConditionalOperator()
        {
            string description = new ReturnTypeComplexityAnalyzer().SupportedDiagnostics[0].Description.ToString();
            StringAssert.Contains(description, "?:");
            StringAssert.Contains(description, "&& and || do not count");
            Assert.IsFalse(description.Contains("conditional operators"));
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
