using System;

namespace Coree.Analyzers.CodeClarity.DebugHost
{
    internal static class Program
    {
        private static void Main()
        {
            // Make Coree.Analyzers.CodeClarity the Visual Studio startup project and start debugging from there (F5).
            // Select the Coree.Analyzers.CodeClarity Roslyn Component launch profile. Do not F5 this console.
            // Visual Studio needs the .NET Compiler Platform SDK component.
            // F5 on this console only runs Main; it does not attach to the analyzer.

            // Change ReturnTypeComplexityAnalyzerSeverity (warning, error, message, or off)
            // and ReturnTypeComplexityAnalyzerMaximum (allowed calculation steps; default 1).
            // InlinedHeight reports CCCRC001. NamedHeight does not.
            Console.WriteLine(InlinedHeight(1f, 2f));
            Console.WriteLine(NamedHeight(1f, 2f));
        }

        private static float InlinedHeight(float x, float z)
        {
            const float amplitude = 1f;
            const float frequencyX = 0.1f;
            const float frequencyZ = 0.1f;
            return amplitude * MathF.Sin(x * frequencyX) * MathF.Cos(z * frequencyZ);
        }

        private static float NamedHeight(float x, float z)
        {
            float baseWave = MathF.Sin(x * 0.1f);
            float heightVariation = MathF.Cos(z * 0.1f);
            float groundHeight = 1f * baseWave * heightVariation;
            return groundHeight;
        }
    }
}
