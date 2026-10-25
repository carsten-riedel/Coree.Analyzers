# Coree.Analyzers.CodeClarity

<!-- Maintenance note: Keep this README aligned with the Coree.Analyzers.CodeClarity section of the repository README. Use absolute NuGet/GitHub URLs here where the repository README can use repository-relative links. -->

[![NuGet Version](https://img.shields.io/nuget/v/Coree.Analyzers.CodeClarity?label=NuGet&logo=nuget)](https://www.nuget.org/packages/Coree.Analyzers.CodeClarity) [![NuGet Downloads](https://img.shields.io/nuget/dt/Coree.Analyzers.CodeClarity?label=Downloads&logo=nuget)](https://www.nuget.org/packages/Coree.Analyzers.CodeClarity) [![Build Status](https://img.shields.io/github/actions/workflow/status/carsten-riedel/Coree.Analyzers/cicd.yml?branch=main&label=build)](https://github.com/carsten-riedel/Coree.Analyzers/actions/workflows/cicd.yml) [![Targets](https://img.shields.io/badge/targets-netstandard2.0-512BD4?logo=dotnet&logoColor=white)](https://www.nuget.org/packages/Coree.Analyzers.CodeClarity) [![License](https://img.shields.io/github/license/carsten-riedel/Coree.Analyzers?logo=mit)](https://github.com/carsten-riedel/Coree.Analyzers/blob/main/LICENSE)

A return expression should stay directly readable. This package flags one that exceeds the allowed number of calculation steps. A parameter list has two shapes: the whole list on one line, parentheses included, or each parameter on its own line with the closing parenthesis on the following line. A type declared directly in a file must sit in a namespace. Consecutive methods in a type keep a blank line between them.

```bash
dotnet add package Coree.Analyzers.CodeClarity
```

Example in a project file. The version `0.1.2` is the one shown here:

```xml
<ItemGroup>
  <PackageReference Include="Coree.Analyzers.CodeClarity" Version="0.1.2">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
</ItemGroup>
```

## Diagnostics

| Id | What it flags |
| --- | --- |
| **CCCRC001** | A `return` or `yield return` expression whose calculation-step score exceeds `ReturnTypeComplexityAnalyzerMaximum` (default 1) |
| **CCCPL001** | A parameter list that is neither entirely on one line, parentheses included, nor one parameter per line with the closing parenthesis on the following line |
| **CCCPL002** | A parameter list that is broken across lines even though the parameters fit in `ParameterListFitAnalyzerMaximum` characters (default 120) |
| **CCCNS001** | A class, struct, record, interface, enum, or delegate declared outside a namespace |
| **CCCMB001** | A method with a body that follows another method with a body in the same type without a blank line between them |

Arithmetic other than string concatenation, method calls other than `nameof`, and conditional operators each count as one step. A user-defined `operator +` counts even when one operand is a string. Casts, signs, `await`, member access, and identifiers do not. Default severity is warning.

Only the C# `nameof` operator is exempt. A user-defined method named `nameof` (including `@nameof`) counts as a method call. For example, `return @nameof(a + b);` has two steps and reports at the default maximum of 1.

Each shift operator (`<<`, `>>`, and `>>>`) also counts as one step. At the default maximum, `return value >>> count` stays quiet, while `return value >>> (count + extra)` reports CCCRC001.

At the default maximum of 1, `return a + b` stays quiet, and so does `return total`. `return a + b + c` reports CCCRC001. `total` is a local; that return is only the name. A `yield return` expression uses that same score. `yield return a + b` stays quiet, and `yield return a + b + c` reports CCCRC001.

```csharp
int OneStep(int a, int b)
{
    return a + b;
}

int TwoSteps(int a, int b, int c)
{
    return a + b + c;
}

int FromLocal(int a, int b, int c)
{
    int total = a + b + c;
    return total;
}
```

CCCPL001 looks at methods, local functions, constructors, and primary constructors of classes, structs, and records. Calls, indexers, delegates, and lambdas stay out of it. An empty list and a single parameter stay quiet. The first two samples report CCCPL001. The other two do not: one line, parentheses included, or one parameter per line with the closing parenthesis on the following line. A parameter that began higher still shares a line when the next parameter sits on its last line, and that list reports CCCPL001. The same parameter stays quiet when the next one starts on a later line.

```csharp
public void M(
    float a,
    float b,
    float c)

public void M(
    float a, float b, float c)

public record TerrainProfile(float Amplitude, float FrequencyX, float FrequencyZ, float DetailAmplitude, float DetailX, float DetailZ);

public void M(
    float a,
    float b,
    float c
)
```

The closing parenthesis follows the last line of the last parameter. An attribute or a default may occupy earlier lines of that parameter. This stays quiet:

```csharp
public void M(
    int a,
    [NotNull]
    int b
)
```

This reports CCCPL001, because `)` is on that parameter's last line:

```csharp
public void M(
    int a,
    [NotNull]
    int b)
```

CCCPL002 is separate from that shape rule. The last sample stays within CCCPL001 and still reports CCCPL002, because the parameters would fit on one line. The length is the parameters joined by `, ` after tabs, line breaks, and repeated spaces collapse. Whitespace inside literals is kept. Parentheses are not counted. A list that contains a `//` comment stays quiet, because that comment cannot be kept on the one line the diagnostic recommends. A `//` inside a string is not a comment and can still report. `float a, float b, float c` is 25 characters, so it reports at the default of 120. The one-line record does not. A longer list that uses the same legal break stays quiet.

CCCNS001 reports a class, struct, record, interface, enum, or delegate that is not inside a namespace. Nested types are not reported on their own. The first sample reports CCCNS001. The block namespace and the file-scoped namespace do not.

```csharp
class Terrain { }

namespace Game
{
    class Terrain { }
}

namespace Game;

class Terrain { }
```

CCCMB001 reports the later of two methods with a body that are consecutive members of the same type and have no blank line between them. A whitespace-only line counts. A comment or documentation line does not. The first sample reports CCCMB001. The second does not. Constructors, fields, properties, operators, indexers, local functions, and methods without a body stay quiet.

Blank lines are recognized with all C# line endings: LF, CRLF, CR, next line (U+0085), line separator (U+2028), and paragraph separator (U+2029), including mixed endings within a file.

```csharp
public bool Walkable()
{
    return true;
}
private bool StaticSegmentClear()
{
    return true;
}

public bool Walkable()
{
    return true;
}

private bool StaticSegmentClear()
{
    return true;
}
```

## MSBuild properties

Set these on the consuming compile target. The shipped props make them compiler-visible. Severity: `warning`, `error`, `message`, or `off`. An empty, negative, or non-numeric maximum keeps that analyzer's default: 1 calculation step, or 120 characters for CCCPL002.

```xml
<PropertyGroup>
  <ReturnTypeComplexityAnalyzerSeverity>warning</ReturnTypeComplexityAnalyzerSeverity>
  <ReturnTypeComplexityAnalyzerMaximum>1</ReturnTypeComplexityAnalyzerMaximum>
  <ParameterListLayoutAnalyzerSeverity>warning</ParameterListLayoutAnalyzerSeverity>
  <ParameterListFitAnalyzerSeverity>warning</ParameterListFitAnalyzerSeverity>
  <ParameterListFitAnalyzerMaximum>120</ParameterListFitAnalyzerMaximum>
  <TypeNamespaceAnalyzerSeverity>warning</TypeNamespaceAnalyzerSeverity>
  <MethodBlankLineAnalyzerSeverity>warning</MethodBlankLineAnalyzerSeverity>
</PropertyGroup>
```

Repository: [https://github.com/carsten-riedel/Coree.Analyzers](https://github.com/carsten-riedel/Coree.Analyzers)

License: MIT
