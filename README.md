# Coree.Analyzers.Typography

.NET multi-analyzer repository. The GitHub landing page is this file.
Each package's `.slnx` and notes live under `src/sln/Coree.Analyzers.Typography/`. CLI commands for this package start in that folder.

```text
./                         repository root (this README)
src/sln/Coree.Analyzers.Typography/      this package's .slnx and notes
src/prj/Coree.Analyzers.Typography/      packable analyzer package (netstandard2.0)
src/prj/Coree.Analyzers.Typography/AnalyzerPackage/  consumer MSBuild props (packed to build/ and buildTransitive/)
src/prj/Coree.Analyzers.Typography/Properties/Build/  MSBuild targets (not source)
src/prj/Coree.Analyzers.Typography/Properties/NugetMetadata/  NuGet metadata (readme, icon, notes)
src/prj/Coree.Analyzers.Typography/.config/dotnet-tools.json  empty local tool manifest
src/prj/Coree.Analyzers.Typography.Tests/  tests (not packed)
src/prj/Coree.Analyzers.Typography.DebugHost/  Visual Studio F5 compile target (not packed)
```

Open a terminal in `src/sln/Coree.Analyzers.Typography/` and run `dotnet restore`, `dotnet build`, `dotnet test`, or `dotnet pack`. The CLI finds the one solution in that folder; you do not pass a `.slnx` or `.csproj` path.

`src/prj/Coree.Analyzers.Typography/.config/dotnet-tools.json` is an empty local tool manifest. Run `dotnet tool install --local` from that project folder.
The default solution-folder working directory does not see this file.
Command details and layout notes: [src/sln/Coree.Analyzers.Typography/Readme.md](src/sln/Coree.Analyzers.Typography/Readme.md).
