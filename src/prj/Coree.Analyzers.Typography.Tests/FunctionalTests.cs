using Coree.Analyzers.Typography;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Coree.Analyzers.Typography.Tests
{
    [TestClass]
    public class FunctionalTests
    {
        private const string EmDashSource = "class C { string s = \"a\u2014b\"; }";
        private const string QuoteSource = "class C { string s = \"x\u201Cy\"; }";

        [TestMethod]
        public async Task EmDashInStringReportsDiagnostic()
        {
            var expected = DiagnosticResult
                .CompilerWarning(EmDashAnalyzer.DiagnosticId)
                .WithSpan(1, 24, 1, 25);

            await CSharpAnalyzerVerifier<EmDashAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(EmDashSource, expected);
        }

        [TestMethod]
        public async Task AsciiHyphenReportsNoDiagnostic()
        {
            const string test = "class C { string s = \"a-b\"; }";
            await CSharpAnalyzerVerifier<EmDashAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TypographicQuoteInStringReportsDiagnostic()
        {
            var expected = DiagnosticResult
                .CompilerWarning(SmartQuotesAnalyzer.DiagnosticId)
                .WithSpan(1, 24, 1, 25);

            await CSharpAnalyzerVerifier<SmartQuotesAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(QuoteSource, expected);
        }

        [TestMethod]
        public async Task AsciiQuotesReportNoDiagnostic()
        {
            const string test = "class C { string s = \"hello\"; }";
            await CSharpAnalyzerVerifier<SmartQuotesAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task EmDashSeverityErrorReportsError()
        {
            var expected = DiagnosticResult
                .CompilerError(EmDashAnalyzer.DiagnosticId)
                .WithSpan(1, 24, 1, 25);

            await VerifyWithSeverityAsync<EmDashAnalyzer>(
                EmDashSource,
                EmDashAnalyzer.SeverityPropertyName,
                "error",
                expected);
        }

        [TestMethod]
        public async Task EmDashSeverityOffReportsNothing()
        {
            await VerifyWithSeverityAsync<EmDashAnalyzer>(
                EmDashSource,
                EmDashAnalyzer.SeverityPropertyName,
                "off");
        }

        [TestMethod]
        public async Task SmartQuotesSeverityMessageReportsInfo()
        {
            var expected = new DiagnosticResult(SmartQuotesAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
                .WithSpan(1, 24, 1, 25);

            await VerifyWithSeverityAsync<SmartQuotesAnalyzer>(
                QuoteSource,
                SmartQuotesAnalyzer.SeverityPropertyName,
                "message",
                expected);
        }

        [TestMethod]
        public async Task EmDashInMatchingAdditionalFileReportsDiagnostic()
        {
            var expected = DiagnosticResult
                .CompilerWarning(EmDashAnalyzer.DiagnosticId)
                .WithSpan("sample.txt", 1, 2, 1, 3);

            await VerifyWithAdditionalFileAsync<EmDashAnalyzer>(
                EmDashAnalyzer.IncludesPropertyName,
                "*.txt",
                "sample.txt",
                "a\u2014b",
                expected);
        }

        [TestMethod]
        public async Task SmartQuotesInMatchingAdditionalFileReportsDiagnostic()
        {
            var expected = DiagnosticResult
                .CompilerWarning(SmartQuotesAnalyzer.DiagnosticId)
                .WithSpan("sample.txt", 1, 2, 1, 3);

            await VerifyWithAdditionalFileAsync<SmartQuotesAnalyzer>(
                SmartQuotesAnalyzer.IncludesPropertyName,
                "*.txt",
                "sample.txt",
                "x\u201Cy",
                expected);
        }

        [TestMethod]
        public async Task AdditionalFileWithoutPropertyReportsNothing()
        {
            var test = new CSharpAnalyzerTest<EmDashAnalyzer, DefaultVerifier>
            {
                TestCode = "class C { }",
            };
            test.TestState.AdditionalFiles.Add(("sample.txt", "a\u2014b"));
            await test.RunAsync();
        }

        [TestMethod]
        public async Task AdditionalFileNotMatchingGlobReportsNothing()
        {
            await VerifyWithAdditionalFileAsync<EmDashAnalyzer>(
                EmDashAnalyzer.IncludesPropertyName,
                "*.csproj",
                "sample.txt",
                "a\u2014b");
        }

        [TestMethod]
        public async Task EmDashInMatchingCsprojAdditionalFileReportsDiagnostic()
        {
            var expected = DiagnosticResult
                .CompilerWarning(EmDashAnalyzer.DiagnosticId)
                .WithSpan("sample.csproj", 1, 6, 1, 7);

            await VerifyWithAdditionalFileAsync<EmDashAnalyzer>(
                EmDashAnalyzer.IncludesPropertyName,
                "*.txt|*.csproj",
                "sample.csproj",
                "<!-- \u2014 -->",
                expected);
        }

        [TestMethod]
        public async Task EmptyIncludesPropertyReportsNothing()
        {
            await VerifyWithAdditionalFileAsync<EmDashAnalyzer>(
                EmDashAnalyzer.IncludesPropertyName,
                string.Empty,
                "sample.txt",
                "a\u2014b");
        }

        [TestMethod]
        public async Task RecursiveIncludeMatchesNestedAdditionalFile()
        {
            var expected = DiagnosticResult
                .CompilerWarning(EmDashAnalyzer.DiagnosticId)
                .WithSpan("sub/sample.txt", 1, 2, 1, 3);

            await VerifyWithAdditionalFileAsync<EmDashAnalyzer>(
                EmDashAnalyzer.IncludesPropertyName,
                "**/*.txt",
                "sub/sample.txt",
                "a\u2014b",
                expected);
        }

        [TestMethod]
        public async Task ExcludeRemovesNestedFileFromIncludes()
        {
            await VerifyWithAdditionalFileAsync<EmDashAnalyzer>(
                EmDashAnalyzer.IncludesPropertyName,
                "**/*.txt",
                EmDashAnalyzer.ExcludesPropertyName,
                "sub/*.txt",
                "sub/sample.txt",
                "a\u2014b");
        }

        [TestMethod]
        public async Task SmartQuotesExcludeRemovesNestedFileFromIncludes()
        {
            await VerifyWithAdditionalFileAsync<SmartQuotesAnalyzer>(
                SmartQuotesAnalyzer.IncludesPropertyName,
                "**/*.txt",
                SmartQuotesAnalyzer.ExcludesPropertyName,
                "sub/*.txt",
                "sub/sample.txt",
                "x\u201Cy");
        }

        [TestMethod]
        public async Task ProjectDirectoryFileReportsWhenIncludesAll()
        {
            var dir = CreateTempDirectory();
            try
            {
                Directory.CreateDirectory(Path.Combine(dir, "docs"));
                var path = Path.Combine(dir, "docs", "notes.md");
                File.WriteAllText(path, "a\u2014b");
                var expected = DiagnosticResult
                    .CompilerWarning(EmDashAnalyzer.DiagnosticId)
                    .WithSpan(path, 1, 2, 1, 3);

                await VerifyWithProjectDirectoryAsync<EmDashAnalyzer>(
                    dir,
                    "**",
                    string.Empty,
                    expected);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        [TestMethod]
        public async Task ProjectDirectorySkipsBinObjAndBinaryFiles()
        {
            var dir = CreateTempDirectory();
            try
            {
                Directory.CreateDirectory(Path.Combine(dir, "bin"));
                File.WriteAllText(Path.Combine(dir, "bin", "out.txt"), "a\u2014b");
                File.WriteAllBytes(Path.Combine(dir, "blob.bin"), new byte[] { 0, 0xE2, 0x80, 0x94 });
                await VerifyWithProjectDirectoryAsync<EmDashAnalyzer>(dir, "**", string.Empty);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        [TestMethod]
        public async Task ProjectDirectoryExcludeRemovesDiskFile()
        {
            var dir = CreateTempDirectory();
            try
            {
                File.WriteAllText(Path.Combine(dir, "skip.md"), "a\u2014b");
                await VerifyWithProjectDirectoryAsync<EmDashAnalyzer>(dir, "**", "*.md");
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        [TestMethod]
        public async Task EmptyIncludesSkipsProjectDirectoryWalk()
        {
            var dir = CreateTempDirectory();
            try
            {
                File.WriteAllText(Path.Combine(dir, "notes.md"), "a\u2014b");
                await VerifyWithProjectDirectoryAsync<EmDashAnalyzer>(dir, string.Empty, string.Empty);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        [TestMethod]
        public async Task AdditionalFileAndProjectDirectoryFileReportOnce()
        {
            var dir = CreateTempDirectory();
            try
            {
                var path = Path.Combine(dir, "notes.md");
                File.WriteAllText(path, "a\u2014b");
                var expected = DiagnosticResult
                    .CompilerWarning(EmDashAnalyzer.DiagnosticId)
                    .WithSpan(path, 1, 2, 1, 3);

                var test = new CSharpAnalyzerTest<EmDashAnalyzer, DefaultVerifier>
                {
                    TestCode = "class C { }",
                };
                test.ExpectedDiagnostics.Add(expected);
                test.TestState.AdditionalFiles.Add((path, "a\u2014b"));
                test.TestState.AnalyzerConfigFiles.Add((
                    "/.globalconfig",
                    "is_global = true\nbuild_property." + EmDashAnalyzer.IncludesPropertyName + " = **\nbuild_property.MSBuildProjectDirectory = " + ToConfigPath(dir) + "\n"));
                await test.RunAsync();
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        private static async Task VerifyWithSeverityAsync<TAnalyzer>(
            string source,
            string propertyName,
            string severity,
            params DiagnosticResult[] expected)
            where TAnalyzer : Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer, new()
        {
            var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
            {
                TestCode = source,
            };
            test.ExpectedDiagnostics.AddRange(expected);
            test.TestState.AnalyzerConfigFiles.Add((
                "/.globalconfig",
                "is_global = true\nbuild_property." + propertyName + " = " + severity + "\n"));
            await test.RunAsync();
        }

        private static async Task VerifyWithAdditionalFileAsync<TAnalyzer>(
            string includesPropertyName,
            string includes,
            string additionalPath,
            string additionalContent,
            params DiagnosticResult[] expected)
            where TAnalyzer : Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer, new()
        {
            await VerifyWithAdditionalFileAsync<TAnalyzer>(
                includesPropertyName,
                includes,
                string.Empty,
                string.Empty,
                additionalPath,
                additionalContent,
                expected);
        }

        private static async Task VerifyWithAdditionalFileAsync<TAnalyzer>(
            string includesPropertyName,
            string includes,
            string excludesPropertyName,
            string excludes,
            string additionalPath,
            string additionalContent,
            params DiagnosticResult[] expected)
            where TAnalyzer : Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer, new()
        {
            var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
            {
                TestCode = "class C { }",
            };
            test.ExpectedDiagnostics.AddRange(expected);
            test.TestState.AdditionalFiles.Add((additionalPath, additionalContent));
            var config = "is_global = true\nbuild_property." + includesPropertyName + " = " + includes + "\n";
            if (!string.IsNullOrEmpty(excludesPropertyName))
            {
                config += "build_property." + excludesPropertyName + " = " + excludes + "\n";
            }

            test.TestState.AnalyzerConfigFiles.Add(("/.globalconfig", config));
            await test.RunAsync();
        }

        private static async Task VerifyWithProjectDirectoryAsync<TAnalyzer>(
            string projectDirectory,
            string includes,
            string excludes,
            params DiagnosticResult[] expected)
            where TAnalyzer : Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer, new()
        {
            var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
            {
                TestCode = "class C { }",
            };
            test.ExpectedDiagnostics.AddRange(expected);
            var config = "is_global = true\nbuild_property." + EmDashAnalyzer.IncludesPropertyName + " = " + includes
                + "\nbuild_property.MSBuildProjectDirectory = " + ToConfigPath(projectDirectory) + "\n";
            if (!string.IsNullOrEmpty(excludes))
            {
                config += "build_property." + EmDashAnalyzer.ExcludesPropertyName + " = " + excludes + "\n";
            }

            test.TestState.AnalyzerConfigFiles.Add(("/.globalconfig", config));
            await test.RunAsync();
        }

        private static string CreateTempDirectory()
        {
            var dir = Path.Combine(Path.GetTempPath(), "coree-analyzers-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static string ToConfigPath(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}
