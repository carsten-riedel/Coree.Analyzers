# Coree.Analyzers.CodeClarity

This folder is the per-package area under `src/sln/` for solution-level or cross-project files that should not sit next to a single `.csproj`.
The `.slnx` lives here; keep the folder while that is true. You can still add extra solution items here.

The `.slnx` and this readme live in this folder. Open a terminal here for the commands below. The CLI finds the one solution in this directory; you do not pass a `.slnx` or `.csproj` path. Other packages keep their own `.slnx` under `src/sln/<name>/`, so `dotnet` does not ask you to specify a solution.

```text
./                         you are here (this readme + Coree.Analyzers.CodeClarity.slnx)
../../prj/Coree.Analyzers.CodeClarity/    packable analyzer package (netstandard2.0)
../../prj/Coree.Analyzers.CodeClarity/.config/dotnet-tools.json  empty local tool manifest
../../prj/Coree.Analyzers.CodeClarity.Tests/  tests (not packed)
../../prj/Coree.Analyzers.CodeClarity.DebugHost/  Visual Studio F5 compile target (not packed)
```

Package metadata, license, icon, and release notes live in `src/prj/Coree.Analyzers.CodeClarity/Properties/NugetMetadata/`.

`--tl:off` is optional. Without it the CLI shows the compact terminal logger. Add `--tl:off` for the classic per-project log. The commands work either way.

## Restore and build

```bash
dotnet restore
dotnet build
```

## Test

The test project explicitly configures MSTest for method-level parallel execution within one test assembly. Tests must therefore not share mutable global state.

```bash
dotnet test
```

After a test run, the links below point to generated reports for the test host TFM.

[Test results (trx)](../../prj/Coree.Analyzers.CodeClarity.Tests/MSTestResults/Coree.Analyzers.CodeClarity.Tests-net10.0.trx)
[Test results (html)](../../prj/Coree.Analyzers.CodeClarity.Tests/MSTestResults/result-net10.0.html)
[Coverlet output](../../prj/Coree.Analyzers.CodeClarity.Tests/CoverletOutput/coverage.opencover.xml)

Coverlet measures only the analyzer assembly (`[Coree.Analyzers.CodeClarity]*`) and fails `dotnet test` if line, branch, or method coverage is under 100%.

## Pack

```bash
dotnet pack
```

Creates one `.nupkg` in `src/prj/Coree.Analyzers.CodeClarity/bin/Pack/` with the analyzer under `analyzers/dotnet/cs` (not `lib/`) and `Coree.Analyzers.CodeClarity.props` under `build/` and `buildTransitive/` (`ReturnTypeComplexityAnalyzerSeverity`, `ReturnTypeComplexityAnalyzerMaximum`, `ParameterListLayoutAnalyzerSeverity`). Test, DebugHost, and optional benchmark projects are not packed.

Restore fails this analyzer package on high (`NU1903`) and critical (`NU1904`) vulnerable packages. Low and moderate stay warnings. `NugetReport` next to the tests lists that package's packages (txt/json) and is still info-only.

## Publish

The analyzer package sets `IsPublishable` to `false`. Distribution is `dotnet pack`. To write output to `src/prj/Coree.Analyzers.CodeClarity/bin/Publish/` for a one-off inspect, set `IsPublishable` to `true` and run:

```bash
dotnet publish
```

This is a Roslyn analyzer, not an executable. Consumers install the nupkg; they do not publish this project.

## CI

Use `-m:1` for the build so a pipeline does not depend on machine load. It avoids occasional file locks when the analyzer is built as a solution project and as a test `ProjectReference` at the same time. Multi-target test execution is already configured as parallel in the test project. Run these commands from this folder so each package has exactly one `.slnx` in the working directory.

```bash
dotnet restore
dotnet build --no-restore -m:1
dotnet test --no-build
dotnet pack
```

## Debug (Visual Studio)

To break in the analyzer, install the **.NET Compiler Platform SDK** Visual Studio component, then:

1. Set `Coree.Analyzers.CodeClarity` as the startup project (not `Coree.Analyzers.CodeClarity.DebugHost`).
2. Select the `Coree.Analyzers.CodeClarity` launch profile (Roslyn Component).
3. Set a breakpoint in `ReturnTypeComplexityAnalyzer` or `ParameterListLayoutAnalyzer`.
4. Press F5. Visual Studio compiles `Coree.Analyzers.CodeClarity.DebugHost` and attaches to that compilation.

`Coree.Analyzers.CodeClarity.DebugHost` is only the compile target. `InlinedHeight` reports CCCRC001; `NamedHeight` does not. `MixedProfile` reports CCCPL001; `PackedProfile` does not. F5 / `dotnet run` on the console only runs `Main`; it does not attach to the analyzer.

Severity is an MSBuild property on the compile target (`ReturnTypeComplexityAnalyzerSeverity`, `ParameterListLayoutAnalyzerSeverity`): `warning` (default), `error`, `message`, or `off`. `ReturnTypeComplexityAnalyzerMaximum` is the allowed number of calculation steps in a return expression; the default is 1. An empty or invalid maximum keeps that default. The analyzer nupkg ships `build/` and `buildTransitive/` props so PackageReference consumers get the same knobs. DebugHost imports that props file because it uses a project analyzer reference, not the nupkg.

For stepping without F5, debug `ReturnTypeComplexityAnalyzerTests` from Test Explorer.

`DebugHost` is not packed. `dotnet pack` still produces only the analyzer nupkg.
