using Coree.Analyzers.CodeClarity;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Coree.Analyzers.CodeClarity.Tests
{
    [TestClass]
    public class MethodBlankLineAnalyzerTests
    {
        [TestMethod]
        public async Task MissingBlankLineReports()
        {
            const string test = @"
namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }

class Packed
{
    public bool Walkable()
    {
        return true;
    }
    private bool {|#0:StaticSegmentClear|}()
    {
        return true;
    }
    private bool {|#1:CanSee|}()
    {
        return true;
    }
}

class SameLine
{
    void A() { } void {|#2:B|}() { }
}

class Noted
{
    void A() { }
    // note
    void {|#3:B|}() { }
}

class Documented
{
    void A() { }
    /// <summary>x</summary>
    void {|#4:B|}() { }
}

class Attributed
{
    void A() { }
    [System.Obsolete]
    void {|#5:B|}() { }
}

class ExpressionBodied
{
    int A() => 1;
    int {|#6:B|}() => 2;
}

class Nested
{
    void OuterA() { }

    void OuterB() { }

    class Inner
    {
        void A() { }
        void {|#7:B|}() { }
    }
}

struct Value
{
    void A() { }
    void {|#8:B|}() { }
}

record Profile
{
    void A() { }
    void {|#9:B|}() { }
}

interface Defaults
{
    void A() { }
    void {|#10:B|}() { }
}

class Preprocessor
{
    void A() { }
#if true
    void {|#11:B|}() { }
#endif
}
";
            DiagnosticResult[] expected = new DiagnosticResult[12];
            for (int index = 0; index < expected.Length; index++)
            {
                expected[index] = DiagnosticResult
                    .CompilerWarning(MethodBlankLineAnalyzer.DiagnosticId)
                    .WithLocation(index);
            }

            expected[0] = expected[0].WithMessage("Put a blank line before this method. End the previous method, leave one empty line, then start this method, as in void Walkable() {}\\n\\nvoid StaticSegmentClear() {}.");
            await CSharpAnalyzerVerifier<MethodBlankLineAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task SeparatedAndOtherMembersStayQuiet()
        {
            const string test = @"
class Separated
{
    void A()
    {
        return;
    }

    void B() { }

    void C() { }
}

class WhitespaceLine
{
    void A() { }
    
    void B() { }
}

class BlankThenComment
{
    void A() { }

    // note
    void B() { }
}

class CommentThenBlank
{
    void A() { }
    // note

    void B() { }
}

class BlankThenDoc
{
    void A() { }

    /// <summary>x</summary>
    void B() { }
}

class BlankThenAttribute
{
    void A() { }

    [System.Obsolete]
    void B() { }
}

class PropertyBetween
{
    void A() { }
    int Value => 1;
    void B() { }
}

class ConstructorThenMethod
{
    public ConstructorThenMethod() { }
    void B() { }
}

class FieldThenMethod
{
    int value = 1;
    void B() { value = 2; }
}

class Locals
{
    void Outer()
    {
        void InnerA() { }
        void InnerB() { }
    }
}

interface Signatures
{
    void A();
    void B();
}

interface BodyThenSignature
{
    void A() { }
    void B();
}

interface SignatureThenBody
{
    void A();
    void B() { }
}

class OnlyOne
{
    void A() { }
}
";
            await CSharpAnalyzerVerifier<MethodBlankLineAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task CarriageReturnBlankLineStaysQuiet()
        {
            const string source = "class C {\r\n    void A() { }\r\n\r\n    void B() { }\r\n}";
            await CSharpAnalyzerVerifier<MethodBlankLineAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(source);
        }

        [TestMethod]
        public async Task SeverityErrorReportsError()
        {
            const string source = "class C { void A() { } void {|#0:B|}() { } }";
            var expected = DiagnosticResult
                .CompilerError(MethodBlankLineAnalyzer.DiagnosticId)
                .WithLocation(0);
            await VerifyAsync(source, "error", expected);
        }

        [TestMethod]
        public async Task SeverityMessageReportsInfo()
        {
            const string source = "class C { void A() { } void {|#0:B|}() { } }";
            var expected = new DiagnosticResult(MethodBlankLineAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
                .WithLocation(0);
            await VerifyAsync(source, "message", expected);
        }

        [TestMethod]
        public async Task SeverityOffReportsNothing()
        {
            const string source = "class C { void A() { } void B() { } }";
            await VerifyAsync(source, "off");
        }

        private static async Task VerifyAsync(string source, string severity, params DiagnosticResult[] expected)
        {
            var test = new CSharpAnalyzerTest<MethodBlankLineAnalyzer, DefaultVerifier>
            {
                TestCode = source,
            };
            test.ExpectedDiagnostics.AddRange(expected);
            string config = "is_global = true\n"
                + "build_property." + MethodBlankLineAnalyzer.SeverityPropertyName + " = " + severity + "\n";
            test.TestState.AnalyzerConfigFiles.Add(("/.globalconfig", config));
            await test.RunAsync();
        }
    }
}
