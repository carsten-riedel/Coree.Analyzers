## Before

CCCPL002 tells a consumer to put a short parameter list on one line even when that list contains a line comment. The length is the parameter text after whitespace collapses, so this list is measured as if it fit:

```csharp
public void M(
    int // amplitude
    a,
    int b)
```

The measured spelling still contains `//`. Writing the list on one line comments out the rest of the signature. A comment between the parameters is left out of the length, and the same diagnostic still says the parameters fit:

```csharp
public void M(
    int a, // amplitude
    int b)
```

The one-line form the message shows, `M(a, b, c)`, does not keep that comment.

## Change

CCCPL002 stays quiet when the parameter list contains a single-line comment. The length calculation is unchanged, so a short broken list with no line comment still reports. A `//` inside a string is not a comment and still reports. A block comment can still report. The repository README, NuGet README, analyzer description, and package notes say this.

## After

The list with `// amplitude` inside the first parameter no longer reports CCCPL002. The list with `// amplitude` between the parameters no longer reports it either. A short broken list whose string default contains `http://example` still reports CCCPL002 and says the parameters fit in 44 characters, limit 120. A block comment inside a parameter still reports and says 28 characters. Lists that were already short and had no comment still report.

## Evidence

At `origin/main` `ab21473c4e2051b8b0cc2abcbe38ddb35e7f7c8d`, `ParameterListFit.Normalize` walks every character and turns each whitespace run into one space. A `//` comment inside a parameter is not whitespace, so it stays in the measured line. A `//` comment between parameters sits outside each parameter's span, so the length ignores it. `ShortBreaksReport` already expects CCCPL002 for a short list with no comment. Nothing in that suite placed a line comment in the list.

Before the fix, `ParameterListFitAnalyzerTests` with coverage collection disabled reported two comment failures and one invalid sample. The interior comment produced CCCPL002 at 25 characters, span `(1,17)-(5,2)`. The comment between parameters produced CCCPL002 at 12 characters, span `(1,17)-(4,2)`, which is `int a, int b` with the comment omitted. The first string sample also produced CCCPL002 at 36 characters, but it was invalid C#: the optional `string url` preceded required `int b` (`CS1737`). That sample was replaced by required `string scheme` followed by the optional URL. Nine other tests in that filtered run passed. After the fix, the same filtered class passed 13 tests. The full suite, package inspection, and commands are recorded below. Consumer benefit is not measured.

## Understood assignment

The user invoked `execute-product-evolution` with no preset and no phase-specific tailoring. A correction then said to follow this skill directly, to drop extra wait, topic, and branch-suffix instructions, and to continue the same run. Before any policy file was opened, Discover, Assess, and Select & Bound were each resolved to Conservative `Default.md`. That choice did not change. No Balanced or Ambitious profile was read.

## Discover — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicyDiscover/Contract.md` and `Default.md`.

Read both package introductions, the CodeClarity and Typography solution notes, the parameter-list domain note, and the analyzer implementations and tests. Refreshed `origin` and the local `feature/pevo/*` worktrees. GitHub issues and pull requests were empty.

These outcomes are already implemented or claimed and are not candidates: method line endings, `nameof` disambiguation, CCCPL002 literal whitespace (`feature/pevo/parameter-literals-261025`), unsigned right shift, Typography comma globs, the CCCPL001 closing-parenthesis end line (`feature/pevo/parameter-list-end-261025`), single quotation marks (`feature/pevo/single-quotes-261025`), a user-defined `operator +` (`feature/pevo/custom-addition-261025`), and CCCPL001 overlapping parameter spans (`feature/pevo/parameter-line-share-261025`). That last issue saw the line-comment length and left it out of its own change.

- A: CCCPL002 can recommend one line for a list that contains `//` (`ParameterListFit.cs`, `ParameterListFitAnalyzer` message). Collapsing the comment, or ignoring a comment between parameters, does not produce the one-line form the message shows.
- B: an empty Typography Includes value set before the package props import is replaced with `**`. A direct MSBuild evaluation showed an empty value before the import becomes `**`, an empty value after the import stays empty, and an omitted value becomes `**`. SDK `Microsoft.Common.props` imports `Directory.Build.props` before NuGet props, so a repo-wide empty value takes the first path. The project-file example in the README is the second path and still disables scanning.
- C: the package text says conditional operators count, while `&&` and `||` do not (`ReturnExpressionComplexity.cs`, the quiet `&&` test, domain clue S-4e18). The scorer was left unchanged after a maintainer question about `||`.

## Assess — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicyAssess/Contract.md` and `Default.md`.

- A removes a recommendation that cannot be followed without deleting the comment or commenting out the signature. Lists with no line comment keep today's warning. The check can look for comment trivia and leave the existing length calculation alone, so it does not take over the literal-whitespace change already underway in the same file. Rollback is a normal revert.
- B is a real import-order trap, but the documented project-file assignment still works. Turning the earlier empty value off requires a new non-empty setting, and the Typography registration tests are already being edited for comma globs. That is a wider contract change than A.
- C is real wording tension. The behavior is already locked, and a clarification would not stop a diagnostic that gives the wrong edit. It ranks below A.

A is the conservative choice: the misleading one-line advice is visible from the current measurement and message, and the repair does not add warnings.

## Select & Bound — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicySelectBound/Contract.md` and `Default.md`.

Select A. The result is that CCCPL002 stays quiet when a parameter list contains a single-line comment, and still reports a short broken list when `//` occurs only inside a string.

Include the fit check, focused regressions, and the repository README, NuGet README, analyzer description, and package notes that describe this exception. Exclude literal-whitespace measurement, block comments, preprocessor directives, CCCPL001, return scoring, Typography, severity, diagnostic IDs, the maximum, and package version changes.

A fresh branch and issue check before this worktree found no `parameter-line-comment` ref. `parameter-line-share-261025` claims overlapping parameter spans, not this outcome.

### Acceptance criteria

1. A short broken list does not report CCCPL002 when a `//` comment sits inside a parameter, or between parameters.
2. The same short shape still reports when the only `//` is inside a string default, and the message still states the measured length and the limit.
3. Existing short breaks, long lists, maximum handling, and severity handling stay put.
4. Show the comment cases failing against the original check, then pass the full CodeClarity suite with its configured 100% line, branch, and method coverage. Build and pack the package.
5. Repository and NuGet explanations, the analyzer description, and the package notes state the line-comment exception. Public IDs, the maximum, CCCPL001, and the other analyzers stay unchanged.

### Consumers, risk, and rollback

Consumers are C# developers who see CCCPL002 in the IDE or in a build. Lists that already warn and contain no line comment keep warning. Lists that warned only because a line comment was collapsed or ignored become quiet. A block comment can still be part of a short list and can still report.

The main risk is missing a comment that the compiler attaches outside the parameter-list trivia, or treating a `//` inside a string as a comment. The string case is the contrasting test. Rollback is reverting the implementation and the accompanying explanation. No configuration or data migration is required.

## Baseline and execution

- Status: Done. Implementation verified on 2026-10-25. Not merged. Publication of this closure commit, pull request, and worktree cleanup are pending.
- Integration target: `origin/main` at `ab21473c4e2051b8b0cc2abcbe38ddb35e7f7c8d`, refreshed on 2026-10-25.
- Feature branch: `feature/pevo/parameter-line-comment-261025`.
- Temporary ignored worktree: `.worktrees/parameter-line-comment-261025`.
- The issue-only baseline commit is `29c3fe5a3f7c0bbcad2b084e5ee321a3b1ef33a2`. It was published to `origin/feature/pevo/parameter-line-comment-261025` before product edits. `origin/main` stayed at `ab21473c4e2051b8b0cc2abcbe38ddb35e7f7c8d`.
- The implementation commit is `70ce095b03943b485f904748049751add50ba1d5`.
- This closure commit's publication, pull request, and worktree cleanup are not done yet.

## Verification and final outcome

Commands ran from `src/sln/Coree.Analyzers.CodeClarity` in this worktree.

- Red run, before the fix: `dotnet test --filter FullyQualifiedName~ParameterListFitAnalyzerTests -p:CollectCoverage=false --verbosity minimal -m:1`. The two line-comment cases failed as described in Evidence. The first URL sample was invalid C# and was replaced. Nine existing tests passed.
- After the fix, the same filtered command passed 13 tests, including the URL list at 44 characters and a block comment at 28 characters.
- `dotnet restore` was up to date.
- `dotnet build --no-restore -m:1` succeeded with 0 errors. The six warnings are the existing DebugHost samples: CCCMB001, CCCNS001, CCCRC001, two CCCPL002, and CCCPL001.
- `dotnet test --no-build --verbosity minimal` passed 47 tests, failed 0, skipped 0. Coverage of `Coree.Analyzers.CodeClarity` was 100% line, 100% branch, and 100% method.
- `dotnet pack --no-restore -m:1` after the implementation commit produced `Coree.Analyzers.CodeClarity.0.1.10-g70ce095b03.nupkg`. The package has the analyzer DLL and XML under `analyzers/dotnet/cs`, props under `build/` and `buildTransitive/`, and no `lib/` entry. The packed README contains the line-comment sentence, and the nuspec release notes contain the Unreleased CCCPL002 note.
- `git diff --check` on the implementation diff was clean.

No public diagnostic ID, severity, maximum, or other analyzer changed. CCCPL001 was not modified. Literal whitespace measurement was not modified. Expected consumer benefit remains unmeasured. Rollback is reverting `70ce095` together with this explanation.

## Incidental Discoveries (Out of Scope)

- `ProjectDirectoryTextFiles.ContainsNul` returns false when a NUL appears after the first 512 characters. `ContainsNulReadsOnlyTheFirst512Characters` locks that limit.
- Expression-bodied `=> a + b + c` is already expected to stay quiet in `ReturnTypeComplexityAnalyzerTests`.
