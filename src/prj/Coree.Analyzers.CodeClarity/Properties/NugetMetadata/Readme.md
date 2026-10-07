# Coree.Analyzers.CodeClarity

<!-- Maintenance note: Keep this README aligned with the Coree.Analyzers.CodeClarity section of the repository README. Use absolute NuGet/GitHub URLs here where the repository README can use repository-relative links. -->

[![NuGet Version](https://img.shields.io/nuget/v/Coree.Analyzers.CodeClarity?label=NuGet&logo=nuget)](https://www.nuget.org/packages/Coree.Analyzers.CodeClarity) [![NuGet Downloads](https://img.shields.io/nuget/dt/Coree.Analyzers.CodeClarity?label=Downloads&logo=nuget)](https://www.nuget.org/packages/Coree.Analyzers.CodeClarity) [![Build Status](https://img.shields.io/github/actions/workflow/status/carsten-riedel/Coree.Analyzers/cicd.yml?branch=main&label=build)](https://github.com/carsten-riedel/Coree.Analyzers/actions/workflows/cicd.yml) [![Targets](https://img.shields.io/badge/targets-netstandard2.0-512BD4?logo=dotnet&logoColor=white)](https://www.nuget.org/packages/Coree.Analyzers.CodeClarity) [![License](https://img.shields.io/github/license/carsten-riedel/Coree.Analyzers?logo=mit)](https://github.com/carsten-riedel/Coree.Analyzers/blob/main/LICENSE)

A return expression should stay directly readable. This package flags one that exceeds the allowed number of calculation steps.

```bash
dotnet add package Coree.Analyzers.CodeClarity
```

The nupkg is a development dependency: the analyzer under `analyzers/dotnet/cs`, plus `build/` and `buildTransitive/` props. There is no `lib/` group.

## Diagnostics

| Id | What it flags |
| --- | --- |
| **CCCRC001** | A `return` expression whose calculation-step score exceeds `ReturnTypeComplexityAnalyzerMaximum` (default 1) |

Arithmetic other than string concatenation, method calls other than `nameof`, and conditional operators each count as one step. Casts, signs, `await`, member access, and identifiers do not. Default severity is warning.

## MSBuild properties

Set these on the consuming compile target. The shipped props make them compiler-visible. Severity: `warning`, `error`, `message`, or `off`. An empty, negative, or non-numeric maximum keeps the default of 1.

```xml
<PropertyGroup>
  <ReturnTypeComplexityAnalyzerSeverity>warning</ReturnTypeComplexityAnalyzerSeverity>
  <ReturnTypeComplexityAnalyzerMaximum>1</ReturnTypeComplexityAnalyzerMaximum>
</PropertyGroup>
```

Repository: [https://github.com/carsten-riedel/Coree.Analyzers](https://github.com/carsten-riedel/Coree.Analyzers)

License: MIT
