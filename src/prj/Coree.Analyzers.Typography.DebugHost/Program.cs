using System;

namespace Coree.Analyzers.Typography.DebugHost
{
    internal static class Program
    {
        private static void Main()
        {
            // Make Coree.Analyzers.Typography the Visual Studio startup project and start debugging from there (F5).
            // Select the Coree.Analyzers.Typography Roslyn Component launch profile. Do not F5 this console.
            // Visual Studio needs the .NET Compiler Platform SDK component.
            // F5 on this console only runs Main; it does not attach to the analyzer.

            // Change EmDashAnalyzerSeverity / SmartQuotesAnalyzerSeverity / ApostropheAnalyzerSeverity
            // (warning, error, message, or off).
            // Change EmDashAnalyzerIncludes / SmartQuotesAnalyzerIncludes / ApostropheAnalyzerIncludes,
            // EmDashAnalyzerExcludes / SmartQuotesAnalyzerExcludes / ApostropheAnalyzerExcludes,
            // and the matching AdditionalExcludes properties
            // (Includes minus Excludes plus AdditionalExcludes; setting Excludes replaces
            // the default list; AdditionalExcludes is always added; empty includes skip
            // extra files; bin/obj/.git/.vs always stay out).
            // ASCII hyphen, quotes, and apostrophe do not report.
            Console.WriteLine("1-2");
            Console.WriteLine("\"hello\"");
            Console.WriteLine("it's");

            // Em dash reports CTYED001; typographic quotes report CTYQM001;
            // typographic apostrophe reports CTYAP001.
            Console.WriteLine("1—2");
            Console.WriteLine("“hello”");
            Console.WriteLine("it’s");
        }
    }
}
