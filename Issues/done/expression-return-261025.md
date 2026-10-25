# Score expression-bodied returned values

## Before

A method can hand back a calculation that is too dense to read, and CodeClarity stays silent when that calculation is written with `=>`. This method produces no CCCRC001 diagnostic at the default maximum of 1:

```csharp
int Height(int a, int b, int c) => a + b + c;
```

The same expression in a block body does report. `return a + b + c` is two additions, which is over the maximum. Writing the method with `=>` removes the warning without making the calculation easier to read. `int Height(int a, int b) => a + b` is one addition and should stay quiet, the same way `return a + b` does.

The same escape exists for a non-void local function, a property, an indexer, a get accessor, an operator, and a conversion operator. A void method such as `void Log(int a, int b, int c) => Write(a + b + c)` has no returned value. The block form of that call does not report, and the arrow form should stay quiet too.

## Change

CCCRC001 now scores an arrow expression when that expression is the member's returned value, using the same rules as `return` and `yield return`. That covers a non-void method, a non-void local function, a property, an indexer, a get accessor, an operator, and a conversion operator. An `async` method whose return type displays as non-generic `System.Threading.Tasks.Task` or `System.Threading.Tasks.ValueTask` stays quiet, because that arrow is not a returned value. Void members, constructors, setters, lambdas, and anonymous functions stay quiet. The default maximum stays 1. `=> a + b` stays quiet. `=> a + b + c` reports.

The repository README, the CodeClarity NuGet readme, the package description, the analyzer description, the return-complexity note, and the unreleased package notes say so.

## After

`int Height(int a, int b, int c) => a + b + c` now reports CCCRC001 at the default maximum, on `a + b + c`. `int Height(int a, int b) => a + b` stays quiet. The same two-step expression reports for a non-void local function, a property, an indexer, a get accessor, an operator, a conversion operator, `async Task<int>`, and `async ValueTask<int>`. A void arrow, an `async Task`, an `async ValueTask`, a lambda, a constructor arrow, and a setter arrow stay quiet. `=> nameof(C)` stays quiet at maximum 0. A block `return a + b + c` and a `yield return a + b + c` still report. Writing the returned calculation with `=>` no longer removes the warning. Whether that warning changes real projects was not measured.

## Evidence

Before the change, at `origin/main` `085bac71d656d7740ec728cb13137750cd183fc9`, `ReturnTypeComplexityAnalyzer.Initialize` registered only `ReturnStatement` and `YieldReturnStatement`. `SingleCalculationStepIsSimple` expected `int Body(int a, int b, int c) => a + b + c` to produce no diagnostic. The return-complexity note said an expression-bodied member was not visited. Refreshed `origin` had only `main`. No open pull request or remote branch claimed this scoring.

After the change, from `src/sln/Coree.Analyzers.CodeClarity`: `dotnet build --no-restore -m:1` succeeded with 0 errors. The DebugHost warnings are the existing sample diagnostics, including CCCRC001 on the block-bodied `InlinedHeight`. `dotnet test --no-build` passed 111 tests. Coverlet reported 100% line, branch, and method coverage of `Coree.Analyzers.CodeClarity`. `ExpressionBodiedReturnedValuesReport` and `ExpressionBodiedNonValuesAndSingleStepsStayQuiet` cover the cases in After. `dotnet pack` then succeeded and wrote `src/prj/Coree.Analyzers.CodeClarity/bin/Pack/Coree.Analyzers.CodeClarity.1.0.2-g0e00105b9d.nupkg`. That version is the local Git height of the implementation commit, not a published release. Real consumer uptake was not measured.

## Understood assignment and provenance

The user invoked product evolution with the named Balanced preset and no narrower product goal. That authorizes one cohesive increment: record it before implementation, implement it on an isolated feature branch, verify it, publish the branch, and clean up the temporary worktree. No pull request or merge was requested.

Before any policy file was opened, Discover, Assess, and Select & Bound were each set to `Balanced.md`, because the invocation named that preset and did not tailor a phase. No later instruction changed those sources.

## Discover — PolicyDiscover/Balanced.md + PolicyDiscover/Contract.md

Refreshed `origin` and the pull-request list. Already implemented or claimed outcomes were not counted: yield-return scoring, `nameof` disambiguation, user-defined `operator +`, unsigned right shift, method blank-line endings, Typography exclude globs, single quotation marks, CCCPL001 closing-line and shared-line measurement, CCCPL002 literal whitespace, line comments, and preprocessor directives.

`&&` and `||` still do not count. A maintainer question left that scorer unchanged, so counting them was not treated as an open gap.

- A: An expression-bodied member whose arrow is a returned value is not scored. `int Body(int a, int b, int c) => a + b + c` stays quiet in `SingleCalculationStepIsSimple`, while `return a + b + c` reports. The return-complexity note says moving the calculation to `=>` still removes the warning. Evidence: `ReturnTypeComplexityAnalyzer.cs`, `docs/codeclarity/user/return-expression-complexity.md`.
- B: CCCPL002 can tell a consumer to put a parameter list on one line when a parameter literal itself contains a line break, as in the verbatim default `@"one` / `two"` already used by `MultilineParameterOnItsOwnLinesStaysQuiet`. `ParameterListFit` stays quiet only for a `//` comment or a preprocessor directive. A line break inside a literal cannot be kept on one line. Evidence: `ParameterListFit.cs`, `ParameterListLayoutAnalyzerTests.cs`.
- C: CCCPL001 and CCCPL002 register methods, local functions, constructors, and primary constructors. They do not register operators or conversion operators. The published declaration list does not name operators either. Evidence: `ParameterListLayoutAnalyzer.cs`, `ParameterListFitAnalyzer.cs`, `docs/agent-domain-understanding/context/parameter-list-layout.md`.
- D: CTYED001 reports only U+2013 and U+2014. U+2010, U+2011, U+2012, and U+2015 are other dash-punctuation characters and are not in `EmDashCharacters`. The diagnostic text names the en dash and the em dash only. No in-repo source shows those other characters occurring. Evidence: `EmDashAnalyzer.cs`, README diagnostics table.

## Assess — PolicyAssess/Balanced.md + PolicyAssess/Contract.md

- A: The evidence is direct, and the practical hole is the ordinary `=>` form of the return this package already flags. New warnings match the block-bodied case, including warning-as-error builds. Earlier increments left this silence in place and called it the checked boundary of those increments; the return-complexity note still describes it as a way to remove the warning. The work is moderate: reuse the existing score, and keep void and non-value arrows quiet so they do not warn when the block form would not.
- B: The false one-line recommendation is real and close to the comment and directive fixes. It is smaller than A. Multiline string defaults are a narrower encounter than expression-bodied members.
- C: Applying the parameter-list rules to operators is feasible, but the published rule names a declaration set that does not include them, and operator lists are a less common reading problem. New warnings would extend that rule rather than close a stated hole.
- D: Adding more dashes would change a diagnostic whose text and table name only the en dash and the em dash. There is no observed occurrence here. That is a weaker basis than A or B.

A has the strongest combination of a demonstrated consumer escape and a bounded repair. B remains a credible smaller alternative. C and D stay eligible but expand a scope the package currently names.

## Select & Bound — PolicySelectBound/Balanced.md + PolicySelectBound/Contract.md

A is the increment. Expression-bodied members are the normal spelling of the return CCCRC001 exists to keep readable, and the package text already tells a reader that moving a calculation there removes the warning. B is a real false recommendation, but it applies to a less common literal. C and D would widen rules whose published text already limits their targets. A final check of `origin` and the pull-request list found no branch or request for this scoring.

Included: the return analyzer's decision of which arrow expressions are returned values, regressions for the reporting and quiet cases below, and the README, NuGet readme, package description, analyzer description, return-complexity note, and unreleased package notes.

Excluded: B, C, D, `&&` and `||`, lambdas and anonymous functions, constructors, setters, void members, non-generic `async Task` and `async ValueTask` arrows, parameter lists, Typography, severity, the maximum, the diagnostic id, and package version changes.

Acceptance criteria:

1. `int Height(int a, int b, int c) => a + b + c` reports CCCRC001 at the default maximum, on that expression. `int Height(int a, int b) => a + b` stays quiet.
2. The same two-step expression reports for a non-void local function, a property, an indexer, a get accessor, an operator, and a conversion operator.
3. A void arrow, an `async` method that returns non-generic `Task`, a lambda, a constructor arrow, and a setter arrow stay quiet.
4. `return a + b + c` and `yield return a + b + c` still report. `=> nameof(C)` stays quiet at maximum 0.
5. The consumer texts named above say that a returned arrow expression uses the same score, and they name the quiet cases in criterion 3.
6. From `src/sln/Coree.Analyzers.CodeClarity`: `dotnet restore`, `dotnet build --no-restore -m:1`, `dotnet test --no-build`, and `dotnet pack` succeed, with 100% line, branch, and method coverage of the analyzer assembly.

Consumers are C# developers who see CCCRC001 in the IDE or in a build. Block-bodied returns and `yield return` stay as they are. Expression-bodied returned values that already fit the maximum stay quiet. Those over the maximum start to warn. A build that treats warnings as errors can fail on those members. Other analyzers are unchanged.

Rollback is reverting this branch. No configuration migration is required.

## Selected improvement and baseline

- Integration target: `main`. Refreshed base: `origin/main` at `085bac71d656d7740ec728cb13137750cd183fc9`.
- Feature branch: `feature/pevo/expression-return-261025`.
- Worktree: `.worktrees/expression-return-261025`, ignored by `.git/info/exclude`.
- Issue path at baseline: `Issues/open/expression-return-261025.md`.
- Issue-only baseline commit: `1b3e8a592160a7cba6d15c8fa0e7f80bb3d41d6b`, published to `origin/feature/pevo/expression-return-261025` before product edits.
- Implementation commit: `0e00105b9db4a7e0152f6057fcc02fcd01bddfbf`.

## Final outcome

Status: Done. The implementation is verified and is not merged.

`ReturnTypeComplexityAnalyzer` scores a returned arrow with the existing `ReturnExpressionComplexity` rules. The plan's member set was implemented as registered syntax kinds rather than one shared switch. `async Task<int>` and `async ValueTask<int>` report, which is the generic case already implied by scoring returned values. No acceptance criterion was dropped.

Verification is the build, the 111 passing tests, 100% coverage, and the pack result in Evidence. Consumer benefit was not measured outside those tests. Rollback is reverting `0e00105b9db4a7e0152f6057fcc02fcd01bddfbf` together with this closure.

Publication of this Done closure, pull-request creation, merge, and worktree cleanup are not done yet. The remote branch currently points at the issue-only baseline.

## Incidental Discoveries (Out of Scope)

- `&&` and `||` still add nothing to a return score. That was noticed again in `ReturnExpressionComplexity.IsCalculation` and in the existing domain clue. The maintainer previously left the scorer unchanged. Whether that should stay the rule was not reopened.
