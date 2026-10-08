using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Coree.Analyzers.Typography.Tests
{
    [TestClass]
    public class AdditionalFileRegistrationTests
    {
        private const string Targets =
            "AddEmDashAnalyzerAdditionalFiles;AddSmartQuotesAnalyzerAdditionalFiles;AddApostropheAnalyzerAdditionalFiles;AddEllipsisAnalyzerAdditionalFiles;AddMinusAnalyzerAdditionalFiles;AddNbspAnalyzerAdditionalFiles";

        [TestMethod]
        public void CompiledSourcesStayOutOfAdditionalFiles()
        {
            var props = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "Coree.Analyzers.Typography",
                "AnalyzerPackage",
                "Coree.Analyzers.Typography.props"));
            Assert.IsTrue(File.Exists(props), props);

            var root = Path.Combine(Path.GetTempPath(), "typography-additional-" + Guid.NewGuid().ToString("N"));
            var fixture = Path.Combine(root, "fixture");
            var project = Path.Combine(fixture, "Fixture.csproj");
            try
            {
                WriteFixture(fixture, project, props);
                Restore(project);

                var standard = Evaluate(project);
                AssertNoCompileOverlap(standard);
                Assert.AreEqual(1, Count(standard, "Compile", "Program.cs"));
                Assert.AreEqual(0, Count(standard, "AdditionalFiles", "Program.cs"));
                Assert.AreEqual(1, Count(standard, "Compile", "Widget.cs"));
                Assert.AreEqual(0, Count(standard, "AdditionalFiles", "Widget.cs"));
                Assert.IsTrue(Count(standard, "Compile", "Alias.cs") >= 1);
                Assert.AreEqual(0, Count(standard, "AdditionalFiles", "Alias.cs"));
                Assert.AreEqual(1, Count(standard, "Compile", "Linked.cs"));
                Assert.AreEqual(0, Count(standard, "AdditionalFiles", "Linked.cs"));
                Assert.AreEqual(0, Count(standard, "Compile", "Loose.cs"));
                Assert.AreEqual(6, Count(standard, "AdditionalFiles", "Loose.cs"));
                Assert.AreEqual(6, Count(standard, "AdditionalFiles", "Sample.txt"));
                Assert.AreEqual(6, Count(standard, "AdditionalFiles", "Readme.md"));
                Assert.AreEqual(6, Count(standard, "AdditionalFiles", "app.json"));
                Assert.AreEqual(6, Count(standard, "AdditionalFiles", "note.txt"));
                Assert.AreEqual(6, Count(standard, "AdditionalFiles", "Fixture.csproj"));
                Assert.AreEqual(0, Count(standard, "AdditionalFiles", "skip.txt"));
                Assert.AreEqual(0, Count(standard, "AdditionalFiles", "HEAD"));
                Assert.AreEqual(0, Count(standard, "AdditionalFiles", "cache.txt"));
                Assert.IsTrue(Property(standard, "EmDashAnalyzerExcludes").IndexOf("*.dat", StringComparison.Ordinal) >= 0);
                Assert.AreEqual("warning", Property(standard, "EmDashAnalyzerSeverity"));
                Assert.AreEqual("warning", Property(standard, "MinusAnalyzerSeverity"));
                Assert.AreEqual("warning", Property(standard, "NbspAnalyzerSeverity"));

                var customCs = Evaluate(
                    project,
                    "-p:EmDashAnalyzerIncludes=**/*.cs",
                    "-p:SmartQuotesAnalyzerIncludes=**/*.cs",
                    "-p:ApostropheAnalyzerIncludes=**/*.cs",
                    "-p:EllipsisAnalyzerIncludes=**/*.cs",
                    "-p:MinusAnalyzerIncludes=**/*.cs",
                    "-p:NbspAnalyzerIncludes=**/*.cs",
                    "-p:EmDashAnalyzerSeverity=error");
                AssertNoCompileOverlap(customCs);
                Assert.AreEqual(0, Count(customCs, "AdditionalFiles", "Program.cs"));
                Assert.AreEqual(0, Count(customCs, "AdditionalFiles", "Widget.cs"));
                Assert.AreEqual(6, Count(customCs, "AdditionalFiles", "Loose.cs"));
                Assert.AreEqual(0, Count(customCs, "AdditionalFiles", "Sample.txt"));
                Assert.AreEqual("error", Property(customCs, "EmDashAnalyzerSeverity"));

                var filtered = Evaluate(
                    project,
                    "-p:EmDashAnalyzerExcludes=**/*.md",
                    "-p:EmDashAnalyzerAdditionalExcludes=nested/**",
                    "-p:SmartQuotesAnalyzerSeverity=message",
                    "-p:ApostropheAnalyzerSeverity=off");
                AssertNoCompileOverlap(filtered);
                Assert.AreEqual(5, Count(filtered, "AdditionalFiles", "Readme.md"));
                Assert.AreEqual(5, Count(filtered, "AdditionalFiles", "note.txt"));
                Assert.AreEqual(6, Count(filtered, "AdditionalFiles", "Sample.txt"));
                Assert.AreEqual(0, Count(filtered, "AdditionalFiles", "skip.txt"));
                Assert.AreEqual("message", Property(filtered, "SmartQuotesAnalyzerSeverity"));
                Assert.AreEqual("off", Property(filtered, "ApostropheAnalyzerSeverity"));

                var jump = Path.Combine(root, "jump");
                Directory.CreateDirectory(jump);
                var viaParent = Path.Combine(jump, "..", "fixture", "Fixture.csproj");
                var fromDotDot = EvaluateIn(viaParent, jump);
                AssertNoCompileOverlap(fromDotDot);
                Assert.AreEqual(0, Count(fromDotDot, "AdditionalFiles", "Program.cs"));
                Assert.AreEqual(6, Count(fromDotDot, "AdditionalFiles", "Loose.cs"));
                Assert.AreEqual(6, Count(fromDotDot, "AdditionalFiles", "Sample.txt"));

                var designTime = Evaluate(project, "-p:DesignTimeBuild=true");
                AssertNoCompileOverlap(designTime);
                Assert.AreEqual(1, Count(designTime, "Compile", "Program.cs"));
                Assert.AreEqual(0, Count(designTime, "AdditionalFiles", "Program.cs"));
                Assert.AreEqual(6, Count(designTime, "AdditionalFiles", "Loose.cs"));
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, recursive: true);
                }
            }
        }

        private static void WriteFixture(string fixture, string project, string props)
        {
            Directory.CreateDirectory(Path.Combine(fixture, "nested"));
            Directory.CreateDirectory(Path.Combine(fixture, "bin"));
            Directory.CreateDirectory(Path.Combine(fixture, "obj"));
            Directory.CreateDirectory(Path.Combine(fixture, ".git"));
            Directory.CreateDirectory(Path.Combine(fixture, ".vs"));
            var parent = Path.GetDirectoryName(fixture);
            if (parent == null)
            {
                throw new InvalidOperationException(fixture);
            }

            var outside = Path.Combine(parent, "outside");
            Directory.CreateDirectory(outside);

            File.WriteAllText(Path.Combine(fixture, "Program.cs"), "class Program { static void Main() { } }\n");
            File.WriteAllText(Path.Combine(fixture, "Loose.cs"), "class Loose { }\n");
            File.WriteAllText(Path.Combine(fixture, "nested", "Widget.cs"), "class Widget { }\n");
            File.WriteAllText(Path.Combine(fixture, "nested", "Alias.cs"), "class Alias { }\n");
            File.WriteAllText(Path.Combine(outside, "Linked.cs"), "class Linked { }\n");
            File.WriteAllText(Path.Combine(fixture, "Sample.txt"), "sample\n");
            File.WriteAllText(Path.Combine(fixture, "Readme.md"), "readme\n");
            File.WriteAllText(Path.Combine(fixture, "app.json"), "{}\n");
            File.WriteAllText(Path.Combine(fixture, "nested", "note.txt"), "note\n");
            File.WriteAllText(Path.Combine(fixture, "payload.dat"), "dat\n");
            File.WriteAllText(Path.Combine(fixture, "icon.png"), "png\n");
            File.WriteAllText(Path.Combine(fixture, "bin", "skip.txt"), "bin\n");
            File.WriteAllText(Path.Combine(fixture, "obj", "skip.txt"), "obj\n");
            File.WriteAllText(Path.Combine(fixture, ".git", "HEAD"), "ref\n");
            File.WriteAllText(Path.Combine(fixture, ".vs", "cache.txt"), "vs\n");

            var import = props.Replace("&", "&amp;").Replace("\"", "&quot;");
            File.WriteAllText(project, @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>disable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <Import Project=""" + import + @""" />
  <ItemGroup>
    <Compile Remove=""Loose.cs"" />
    <Compile Include=""nested\..\nested\Alias.cs"" />
    <Compile Include=""..\outside\Linked.cs"" Link=""Linked.cs"" />
  </ItemGroup>
</Project>
");
        }

        private static void Restore(string project)
        {
            var result = RunRestore(project);
            Assert.AreEqual(0, result.ExitCode, result.Text);
        }

        private static JsonDocument Evaluate(string project, params string[] extraArguments)
        {
            var directory = Path.GetDirectoryName(project);
            if (directory == null)
            {
                throw new InvalidOperationException(project);
            }

            return EvaluateIn(project, directory, extraArguments);
        }

        private static JsonDocument EvaluateIn(string project, string workingDirectory, params string[] extraArguments)
        {
            var arguments = new List<string>
            {
                "msbuild",
                project,
                "-target:" + Targets,
                "-getItem:Compile,AdditionalFiles",
                "-getProperty:EmDashAnalyzerSeverity,EmDashAnalyzerExcludes,SmartQuotesAnalyzerSeverity,ApostropheAnalyzerSeverity,EllipsisAnalyzerSeverity,MinusAnalyzerSeverity,NbspAnalyzerSeverity",
                "-nologo",
                "-verbosity:quiet",
            };
            arguments.AddRange(extraArguments);
            var result = RunArguments(arguments, workingDirectory);
            Assert.AreEqual(0, result.ExitCode, result.Text);
            return JsonDocument.Parse(result.StdOut);
        }

        private static ProcessResult RunRestore(string project)
        {
            var directory = Path.GetDirectoryName(project);
            if (directory == null)
            {
                throw new InvalidOperationException(project);
            }

            return RunArguments(new List<string> { "restore", project }, directory);
        }

        private static ProcessResult RunArguments(List<string> arguments, string workingDirectory)
        {
            var start = new ProcessStartInfo
            {
                FileName = "dotnet",
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            foreach (var argument in arguments)
            {
                start.ArgumentList.Add(argument);
            }

            using (var process = Process.Start(start))
            {
                if (process == null)
                {
                    throw new InvalidOperationException("dotnet");
                }

                var stdoutTask = process.StandardOutput.ReadToEndAsync();
                var stderrTask = process.StandardError.ReadToEndAsync();
                process.WaitForExit();
                return new ProcessResult(process.ExitCode, stdoutTask.Result, stderrTask.Result);
            }
        }

        private static void AssertNoCompileOverlap(JsonDocument document)
        {
            var compile = Paths(document, "Compile");
            var additional = Paths(document, "AdditionalFiles");
            var overlap = compile.Intersect(additional, StringComparer.OrdinalIgnoreCase).ToArray();
            Assert.AreEqual(0, overlap.Length, string.Join(", ", overlap));
        }

        private static int Count(JsonDocument document, string itemName, string fileName)
        {
            var suffix = Path.DirectorySeparatorChar + fileName;
            return Paths(document, itemName).Count(path => path.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
        }

        private static List<string> Paths(JsonDocument document, string itemName)
        {
            var paths = new List<string>();
            if (!document.RootElement.TryGetProperty("Items", out var items)
                || !items.TryGetProperty(itemName, out var list))
            {
                return paths;
            }

            foreach (var item in list.EnumerateArray())
            {
                if (item.TryGetProperty("FullPath", out var fullPath))
                {
                    var value = fullPath.GetString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        paths.Add(value);
                    }
                }
            }

            return paths;
        }

        private static string Property(JsonDocument document, string name)
        {
            var value = document.RootElement.GetProperty("Properties").GetProperty(name).GetString();
            if (value == null)
            {
                throw new InvalidOperationException(name);
            }

            return value;
        }

        private sealed class ProcessResult
        {
            public ProcessResult(int exitCode, string stdout, string stderr)
            {
                ExitCode = exitCode;
                StdOut = stdout;
                Text = stdout + stderr;
            }

            public int ExitCode { get; }

            public string StdOut { get; }

            public string Text { get; }
        }
    }
}
