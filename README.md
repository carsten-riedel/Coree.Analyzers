<p align="center">
  <img alt="Coree" src="https://raw.githubusercontent.com/carsten-riedel/Coree.Analyzers/main/assets/brand.png" width="128">
</p>

# Coree.Analyzers
.NET multi-analyzer repository. Each package is independently packable. **Coree.Analyzers.Typography** is the first analyzer; it will not necessarily be the last.

## Coree.Analyzers.Typography

Word processors leave em dashes, curly quotes, and typographic apostrophes in source. Compilers do not care. Reviewers do.

This package catches those characters at compile time — in C# and in the extra project files you actually ship. ASCII hyphens, quotes, and apostrophes stay silent. Default severity is warning; you decide whether that is noise, a gate, or off.

```bash
dotnet add package Coree.Analyzers.Typography
```

The nupkg is a development dependency: the analyzer under `analyzers/dotnet/cs`, plus `build/` and `buildTransitive/` props. There is no `lib/` group.

## Diagnostics

| Id | What it flags |
| --- | --- |
| **CTYED001** | Em dash (U+2014) |
| **CTYQM001** | Typographic quotation marks (curly quotes and guillemets) |
| **CTYAP001** | Typographic apostrophe / closing single quotation mark (U+2019) |

C# syntax trees are always in scope. Matching files under the project directory are added as `AdditionalFiles` so Visual Studio can bind the same diagnostics `dotnet build` prints. Paths that are neither a syntax tree nor an additional file pin to the `.csproj` in Visual Studio.

## MSBuild properties

Set these on the consuming compile target. The shipped props make them compiler-visible. One model: **Includes minus (Excludes + AdditionalExcludes)**. Severity: `warning`, `error`, `message`, or `off`. Includes are semicolon-separated globs relative to the project directory. Empty includes turn extra-file scanning off for that analyzer. C# syntax trees stay in scope either way.

Setting `Excludes` **replaces** the default list; it does not merge. Empty or omitted `Excludes` keeps the defaults. `AdditionalExcludes` is always added on top (unset adds nothing). `bin`, `obj`, `.git`, and `.vs` are always excluded (build output, Git metadata, Visual Studio cache — not source), even if you omit them from `Excludes`.

Matched extra files are read from disk. Leading and trailing spaces, tabs, and newlines on `Excludes` and `AdditionalExcludes` are trimmed, so the lists may sit on their own lines in the csproj.

Defaults (you do not have to set these):

```xml
<PropertyGroup>
  <!-- warning: matched extra files are read from disk. -->
  <EmDashAnalyzerSeverity>warning</EmDashAnalyzerSeverity>
  <SmartQuotesAnalyzerSeverity>warning</SmartQuotesAnalyzerSeverity>
  <ApostropheAnalyzerSeverity>warning</ApostropheAnalyzerSeverity>
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
