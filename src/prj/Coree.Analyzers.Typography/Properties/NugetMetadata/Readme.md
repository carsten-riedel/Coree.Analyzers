# Coree.Analyzers

<!-- Maintenance note: Keep this README aligned with the repository README for shared prose, examples, headings, badges, and feature descriptions. Use absolute NuGet/GitHub URLs here where the repository README can use repository-relative links; otherwise keep shared content in sync. -->

.NET multi-analyzer repository. Each package is independently packable. **Coree.Analyzers.Typography** and **Coree.Analyzers.CodeClarity** are the first two; they will not necessarily be the last.

## Coree.Analyzers.Typography

[![NuGet Version](https://img.shields.io/nuget/v/Coree.Analyzers.Typography?label=NuGet&logo=nuget)](https://www.nuget.org/packages/Coree.Analyzers.Typography) [![NuGet Downloads](https://img.shields.io/nuget/dt/Coree.Analyzers.Typography?label=Downloads&logo=nuget)](https://www.nuget.org/packages/Coree.Analyzers.Typography) [![Build Status](https://img.shields.io/github/actions/workflow/status/carsten-riedel/Coree.Analyzers/cicd.yml?branch=main&label=build)](https://github.com/carsten-riedel/Coree.Analyzers/actions/workflows/cicd.yml) [![Targets](https://img.shields.io/badge/targets-netstandard2.0-512BD4?logo=dotnet&logoColor=white)](https://www.nuget.org/packages/Coree.Analyzers.Typography) [![License](https://img.shields.io/github/license/carsten-riedel/Coree.Analyzers?logo=mit)](https://github.com/carsten-riedel/Coree.Analyzers/blob/main/LICENSE)

Word processors leave en/em dashes, curly quotes, typographic apostrophes, ellipses, minus signs, and no-break spaces in source. Compilers do not care. Reviewers do.

This package catches those characters at compile time, in C# and in the extra project files you actually ship. ASCII hyphens, quotes, apostrophes, periods, and spaces stay silent. Default severity is warning; you decide whether that is noise, a gate, or off.

```bash
dotnet add package Coree.Analyzers.Typography
```

The nupkg is a development dependency: the analyzer under `analyzers/dotnet/cs`, plus `build/` and `buildTransitive/` props. There is no `lib/` group.

## Diagnostics

| Id | What it flags |
| --- | --- |
| **CTYED001** | En dash (U+2013) and em dash (U+2014) |
| **CTYQM001** | Typographic quotation marks (curly quotes and guillemets) |
| **CTYAP001** | Typographic apostrophe / closing single quotation mark (U+2019) |
| **CTYEL001** | Horizontal ellipsis (U+2026) |
| **CTYMN001** | Minus sign (U+2212) |
| **CTYNB001** | No-break space (U+00A0) and narrow no-break space (U+202F) |

C# syntax trees are always in scope. Matching files under the project directory are added as `AdditionalFiles` so Visual Studio can bind the same diagnostics `dotnet build` prints. Paths that are neither a syntax tree nor an additional file pin to the `.csproj` in Visual Studio.

## MSBuild properties

Set these on the consuming compile target. The shipped props make them compiler-visible. One model: **Includes minus (Excludes + AdditionalExcludes)**. Severity: `warning`, `error`, `message`, or `off`. Includes are semicolon-separated globs relative to the project directory. Empty includes turn extra-file scanning off for that analyzer. C# syntax trees stay in scope either way.

Setting `Excludes` **replaces** the default list; it does not merge. Empty or omitted `Excludes` keeps the defaults. `AdditionalExcludes` is always added on top (unset adds nothing). `bin`, `obj`, `.git`, and `.vs` are always excluded (build output, Git metadata, Visual Studio cache, not source), even if you omit them from `Excludes`.

Matched extra files are read from disk. Leading and trailing spaces, tabs, and newlines on `Excludes` and `AdditionalExcludes` are trimmed, so the lists may sit on their own lines in the csproj.

Defaults (you do not have to set these):

```xml
<PropertyGroup>
  <!-- warning: matched extra files are read from disk. -->
  <EmDashAnalyzerSeverity>warning</EmDashAnalyzerSeverity>
  <SmartQuotesAnalyzerSeverity>warning</SmartQuotesAnalyzerSeverity>
  <ApostropheAnalyzerSeverity>warning</ApostropheAnalyzerSeverity>
  <EllipsisAnalyzerSeverity>warning</EllipsisAnalyzerSeverity>
  <MinusAnalyzerSeverity>warning</MinusAnalyzerSeverity>
  <NbspAnalyzerSeverity>warning</NbspAnalyzerSeverity>
  <EmDashAnalyzerIncludes>**</EmDashAnalyzerIncludes>
  <EmDashAnalyzerExcludes>
    bin\**;obj\**;.git\**;.vs\**;**\*.dll;**\*.exe;**\*.pdb;**\*.png;**\*.jpg;**\*.jpeg;**\*.gif;**\*.bmp;**\*.ico;**\*.nupkg;**\*.snupkg;**\*.zip;**\*.7z;**\*.snk;**\*.woff;**\*.woff2;**\*.ttf;**\*.eot;**\*.otf;**\*.pdf;**\*.resources;**\*.cache;**\*.suo;**\*.user;**\*.bin;**\*.so;**\*.dylib;**\*.winmd;**\*.db;**\*.sqlite;**\*.sqlite3;**\*.dat;**\*.pfx;**\*.cer;**\*.p12;**\*.wasm
  </EmDashAnalyzerExcludes>
  <SmartQuotesAnalyzerIncludes>**</SmartQuotesAnalyzerIncludes>
  <SmartQuotesAnalyzerExcludes>
    bin\**;obj\**;.git\**;.vs\**;**\*.dll;**\*.exe;**\*.pdb;**\*.png;**\*.jpg;**\*.jpeg;**\*.gif;**\*.bmp;**\*.ico;**\*.nupkg;**\*.snupkg;**\*.zip;**\*.7z;**\*.snk;**\*.woff;**\*.woff2;**\*.ttf;**\*.eot;**\*.otf;**\*.pdf;**\*.resources;**\*.cache;**\*.suo;**\*.user;**\*.bin;**\*.so;**\*.dylib;**\*.winmd;**\*.db;**\*.sqlite;**\*.sqlite3;**\*.dat;**\*.pfx;**\*.cer;**\*.p12;**\*.wasm
  </SmartQuotesAnalyzerExcludes>
  <ApostropheAnalyzerIncludes>**</ApostropheAnalyzerIncludes>
  <ApostropheAnalyzerExcludes>
    bin\**;obj\**;.git\**;.vs\**;**\*.dll;**\*.exe;**\*.pdb;**\*.png;**\*.jpg;**\*.jpeg;**\*.gif;**\*.bmp;**\*.ico;**\*.nupkg;**\*.snupkg;**\*.zip;**\*.7z;**\*.snk;**\*.woff;**\*.woff2;**\*.ttf;**\*.eot;**\*.otf;**\*.pdf;**\*.resources;**\*.cache;**\*.suo;**\*.user;**\*.bin;**\*.so;**\*.dylib;**\*.winmd;**\*.db;**\*.sqlite;**\*.sqlite3;**\*.dat;**\*.pfx;**\*.cer;**\*.p12;**\*.wasm
  </ApostropheAnalyzerExcludes>
  <EllipsisAnalyzerIncludes>**</EllipsisAnalyzerIncludes>
  <EllipsisAnalyzerExcludes>
    bin\**;obj\**;.git\**;.vs\**;**\*.dll;**\*.exe;**\*.pdb;**\*.png;**\*.jpg;**\*.jpeg;**\*.gif;**\*.bmp;**\*.ico;**\*.nupkg;**\*.snupkg;**\*.zip;**\*.7z;**\*.snk;**\*.woff;**\*.woff2;**\*.ttf;**\*.eot;**\*.otf;**\*.pdf;**\*.resources;**\*.cache;**\*.suo;**\*.user;**\*.bin;**\*.so;**\*.dylib;**\*.winmd;**\*.db;**\*.sqlite;**\*.sqlite3;**\*.dat;**\*.pfx;**\*.cer;**\*.p12;**\*.wasm
  </EllipsisAnalyzerExcludes>
  <MinusAnalyzerIncludes>**</MinusAnalyzerIncludes>
  <MinusAnalyzerExcludes>
    bin\**;obj\**;.git\**;.vs\**;**\*.dll;**\*.exe;**\*.pdb;**\*.png;**\*.jpg;**\*.jpeg;**\*.gif;**\*.bmp;**\*.ico;**\*.nupkg;**\*.snupkg;**\*.zip;**\*.7z;**\*.snk;**\*.woff;**\*.woff2;**\*.ttf;**\*.eot;**\*.otf;**\*.pdf;**\*.resources;**\*.cache;**\*.suo;**\*.user;**\*.bin;**\*.so;**\*.dylib;**\*.winmd;**\*.db;**\*.sqlite;**\*.sqlite3;**\*.dat;**\*.pfx;**\*.cer;**\*.p12;**\*.wasm
  </MinusAnalyzerExcludes>
  <NbspAnalyzerIncludes>**</NbspAnalyzerIncludes>
  <NbspAnalyzerExcludes>
    bin\**;obj\**;.git\**;.vs\**;**\*.dll;**\*.exe;**\*.pdb;**\*.png;**\*.jpg;**\*.jpeg;**\*.gif;**\*.bmp;**\*.ico;**\*.nupkg;**\*.snupkg;**\*.zip;**\*.7z;**\*.snk;**\*.woff;**\*.woff2;**\*.ttf;**\*.eot;**\*.otf;**\*.pdf;**\*.resources;**\*.cache;**\*.suo;**\*.user;**\*.bin;**\*.so;**\*.dylib;**\*.winmd;**\*.db;**\*.sqlite;**\*.sqlite3;**\*.dat;**\*.pfx;**\*.cer;**\*.p12;**\*.wasm
  </NbspAnalyzerExcludes>
</PropertyGroup>
```

Override what you need. Keep the defaults and skip extra folders:

```xml
<PropertyGroup>
  <!-- warning: matched extra files are read from disk. -->
  <EmDashAnalyzerAdditionalExcludes>**\docs\**</EmDashAnalyzerAdditionalExcludes>
</PropertyGroup>
```

Replace the default exclude list to scan `.dat` / `.resources`, and fail the build on em dashes. Copy the default `EmDashAnalyzerExcludes` value and delete only `**\*.dat` and `**\*.resources` (those two slots are marked):

```xml
<PropertyGroup>
  <!-- warning: matched extra files are read from disk. -->
  <EmDashAnalyzerSeverity>error</EmDashAnalyzerSeverity>
  <EmDashAnalyzerExcludes>
    bin\**;obj\**;.git\**;.vs\**;**\*.dll;**\*.exe;**\*.pdb;**\*.png;**\*.jpg;**\*.jpeg;**\*.gif;**\*.bmp;**\*.ico;**\*.nupkg;**\*.snupkg;**\*.zip;**\*.7z;**\*.snk;**\*.woff;**\*.woff2;**\*.ttf;**\*.eot;**\*.otf;**\*.pdf;<!-- **\*.resources removed -->;**\*.cache;**\*.suo;**\*.user;**\*.bin;**\*.so;**\*.dylib;**\*.winmd;**\*.db;**\*.sqlite;**\*.sqlite3;<!-- **\*.dat removed -->;**\*.pfx;**\*.cer;**\*.p12;**\*.wasm
  </EmDashAnalyzerExcludes>
</PropertyGroup>
```


## Coree.Analyzers.CodeClarity

[![NuGet Version](https://img.shields.io/nuget/v/Coree.Analyzers.CodeClarity?label=NuGet&logo=nuget)](https://www.nuget.org/packages/Coree.Analyzers.CodeClarity) [![NuGet Downloads](https://img.shields.io/nuget/dt/Coree.Analyzers.CodeClarity?label=Downloads&logo=nuget)](https://www.nuget.org/packages/Coree.Analyzers.CodeClarity) [![Build Status](https://img.shields.io/github/actions/workflow/status/carsten-riedel/Coree.Analyzers/cicd.yml?branch=main&label=build)](https://github.com/carsten-riedel/Coree.Analyzers/actions/workflows/cicd.yml) [![Targets](https://img.shields.io/badge/targets-netstandard2.0-512BD4?logo=dotnet&logoColor=white)](https://www.nuget.org/packages/Coree.Analyzers.CodeClarity) [![License](https://img.shields.io/github/license/carsten-riedel/Coree.Analyzers?logo=mit)](https://github.com/carsten-riedel/Coree.Analyzers/blob/main/LICENSE)

A return expression should stay directly readable. This package flags one that exceeds the allowed number of calculation steps.

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

### Diagnostics

| Id | What it flags |
| --- | --- |
| **CCCRC001** | A `return` expression whose calculation-step score exceeds `ReturnTypeComplexityAnalyzerMaximum` (default 1) |

Arithmetic other than string concatenation, method calls other than `nameof`, and conditional operators each count as one step. Casts, signs, `await`, member access, and identifiers do not. Default severity is warning.

At the default maximum of 1, `return a + b` stays quiet, and so does `return total`. `return a + b + c` reports CCCRC001. `total` is a local; that return is only the name.

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

### MSBuild properties

Set these on the consuming compile target. The shipped props make them compiler-visible. Severity: `warning`, `error`, `message`, or `off`. An empty, negative, or non-numeric maximum keeps the default of 1.

```xml
<PropertyGroup>
  <ReturnTypeComplexityAnalyzerSeverity>warning</ReturnTypeComplexityAnalyzerSeverity>
  <ReturnTypeComplexityAnalyzerMaximum>1</ReturnTypeComplexityAnalyzerMaximum>
</PropertyGroup>
```

Repository: [https://github.com/carsten-riedel/Coree.Analyzers](https://github.com/carsten-riedel/Coree.Analyzers)

License: MIT
