## Before

CCCPL002 warns when a parameter list is broken across lines even though the parameters would still fit on one line. The message tells the author to write that list on one line, as in `M(a, b, c)`.

A default can be a verbatim or raw string that itself contains a line break. The length check keeps that break, because it is part of the literal, but it still treats the list as fitting when the character count is within `ParameterListFitAnalyzerMaximum` (120 by default) and the list has no `//` comment and no preprocessor directive. This list is only a short parameter text once formatting space is collapsed, and it still reports:

```csharp
void M(
    int limit,
    string query = @"select
        id")
```

The recommended one-line spelling cannot keep that string. A line break inside a block comment is different: the comment can sit on one line, and that list still reports. An escaped `\n` in a regular string is still one source line and can still report.

## Change

CCCPL002 stays quiet when a parameter token's source text contains a C# line break: CR, LF, next line (U+0085), line separator (U+2028), or paragraph separator (U+2029). The check uses the source spelling, so an escaped `\n` in a regular string is still one line. Formatting whitespace still collapses. A block comment can still sit on one line, and a short wrapped list without a literal line break still reports. The repository README, the NuGet readme, the parameter-list user page, the diagnostic description, and the package notes say so.

## After

The list in Before no longer reports CCCPL002. The line break in `@"select` / `id"` stays in the string, and the diagnostic does not ask for a one-line spelling that would remove it. A short wrapped list such as `float a, float b, float c` still reports. `string value = "select\nid"` still reports, because that `\n` is two source characters, not a line break. A multiline block comment still reports, because that comment can be written on one line. Whether fewer projects now fail a warning-as-error build was not measured.

## Evidence

Before the change, `ParameterListFit.Normalize` appended each token's own text and `UsesBreakButFits` still returned true when that length was within the maximum. Observed on `main` at `085bac71d656d7740ec728cb13137750cd183fc9`.

After the change, from `src/sln/Coree.Analyzers.CodeClarity`: `dotnet test -m:1` passed 118 tests, with Coverlet reporting 100% line, branch, and method coverage of `Coree.Analyzers.CodeClarity`. `dotnet build --no-restore -m:1` succeeded with 0 errors and the 6 existing DebugHost sample warnings. `dotnet pack --no-restore -m:1` wrote `src/prj/Coree.Analyzers.CodeClarity/bin/Pack/Coree.Analyzers.CodeClarity.1.0.1-g6a1405d804.nupkg`. The packed readme and nuspec notes include the literal line-break exception. `LiteralLineBreakStaysQuiet` covers verbatim, raw, and interpolated forms and the five line-break characters. `EscapedNewlineInsideRegularStringStillReports` still warns. `MultilineBlockCommentStillReports` still warns. Implementation commit: `c12b3bf812b8ecd57cfb0b6aea85c147688162f1`. Consumer benefit remains unmeasured.

## Understood assignment and provenance

The user invoked execute-product-evolution and named Balanced. That was read as the Balanced preset for Discover, Assess, and Select & Bound, with no separate phase instruction. Authorization for this cycle is that invocation. No consumer interview or usage count was available.

## Discover → Assess → Select & Bound — decision trail

### Discover (`Balanced.md` + `Contract.md`)

Open and completed local issues, GitHub issues, pull requests, and remote branch names were checked as candidates appeared. Nothing else was implementing these outcomes. Remote heads were only `main`.

- A: CCCPL002 can tell an author to put a parameter list on one line when a verbatim or raw string in that list contains a real line break. Evidence: `ParameterListFit.cs`, the quiet paths for `//` and directives, and the single-line literal tests.
- B: `int M(int a, int b, int c) => a + b + c` does not report CCCRC001, while the same expression on `return` does. The user page states that moving the calculation to an expression-bodied member removes the warning. Evidence: `ReturnTypeComplexityAnalyzer.cs` registers only `return` and `yield return`; `SingleCalculationStepIsSimple` expects the arrow form to stay quiet.
- C: The package text says conditional operators count as a CCCRC001 step. The scorer counts `?:` and does not count `&&` or `||`. A maintainer question about `||` left the scorer unchanged. Evidence: `ReturnExpressionComplexity.cs`, the quiet `a > b && a >= b` case, domain clue S-4e18.
- D: `return new Foo(Bar())` scores only the inner call, so it stays within the default maximum, while `return Bar(Baz())` reports. The published rule names method calls, not constructors. Evidence: `ReturnExpressionComplexity.cs` increments `InvocationExpressionSyntax` only.
- E: An empty Typography `Includes` value that is already empty when the package props import is replaced with `**`. A value written in the project body after that import can still stay empty and turn extra-file scanning off, which is the path the README shows. Evidence: `EmDashAnalyzer.props` and the same condition in the other analyzer props.
- F: The CodeClarity install example and both projects' dogfood references still say `0.1.2`. NuGet lists `1.0.1` for that package. The 1.0 versioning note recorded that those references were left as they were.

### Assess (`Balanced.md` + `Contract.md`)

- A: The warning's own advice cannot be followed without changing the literal. The same promise already stays quiet for a `//` comment and a preprocessor directive. This removes a wrong warning; it does not add one. The check is local to parameter tokens and can be verified with the existing analyzer tests.
- B: This is the larger hole in the return rule, and expression-bodied members are common. It would add warnings where the tests and the user page currently promise silence, across methods, local functions, properties, indexers, accessors, and operators. That compatibility cost is wider than A.
- C: The mismatch is real, but the scorer was already left unchanged on purpose. A wording fix would be safe and smaller than A. It would not change what the analyzer reports.
- D: Counting `new` would make ordinary `return new Point(a + b)` report at the default maximum of 1. The published wording does not call a constructor a calculation step. The likely noise is higher than the evidence that this is a defect.
- E: The documented project-file path already disables scanning. An earlier empty value is indistinguishable from an omitted value at the props condition, so a small change cannot honor both "omitted means scan" and "empty in `Directory.Build.props` means do not scan."
- F: Copying the sample installs an old package. Bumping the dogfood reference to `1.0.1` can also fail the build under `error` severity and pull in source edits outside one outcome. The versioning change had just left `0.1.2` in place.

A has the clearer defect, the safer compatibility result, and a bounded check. B would touch more code, but it changes a documented quiet case. C, E, and F are real and were not selected.

### Select & Bound (`Balanced.md` + `Contract.md`)

A is selected. The result is that CCCPL002 stays quiet when a parameter token contains a C# line break, and still reports a short wrapped list that can actually be written on one line.

Included: the fit decision, regressions for verbatim and raw strings and for the C# line-break characters, the existing short-list and single-line-literal cases, and the README, NuGet readme, user page, diagnostic description, and package notes that describe this exception.

Excluded: CCCPL001, block comments, escaped `\n` in a regular string, expression-bodied members, `&&` and `||`, constructors, Typography includes, the `0.1.2` pin, severity, the maximum, the diagnostic id, and package version changes.

A fresh fetch before this issue found only `origin/main` at `085bac71d656d7740ec728cb13137750cd183fc9`. No issue, pull request, or branch claimed this outcome.

## Selected improvement and baseline

Consumers are CodeClarity users, including projects that treat warnings as errors. The diagnostic id, default severity, and default maximum stay. A list that truly fits still reports. A list that reports today only because of a literal line break stops reporting. That is a narrower warning, not a new rule.

Acceptance:

- A wrapped list whose verbatim or raw string contains a line break does not report at the default maximum.
- Each C# line break inside a literal is covered: CR, LF, CRLF, U+0085, U+2028, and U+2029.
- A short wrapped list of ordinary parameters still reports.
- Spaces and tabs inside a single-line literal still count, and that wrapped list still reports.
- A `//` comment, a preprocessor directive, and a block comment keep their current results.
- An escaped `\n` in a regular string can still report.
- The consumer texts named above describe the literal line-break exception.
- `dotnet test` for CodeClarity passes with 100% line, branch, and method coverage of the analyzer assembly.

Rollback is reverting this branch. Integration target is `main`. Local branch: `feature/pevo/literal-line-break-261025`. Worktree: `.worktrees/literal-line-break`. Base: `origin/main` at `085bac71d656d7740ec728cb13137750cd183fc9`. Issue baseline `6a1405d8049435dc000b1e4d3a7f3d7e65b5d229` is on `origin/feature/pevo/literal-line-break-261025`. Implementation is `c12b3bf812b8ecd57cfb0b6aea85c147688162f1`. Publication of this closure commit, review, and cleanup are not done yet.

## Final outcome

Status: Done. The implementation is verified and ready for review. It is not merged.

The sample in Before now puts `int limit` before the string default. The first test draft used the other order and failed with CS1737; that order is invalid C# and was not a product change. The quiet result does not depend on which parameter holds the string.

`dotnet test -m:1` passed 118 tests with 100% line, branch, and method coverage. Build had 0 errors. Pack included the updated readme and release notes. CCCPL001 was not modified. Rollback is reverting `c12b3bf812b8ecd57cfb0b6aea85c147688162f1` together with this closure.

The move to `Issues/done/` is commit `7a73a2508bc25104351e23283cfde6fcfa223e24`. That commit did not include this updated wording, so the verified outcome is recorded in the following issue commit. Publication of that commit, review, and cleanup are pending. The follow-up commit SHA is not written here.

## Incidental Discoveries (Out of Scope)

- `ProjectDirectoryTextFiles.ContainsNul` returns false when the first NUL is after 512 characters. `ContainsNulReadsOnlyTheFirst512Characters` locks that bound. No file in this run was shown to warn because of it.
- CTYQM001 does not include single guillemets U+2039 and U+203A. The single-quote change explicitly left them out, and the published character list does not name them.
