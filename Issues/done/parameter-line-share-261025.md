## Before

CCCPL001 is quiet for this parameter list. The second parameter sits on the same line as the end of the first, and `)` is on the following line. The second parameter has a default so the signature compiles:

```csharp
public void M(
    string value = @"one
two", int other = 1
)
```

The same silence happens when a multiline attribute ends on the line that also starts the next parameter. The packed shape already warns when both parameters start on that line, as in `int b, int c`. Here the first parameter began on an earlier line, so the check never sees the shared line. A consumer can keep a layout the two-shape rule rejects by wrapping the earlier parameter.

## Change

`ParameterListLayout` now treats a later parameter as sharing a line when it begins on or above the earlier parameter's last line. The other checks are unchanged: one line including both parentheses, a parameter starting on the opening parenthesis line, and the closing parenthesis measured from the last parameter's start line. Regressions cover a verbatim default and a multiline attribute that run into the next parameter, and the same shapes when the next parameter starts on a later line. The CodeClarity explanations and package notes say this.

## After

The verbatim list above reports CCCPL001, and so does the attribute list whose next parameter sits on the attribute's last line. The same verbatim default and attribute stay quiet when the next parameter starts on a later line. A consumer can no longer hide a shared line by beginning the earlier parameter higher up. Lists that already passed with one parameter per line still pass. Whether that warning changes real projects was not measured.

## Evidence

At `origin/main` `ab21473c4e2051b8b0cc2abcbe38ddb35e7f7c8d`, `ParameterListLayout.IsDisallowed` stored only each parameter's start line. Before the repair, `ContinuedParameterSharingALineReports` expected 2 CCCPL001 diagnostics and got none; `MultilineParameterOnItsOwnLinesStaysQuiet` passed. After the repair, from `src/sln/Coree.Analyzers.CodeClarity` in the worktree: `dotnet build --no-restore -m:1` succeeded with 0 errors and the 6 expected DebugHost sample warnings; `dotnet test --no-build` passed 45 tests with 100% line, branch, and method coverage; `dotnet pack --no-restore -m:1` produced a package with the analyzer under `analyzers/dotnet/cs`, props under `build/` and `buildTransitive/`, the updated readme, the shared-line release note, and no `lib/` group. Consumer benefit is not measured.

## Understood assignment

The user invoked `execute-product-evolution` with no preset and no phase-specific tailoring. A correction then said to follow this skill directly, to drop extra wait, topic, and branch-suffix instructions, and to continue the same run. Before any policy file was opened, Discover, Assess, and Select & Bound were each resolved to Conservative `Default.md`. That choice did not change. No Balanced or Ambitious profile was read.

## Discover — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicyDiscover/Contract.md` and `Default.md`.

Read both package introductions, the CodeClarity and Typography solution notes, the parameter-list domain note, and the analyzer implementations and tests. Refreshed `origin` and the local `feature/pevo/*` worktrees. GitHub issues and pull requests were empty. These outcomes are already implemented or claimed and are not candidates: method line endings, `nameof` disambiguation, CCCPL002 literal whitespace, unsigned right shift, Typography comma globs, the CCCPL001 closing-parenthesis end line (`feature/pevo/parameter-list-end-261025`), single quotation marks U+2018/U+201A/U+201B (`feature/pevo/single-quotes-261025`), and a user-defined `operator +` (`feature/pevo/custom-addition-261025`).

- A: CCCPL001 misses two parameters that occupy one line when the earlier parameter began above that line (`ParameterListLayout.cs`). The published two-shape rule and the existing packed-line test already reject a shared line. The closing parenthesis is still measured from the last start line; that separate gap is claimed elsewhere.
- B: an empty Typography Includes value that is already empty when the package props import is replaced with `**`. The README says an empty Includes value turns extra-file scanning off. A value written in the project body, after that import, can still stay empty. Whether anyone depends on the earlier assignment was not established.
- C: `ParameterListFit.Normalize` collapses a `//` comment inside a parameter, so CCCPL002 can recommend a one-line list that would not preserve the comment. `feature/pevo/parameter-literals-261025` is already changing that normalizer for literal whitespace. The comment boundary is not stated in the fit rule.

## Assess — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicyAssess/Contract.md` and `Default.md`.

- A removes a false silence on a layout the existing rule already rejects when both parameters start together. The warning is new only for spans that overlap. Lists whose parameters occupy separate lines stay quiet, including a multiline parameter that is not last. The closing-line measurement stays on the start line, so this does not take the other open parameter-list change. Rollback is a normal revert. The same source file will also be edited by that other change; the outcomes compose, and this branch stays based on `origin/main`.
- B disagrees with the empty-includes sentence only at import time. The documented project-file assignment is a later write. Changing the default risks scanning behavior for projects that never set the property. It ranks below A.
- C is a plausible misleading length, but the normalizer is already being changed and the comment rule is unsettled. Leave it out.

A is the conservative choice: the contract is the existing two-shape rule, the behavior change is a warning on a layout that rule already names, and the tests can show both the overlap and the separate-line control.

## Select & Bound — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicySelectBound/Contract.md` and `Default.md`.

Select A. The result is that CCCPL001 reports parameters whose line spans overlap, while a multiline parameter that ends before the next parameter starts keeps today's result.

Include `ParameterListLayout`, focused regressions, and the repository README, NuGet README, and package notes that describe a shared line. Exclude the closing-parenthesis end-line change, CCCPL002, comments in the length calculation, return scoring, Typography, severity, diagnostic IDs, and package version changes.

A fresh branch and issue check before this worktree found no `parameter-line-share` ref. `custom-addition-261025` appeared in the same refresh and claims the operator outcome, not this one.

### Acceptance criteria

1. A method whose earlier parameter continues onto the line of the next parameter reports CCCPL001 when `)` is on the following line. Cover a verbatim string default and a multiline attribute.
2. The same verbatim default stays quiet when the next parameter starts on a later line.
3. Existing quiet and reported shapes stay put, including shared start lines, a parameter on the opening line, and `)` measured from the last parameter's start line.
4. Show the new overlap cases failing against the original check, then pass the full CodeClarity suite with its configured 100% line, branch, and method coverage. Build and pack the package.
5. Repository and NuGet explanations and the package notes describe the shared-line rule. Public IDs, severities, CCCPL002, and the other analyzers stay unchanged.

### Consumers, risk, and rollback

Consumers are C# developers who see CCCPL001 in the IDE or in a build. Lists that already pass keep passing when parameter spans do not overlap. Lists that were quiet only because the earlier parameter started higher now warn. CCCPL002 is unchanged and can still speak about length.

The main risk is a span whose exclusive end sits at column 0 of the next line and is treated as occupying that line. The legal one-parameter-per-line tests must stay quiet; if a span ends that way, the occupied line is the previous one. Rollback is reverting the implementation and the accompanying explanation. No configuration or data migration is required.

## Baseline and execution

- Status: Done (implementation verified; not merged). Date: 2026-10-25.
- Integration target: `origin/main` at `ab21473c4e2051b8b0cc2abcbe38ddb35e7f7c8d`, refreshed on 2026-10-25.
- Feature branch: `feature/pevo/parameter-line-share-261025`.
- Temporary ignored worktree: `.worktrees/parameter-line-share-261025`.
- Issue-only baseline `3376a8b0a26418ac25af04924ce9ef6731edd0ca` was published and the remote ref matched before product edits.
- Implementation is `0dd97b752ad9bdb7fe80cc8e86e8e19b0f016a73`. Documentation is `8f47db96e1c9e69bfb2a7b57d78aa6fb7b359eed`. No change to the selected outcome or outer scope was required.
- The closing parenthesis is still measured from the last parameter's start line. Tested one-parameter-per-line lists stayed quiet with the span's end line, so no column-0 adjustment was added.

## Verification and final outcome

Acceptance criteria 1-5 are satisfied. Commands ran from `src/sln/Coree.Analyzers.CodeClarity` in the isolated worktree.

- Red, before the layout change, with coverage collection disabled: `ContinuedParameterSharingALineReports` failed (expected 2, actual 0). `MultilineParameterOnItsOwnLinesStaysQuiet` passed.
- Green: `dotnet test --filter FullyQualifiedName~ParameterListLayoutAnalyzerTests -p:CollectCoverage=false -m:1` passed 7 tests.
- `dotnet build --no-restore -m:1`: succeeded, 0 errors, 6 expected DebugHost warnings (CCCMB001, CCCNS001, CCCPL001, two CCCPL002, CCCRC001).
- `dotnet test --no-build`: 45 passed, 0 failed, 100% line, branch, and method coverage.
- `dotnet pack --no-restore -m:1`: succeeded. The package contains the analyzer DLL and XML under `analyzers/dotnet/cs`, props under `build/` and `buildTransitive/`, the updated readme, the shared-line note in the nuspec, and no `lib/` group.
- `git diff --check` was clean before the implementation commits. The diff is the layout helper, its regressions, the two CodeClarity readmes, and the package notes.
- Public IDs, severities, CCCPL002, and the other analyzers are unchanged. Typography was not run.
- Real consumer usage remains unmeasured. A span that ends at column 0 of the next line was not observed in the passing one-parameter-per-line fixtures.

Final-head publication and temporary worktree/local branch cleanup are pending at this closure commit. The early remote feature branch exists. PR creation and integration remain separate human decisions. Rollback remains a normal revert of the implementation and documentation commits.

## Incidental Discoveries (Out of Scope)

- `SmartQuotesAnalyzer` still describes itself as a sample analyzer and says to replace the type. That text was not part of this selection.
- CodeClarity package notes on this baseline stop at 0.1.5 and do not mention CCCPL002, CCCNS001, or CCCMB001. Only the note required by the shared-line change is in scope.
- `ProjectDirectoryTextFiles.ContainsNul` inspects only the first 512 characters. Whether a later NUL should suppress a Typography scan was not investigated.
