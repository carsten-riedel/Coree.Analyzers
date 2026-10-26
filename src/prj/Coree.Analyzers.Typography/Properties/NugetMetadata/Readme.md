# Coree.Analyzers.Typography

<!-- Maintenance note: Keep this README aligned with the Coree.Analyzers.Typography section of the repository README. Use absolute NuGet/GitHub URLs here where the repository README can use repository-relative links. -->

[![NuGet Version](https://img.shields.io/nuget/v/Coree.Analyzers.Typography?label=NuGet&logo=nuget)](https://www.nuget.org/packages/Coree.Analyzers.Typography) [![NuGet Downloads](https://img.shields.io/nuget/dt/Coree.Analyzers.Typography?label=Downloads&logo=nuget)](https://www.nuget.org/packages/Coree.Analyzers.Typography) [![Build Status](https://img.shields.io/github/actions/workflow/status/carsten-riedel/Coree.Analyzers/cicd.yml?branch=main&label=build)](https://github.com/carsten-riedel/Coree.Analyzers/actions/workflows/cicd.yml) [![Targets](https://img.shields.io/badge/targets-netstandard2.0-512BD4?logo=dotnet&logoColor=white)](https://www.nuget.org/packages/Coree.Analyzers.Typography) [![License](https://img.shields.io/github/license/carsten-riedel/Coree.Analyzers?logo=mit)](https://github.com/carsten-riedel/Coree.Analyzers/blob/main/LICENSE)

Word processors leave en/em dashes, curly quotes, typographic apostrophes, ellipses, minus signs, and no-break spaces in source. Compilers do not care. Reviewers do.

This package catches those characters at compile time, in C# and in the extra project files you actually ship. ASCII hyphens, quotes, apostrophes, periods, and spaces stay silent. Default severity is warning; you decide whether that is noise, a gate, or off.

In a C# document, the editor can replace the flagged character with that ASCII character. An en dash, em dash, or minus sign becomes a hyphen. A curly double quote or guillemet becomes a straight double quote. A single quotation mark or typographic apostrophe becomes an apostrophe. An ellipsis becomes three periods. A no-break space becomes a normal space. Inside a normal or interpolated string, a straight double quote is written as `\"`. Inside a verbatim string, it is written as `""`. Inside a character literal, an apostrophe is written as `\'`. A comment keeps the raw character. A raw string (`"""`) is left unchanged, and the warning still reports. A `u8` suffix follows that string: `"..."u8` escapes a double quote as `\"`, `@"..."u8` writes it as `""`, and `"""..."""u8` is left unchanged. The fix runs only when you apply it. One diagnostic can be fixed for the whole document, project, or solution. Extra files that are not C# documents still report and are not edited.

```bash
dotnet add package Coree.Analyzers.Typography
```

The nupkg is a development dependency: the analyzer under `analyzers/dotnet/cs`, plus `build/` and `buildTransitive/` props. There is no `lib/` group.

## Diagnostics

| Id | What it flags |
| --- | --- |
| **CTYED001** | En dash (U+2013) and em dash (U+2014) |
| **CTYQM001** | Curly double quotes, guillemets, and single quotation marks U+2018, U+201A, and U+201B |
| **CTYAP001** | Typographic apostrophe / closing single quotation mark (U+2019) |
| **CTYEL001** | Horizontal ellipsis (U+2026) |
| **CTYMN001** | Minus sign (U+2212) |
| **CTYNB001** | No-break space (U+00A0) and narrow no-break space (U+202F) |

CTYQM001 reports the opening single quotation mark (U+2018), the single low-9 quotation mark (U+201A), and the single high-reversed-9 quotation mark (U+201B), along with the curly double quotes and guillemets. The closing single quotation mark (U+2019) stays on CTYAP001 only, so that character is not reported twice.

C# syntax trees are always in scope. Matching files under the project directory are added as `AdditionalFiles` so Visual Studio can bind the same diagnostics `dotnet build` prints. A file already in `Compile` stays on the syntax tree and is not registered again. A `.cs` file that is not compiled is still an additional file when the globs match. Paths that are neither a syntax tree nor an additional file pin to the `.csproj` in Visual Studio.

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

Repository: [https://github.com/carsten-riedel/Coree.Analyzers](https://github.com/carsten-riedel/Coree.Analyzers)

License: MIT
