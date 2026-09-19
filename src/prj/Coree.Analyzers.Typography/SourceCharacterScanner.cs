#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Coree.Analyzers.Typography
{
    internal static class SourceCharacterScanner
    {
        internal static void Register(
            AnalysisContext context,
            DiagnosticDescriptor warningRule,
            DiagnosticDescriptor errorRule,
            DiagnosticDescriptor infoRule,
            string characters,
            string severityPropertyName,
            string includesPropertyName,
            string excludesPropertyName,
            string additionalExcludesPropertyName)
        {
            context.RegisterCompilationStartAction(startContext =>
            {
                var options = startContext.Options.AnalyzerConfigOptionsProvider.GlobalOptions;
                var severity = AnalyzerSeverity.Read(options, severityPropertyName);
                if (!severity.HasValue)
                {
                    return;
                }

                var rule = AnalyzerSeverity.SelectDescriptor(
                    warningRule,
                    errorRule,
                    infoRule,
                    severity.Value);
                startContext.RegisterSyntaxTreeAction(treeContext =>
                    ReportEachMatch(treeContext, rule, characters));

                string includes;
                options.TryGetValue("build_property." + includesPropertyName, out includes);
                if (string.IsNullOrWhiteSpace(includes))
                {
                    return;
                }

                string excludes;
                options.TryGetValue("build_property." + excludesPropertyName, out excludes);
                string additionalExcludes;
                options.TryGetValue("build_property." + additionalExcludesPropertyName, out additionalExcludes);
                excludes = AdditionalFilePatterns.JoinPatterns(excludes, additionalExcludes);
                string projectDirectory;
                options.TryGetValue("build_property.MSBuildProjectDirectory", out projectDirectory);
                var compiledPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var tree in startContext.Compilation.SyntaxTrees)
                {
                    ProjectDirectoryTextFiles.AddSeenPath(compiledPaths, tree.FilePath);
                }

                startContext.RegisterAdditionalFileAction(fileContext =>
                    ReportEachMatch(
                        fileContext,
                        rule,
                        characters,
                        includes,
                        excludes,
                        projectDirectory,
                        compiledPaths));
            });
        }

        internal static SourceText OrEmpty(SourceText text)
        {
            if (text == null)
            {
                return SourceText.From(string.Empty);
            }

            return text;
        }

        internal static void ReportEachMatch(
            SyntaxTreeAnalysisContext context,
            DiagnosticDescriptor rule,
            string characters)
        {
            var tree = context.Tree;
            ReportEachMatch(
                tree.GetText(context.CancellationToken),
                span => Location.Create(tree, span),
                rule,
                characters,
                diagnostic => context.ReportDiagnostic(diagnostic));
        }

        internal static void ReportEachMatch(
            AdditionalFileAnalysisContext context,
            DiagnosticDescriptor rule,
            string characters,
            string includes,
            string excludes,
            string projectDirectory,
            HashSet<string> compiledPaths)
        {
            var file = context.AdditionalFile;
            if (ProjectDirectoryTextFiles.IsAlreadyCompiled(compiledPaths, file.Path))
            {
                return;
            }

            if (!AdditionalFilePatterns.IsSelected(file.Path, includes, excludes, projectDirectory))
            {
                return;
            }

            var text = OrEmpty(file.GetText(context.CancellationToken));
            if (ProjectDirectoryTextFiles.ContainsNul(text))
            {
                return;
            }

            ReportEachMatch(
                text,
                span => Location.Create(file.Path, span, text.Lines.GetLinePositionSpan(span)),
                rule,
                characters,
                diagnostic => context.ReportDiagnostic(diagnostic));
        }

        internal static void ReportEachMatch(
            SourceText text,
            Func<TextSpan, Location> createLocation,
            DiagnosticDescriptor rule,
            string characters,
            Action<Diagnostic> report)
        {
            var length = text.Length;
            for (var i = 0; i < length; i++)
            {
                if (characters.IndexOf(text[i]) < 0)
                {
                    continue;
                }

                var span = TextSpan.FromBounds(i, i + 1);
                report(Diagnostic.Create(rule, createLocation(span)));
            }
        }
    }
}
