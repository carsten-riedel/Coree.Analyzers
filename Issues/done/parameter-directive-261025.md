## Before

CCCPL002 still tells a consumer to put a short parameter list on one line when that list contains a preprocessor directive. The length ignores the directive text, so this shape is treated as fitting:

```csharp
public void M(
    int a,
#if DEBUG
    int debugFlag,
#endif
    int b)
```

C# requires `#if`, `#endif`, `#region`, and the other directives to stand on their own lines. The one-line form the diagnostic shows, `M(a, b, c)`, cannot keep them. Following the warning means dropping the conditional parameter or producing illegal source. Line comments already stay quiet for that same reason. A concurrent unpublished branch claims block comments; it excludes preprocessor directives.

## Change

CCCPL002 now stays quiet when the parameter list contains preprocessor directive trivia (`SyntaxTrivia.IsDirective`). The length calculation is unchanged. A short broken list with no directive still reports. A `#if` sequence inside a string is not trivia and still reports. Repository README, NuGet README, analyzer description, domain note, and Unreleased package notes state this exception. Line-comment quieting and block-comment reporting on this baseline were left as they were.

## After

The list with `#if true` around `int debugFlag` no longer reports CCCPL002. The list with `#region flags` around `int b` no longer reports it either. A short broken list whose string default contains `"#if true"` still reports CCCPL002. `BlockCommentStillReports` still reports. Whether people actually keep those directives was not measured.

## Evidence

At `origin/main` `76107484db8079120328ea90626aeb60b18a31d1`, `ParameterListFit.ContainsLineComment` walked `DescendantTrivia` and returned true only for `SingleLineCommentTrivia`. Before the guard, `PreprocessorIfAroundParameterStaysQuiet` and `PreprocessorRegionAroundParameterStaysQuiet` each returned one CCCPL002 warning (27 and 19 characters, limit 120). Command, from `src/sln/Coree.Analyzers.CodeClarity`: `dotnet test --filter FullyQualifiedName~PreprocessorIfAroundParameterStaysQuiet|FullyQualifiedName~PreprocessorRegionAroundParameterStaysQuiet -p:CollectCoverage=false --verbosity minimal -m:1`. Result: Failed 2, Passed 0.

After the guard, from that same directory: `dotnet build -m:1` succeeded with 0 errors. `dotnet test --no-build` passed 103 tests with 100% line, branch, and method coverage. `dotnet pack --no-restore -m:1` produced `Coree.Analyzers.CodeClarity.0.1.21-gc3176ba206.nupkg` under `src/prj/Coree.Analyzers.CodeClarity/bin/Pack/`: analyzer under `analyzers/dotnet/cs`, props under `build/` and `buildTransitive/`, no `lib/` group. The packed README and nuspec notes mention the preprocessor exception. The first string sample used an optional parameter before a required one (`CS1737`) and was replaced by required `int b` followed by `string option = "#if true"`. Consumer benefit is not measured.

## Understood assignment and provenance

The user invoked `execute-product-evolution` with no named preset and no phase-specific tailoring. That authorizes one cohesive, bounded, verified increment, recorded in an issue before implementation, published on its own feature branch, and not merged. Before any policy file was opened, Discover, Assess, and Select & Bound were each resolved to Conservative `Default.md`. That choice did not change. No Balanced or Ambitious profile was read.

## Discover → Assess → Select & Bound — decision trail

### Discover — Default.md + Contract.md

Read both package introductions, the CodeClarity and Typography solution notes, the parameter-list domain note, analyzer implementations and tests, and the file-based issues already on `main`. Refreshed `origin`. GitHub issues and pull requests were empty. During the later overlap recheck, `origin/feature/pevo/parameter-block-comment-261025` appeared; it claims block-comment quiet for CCCPL002 and explicitly excludes preprocessor directives.

Already implemented or claimed and not counted as candidates: method line endings, `nameof` disambiguation, CCCPL002 literal whitespace, unsigned right shift, Typography exclude-glob splitting, CCCPL001 closing-line and shared-line measurement, single quotation marks, user-defined `operator +`, CCCPL002 line comments, and CCCPL002 block comments on that unpublished branch.

- A: CCCPL002 recommends one line for a list that contains `#if` / `#endif` (or other directive trivia) between parameters (`ParameterListFit.cs`). The recommended spelling cannot keep those directives.
- B: an empty Typography `Includes` value that is already empty when the package props import is replaced with `**`. A value written in the project body after import can stay empty. Distinguishing unset from empty in MSBuild is unresolved.
- C: `ProjectDirectoryTextFiles.ContainsNul` inspects only the first 512 characters. A test locks that limit.
- D: CCCRC001 counts `Foo()` and does not count `new Foo()`. The published text says method calls; constructors are a different syntax kind.
- E: CTYQM001 includes `« »` and omits single guillemets U+2039 and U+203A. The single-quote increment explicitly left those out.

`&&` / `||` scoring and expression-bodied returns were observed and left out of the candidate list: the scorer was left unchanged after a maintainer question, and `SingleCalculationStepIsSimple` already expects `=> a + b + c` to stay quiet.

### Assess — Default.md + Contract.md

- A removes a recommendation that cannot be followed without dropping the directive or writing illegal C#. Only lists with directive trivia become quiet. Verification is a focused analyzer test plus the existing CodeClarity suite. Rollback is a normal revert.
- B is a real import-order mismatch, but empty and omitted are the same `== ''` check, so a behavior fix risks the default `**` scan. It ranks below A.
- C is an explicit cap with a regression test. Expanding the scan is not clearly safer.
- D would add warnings and widens "method calls" to constructor-creation. That is an expansion, not a false diagnostic on today's text.
- E would add warnings for characters the last Typography increment excluded.

A is the conservative choice: same product reasoning as the line-comment exception, no new warnings, and a clear one-line impossibility.

### Select & Bound — Default.md + Contract.md

Select A. The result is that CCCPL002 stays quiet when a parameter list contains preprocessor directive trivia, and still reports a short broken list when it does not.

Include the fit check, focused regressions, and the repository README, NuGet README, analyzer description, parameter-list domain note, and Unreleased package note that describe this exception. Exclude line comments (already on `main`), block comments (claimed by `feature/pevo/parameter-block-comment-261025`), CCCPL001, return scoring, Typography, severity, diagnostic IDs, the maximum, and package version changes.

A fresh remote check before this worktree found no `parameter-directive` ref. `parameter-block-comment-261025` claims `/* */`, not this outcome.

### Acceptance criteria

1. A short broken list that contains `#if` / `#endif` around a parameter does not report CCCPL002.
2. A short broken list that contains `#region` / `#endregion` does not report CCCPL002.
3. The same short shape with no directive still reports, and `//` inside a string still reports.
4. Line-comment quiet cases stay quiet. Block-comment reporting on this baseline stays as it is.
5. Show the directive cases failing against the original check, then pass the full CodeClarity suite with its configured 100% line, branch, and method coverage. Build and pack the package.
6. The package notes and CodeClarity explanations state that a preprocessor directive keeps the list quiet.

### Consumers, risk, and rollback

Consumers are C# developers who see CCCPL002. Lists that already warn without directives keep warning. Lists that warned only because a directive was ignored become quiet. Diagnostic IDs, public configuration, and other analyzers stay unchanged. The main risk is treating a `#if` sequence inside a string as a directive; string text is not trivia, and the existing `http://example` case still reports. Rollback reverts the guard and the accompanying notes together. Real consumer impact is not measured.

## Selected improvement and baseline

- Status: Done. Implementation verified against the acceptance criteria. Not merged.
- Date: 2026-10-25.
- Integration target: `main`; refreshed `origin/main` at `76107484db8079120328ea90626aeb60b18a31d1`.
- Feature branch: `feature/pevo/parameter-directive-261025`.
- Temporary worktree: `.worktrees/parameter-directive-261025`, ignored through `.git/info/exclude`.
- Issue-only baseline: `c3176ba206ceeceac1658810c1d336264cac7a07`, published to `origin` before product edits.
- Implementation: `eb506cb058e81ec9215099921a49164fc810c983`.
- Publication of this closure commit, pull-request creation, merge, and worktree cleanup: pending until after this commit is pushed.

## Final outcome

The planned trivia guard was implemented without a scope change. Block comments still report on this baseline. Line-comment quiet cases stay quiet. No diagnostic id, severity, or maximum changed.

The HashIf string sample was invalid C# at first (`CS1737`) and was corrected before the passing suite run. That did not change the selected outcome.

Rollback remains reverting the guard, tests, and accompanying notes together. Lists that contain directive trivia stop receiving CCCPL002 at the consumer's configured severity. That is the correction, not a separate migration.

## Incidental Discoveries (Out of Scope)

- Expression-bodied `=> a + b + c` is already expected to stay quiet in `ReturnTypeComplexityAnalyzerTests`.
- `&&` and `||` are not CCCRC001 calculation steps. The domain note says the scorer was left unchanged after a question about `||`.
- Empty Typography Includes values set before the package props import can be replaced with `**`.
- `ContainsNul` reads only the first 512 characters.
