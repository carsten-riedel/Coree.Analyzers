## Before

An iterator can hand back a calculation that is too dense to read, and CodeClarity stays silent. This method produces no CCCRC001 diagnostic at the default maximum of 1:

```csharp
int Sequence(int a, int b, int c)
{
    yield return a + b + c;
}
```

The same expression in an ordinary method does report. `return a + b + c` is two additions, which is over the maximum. Moving that expression into `yield return` removes the warning without making the calculation easier to read. `yield return a + b` is one addition and should stay quiet, the same way `return a + b` does.

## Change

`ReturnTypeComplexityAnalyzer` now scores the expression of a `yield return` with the same rules already used for `return`. Arithmetic other than string concatenation, method calls other than `nameof`, the conditional operator `?:`, shift operators, and a user-defined `operator +` each still count as one step. The default maximum stays 1. Roslyn 4.14 represents that statement as `YieldStatementSyntax`. A `yield return` with no expression stays quiet. `yield break` is not registered. Expression-bodied members are unchanged.

Regressions cover the two-addition iterator above, a one-addition `yield return` that stays quiet, `yield return nameof(...)` at maximum 0, and `yield break`. The repository README, the NuGet readme, the package description, the analyzer description, and the unreleased package notes say that a `yield return` expression is included.

## After

The iterator in Before now reports CCCRC001 at the default maximum of 1, on `a + b + c`. `yield return a + b` stays quiet at that default. `yield return nameof(C)` stays quiet at maximum 0. `yield break` stays quiet. An ordinary `return a + b + c` still reports, and an expression-bodied member such as `=> a + b + c` still does not. A developer who moved a dense calculation into an iterator no longer loses the warning. Whether that warning changes real projects was not measured.

## Evidence

At `origin/main` `76107484db8079120328ea90626aeb60b18a31d1`, `ReturnTypeComplexityAnalyzer.Initialize` registered only `SyntaxKind.ReturnStatement`. Before the registration change, `TwoStepYieldReturnReports` failed: expected 1 diagnostic, actual 0. Command, from `src/sln/Coree.Analyzers.CodeClarity`: `dotnet test --filter FullyQualifiedName~TwoStepYieldReturnReports -p:CollectCoverage=false --verbosity minimal -m:1`.

After the change, `dotnet build --no-restore -m:1` succeeded with 0 errors and the 6 existing DebugHost sample warnings. `dotnet test -m:1` passed 104 tests. Coverlet reported 100% line, branch, and method coverage of `Coree.Analyzers.CodeClarity`. `dotnet pack --no-restore -m:1` wrote `src/prj/Coree.Analyzers.CodeClarity/bin/Pack/Coree.Analyzers.CodeClarity.0.1.21-ga3c4736d64.nupkg` with the analyzer under `analyzers/dotnet/cs`, props under `build/` and `buildTransitive/`, the readme, and no `lib/` group. The nuspec text includes `yield return`. Implementation commit: `7b77f21f3e94af50d5d0c0d63ad42d7b2ac190b4`. Consumer benefit remains unmeasured.

## Understood assignment

The user invoked `execute-product-evolution` with no preset and no phase-specific tailoring. Before any policy file was opened, Discover, Assess, and Select & Bound were each resolved to Conservative `Default.md`. That choice did not change. No Balanced or Ambitious profile was read.

## Discover — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicyDiscover/Contract.md` and `Default.md`.

Read both package introductions, the CodeClarity solution notes, the analyzer implementations and tests, and the issue history on `main`. Refreshed `origin`. GitHub issues and pull requests were empty. These outcomes were already implemented or claimed and were not candidates: method line endings, `nameof` disambiguation, CCCPL002 literal whitespace, unsigned right shift, Typography exclude globs, CCCPL001 closing-line and shared-line measurement, single quotation marks, a user-defined `operator +`, CCCPL002 line comments, and CCCPL002 block comments (`origin/feature/pevo/parameter-block-comment-261025`). A fresh check also found `origin/feature/pevo/parameter-directive-261025`, whose closure says CCCPL002 stays quiet for a preprocessor directive. That outcome is claimed and is not a candidate here.

- A: `yield return a + b + c` is not scored, because the analyzer never visits `YieldReturnStatement`. Evidence: `ReturnTypeComplexityAnalyzer.cs`. The same expression on `return` reports in `NestedCallAndSecondOperatorAreTooComplex`.
- B: an empty Typography `Includes` value set in `Directory.Build.props`, before the package props import, is replaced with `**`. The README says an empty includes value turns extra-file scanning off. A value written in the project body, after that import, can still stay empty. An earlier MSBuild evaluation recorded this import order in `Issues/done/parameter-line-comment-261025.md`. Unset and empty look the same when the props are imported.
- C: the package text says conditional operators count as a CCCRC001 step, while `&&` and `||` do not. Evidence: `ReturnExpressionComplexity.cs`, the quiet `a > b && a >= b` case, domain clue S-4e18. The scorer was left unchanged after a maintainer question about `||`.
- D: the CodeClarity release notes never say that CCCNS001 exists. The 0.1.5 entry stops at CCCRC001 and CCCPL001. The unreleased notes mention later CCCPL and CCCMB fixes, not this diagnostic. The README already describes CCCNS001.

## Assess — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicyAssess/Contract.md` and `Default.md`.

- A is a missed application of the existing score. The expression scorer, the maximum, and the message can stay. Iterators whose yield expression is already within the maximum stay quiet. Iterators over the maximum start to warn, including projects that treat warnings as errors. Rollback is a normal revert. The change does not sit in the parameter-list files the other open evolutions edit.
- B is a real import-order trap. Honoring an early empty value needs a way to tell "unset, so use `**`" from "set empty, so scan nothing". Those two states are the same string when the props import. A sentinel would change the documented empty-means-off rule that the project file already relies on. A note alone would not stop the early empty value from scanning.
- C would either change a score the maintainer left in place, or only restate the score the test already locks. It does not correct a warning that tells someone to make an edit they cannot make.
- D is a changelog gap. The consumer explanation of CCCNS001 is already in the README and the NuGet readme. A release-note line does not change what the analyzer reports.

A is the conservative choice: the iterator expression is visible in source, the intended score already exists, and the check can be reused without a new diagnostic id.

## Select & Bound — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicySelectBound/Contract.md` and `Default.md`.

Select A. The result is that CCCRC001 scores a `yield return` expression the same way it scores a `return` expression.

Include the return analyzer's syntax registration, regressions for the two-step, one-step, and `nameof` yield cases, and the README, NuGet readme, analyzer description, and unreleased package notes. Exclude expression-bodied members, `&&` and `||`, parameter lists, Typography, severity, the maximum, the diagnostic id, and package version changes.

The overlap recheck before this worktree found no `yield-return` ref. `parameter-directive-261025` and `parameter-block-comment-261025` claim the parameter-list exceptions, not this outcome.

### Acceptance criteria

1. `yield return a + b + c` reports CCCRC001 at the default maximum of 1, on the expression.
2. `yield return a + b` does not report at that default.
3. `yield return nameof(...)` does not report at maximum 0.
4. Ordinary `return` scoring, severity, and the maximum stay as they are. Expression-bodied members stay quiet.
5. Show the two-step yield producing no diagnostic against the original analyzer, then pass the full CodeClarity suite with its configured 100% line, branch, and method coverage. Build and pack the package.
6. The repository README, NuGet readme, analyzer description, and package notes say a `yield return` expression is scored. The diagnostic id, severity, and maximum stay unchanged.

### Consumers, risk, and rollback

Consumers are C# developers who see CCCRC001 in the IDE or in a build. Iterator expressions that already fit the maximum stay quiet. Iterator expressions over the maximum start to warn. A build that treats warnings as errors can fail on those iterators. Expression-bodied members, ordinary returns, and the other analyzers are unchanged.

The main risk is treating `yield break`, or a `yield return` that has no expression yet, as a value to score. Those forms have no expression and stay quiet. Rollback is reverting the registration, the tests, and the notes. No configuration or data migration is required.

## Baseline and execution

- Status: Done. Implementation verified on 2026-10-25. Not merged.
- Integration target: `main`, refreshed `origin/main` at `76107484db8079120328ea90626aeb60b18a31d1`.
- Feature branch: `feature/pevo/yield-return-261025`.
- Worktree: `.worktrees/yield-return-261025`.
- Issue-only baseline: `a3c4736d641cc0e6dbd4e81770595c31000b22c1`, published to `origin/feature/pevo/yield-return-261025` before product edits.
- Implementation: `7b77f21f3e94af50d5d0c0d63ad42d7b2ac190b4`.
- Publication of this closure commit, pull request, merge, and worktree cleanup are pending.

## Final outcome

CCCRC001 now applies the existing score to a `yield return` expression. The plan did not change. `yield break` and expression-bodied members stayed out, as bounded. Verification is the build, the 104 passing tests, 100% coverage, and the package inspection above. Real consumer uptake was not measured. Rollback is reverting `7b77f21f3e94af50d5d0c0d63ad42d7b2ac190b4` together with this closure.

## Incidental Discoveries (Out of Scope)

- `ProjectDirectoryTextFiles.ContainsNul` inspects only the first 512 characters, and `ContainsNulReadsOnlyTheFirst512Characters` asserts that a later NUL is ignored. No package sentence says the whole file is scanned. Whether that bound hides binary files was not established.
- `int Body(int a, int b, int c) => a + b + c` stays quiet in `SingleCalculationStepIsSimple`. That expression-bodied silence is the checked contract, separate from `yield return`.
- Single guillemets U+2039 and U+203A were left out of the quotation-mark change on purpose (`Issues/done/single-quotes-261025.md`).
