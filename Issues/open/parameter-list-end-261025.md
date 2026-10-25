## Before

CCCPL001 accepts a multi-line parameter list when each parameter starts on its own line and the closing parenthesis is on the next line. That next line is measured from where the last parameter starts. A last parameter that begins higher than it ends therefore warns even though the closing parenthesis already follows the parameter.

```csharp
public void M(
    int a,
    [Mark]
    int b
)
```

`)` is on the line after `int b`. CCCPL001 still reports the list. The same attribute on the first parameter stays quiet when the last parameter occupies one line. A multiline default in the last position warns for the same reason. A consumer who already used the documented list shape can silence the warning by moving a one-line parameter to the end.

## Change

Measure the closing parenthesis from the last line of the last parameter. Leave the other shape checks in place: both parentheses and every parameter on one line, no two parameters starting on the same line, no parameter starting on the opening parenthesis line, and no closing parenthesis on the last parameter's last line or later than the immediately following line. Cover a last-parameter attribute and a last-parameter multiline default, including the closing parenthesis on that parameter's last line and a blank line before `)`. State this span in the consumer explanations and package notes.

## After

Not implemented yet.

## Evidence

At `origin/main` `ab21473c4e2051b8b0cc2abcbe38ddb35e7f7c8d`, `ParameterListLayout.IsDisallowed` keeps the last parameter's start line and allows the list only when the closing parenthesis is on that line plus one. `AcceptedLayoutsStayQuiet` already allows an attribute on the same line as an earlier parameter. `MixedLayoutsReport` already requires a warning when `)` shares the last parameter line or a blank line precedes `)`. Nothing in those tests gives the last parameter a different start and end line. Consumer benefit is not measured.

## Understood assignment

The user invoked `execute-product-evolution` with no preset and no phase-specific tailoring. A later correction said to continue this same run, not to wait on other runs, and not to apply an extra topic preference or branch suffix. Discover, Assess, and Select & Bound were each resolved to Conservative `Default.md` before any policy file was opened. That choice did not change. No Balanced or Ambitious profile was read.

Concurrent evolution branches were screened as existing work. Their outcomes were excluded when they were the same improvement. They were not treated as a reservation of a whole product area.

## Discover — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicyDiscover/Contract.md` and `Default.md`.

Read the repository and package introductions, the CodeClarity and Typography solution notes, the parameter-list and analyzer domain notes, and the current analyzer implementations and tests. Refreshed `origin` branch names, the open evolution issues, and GitHub issues and pull requests. Five candidates remained after removing work that is already implemented or recorded as the selected outcome of another branch:

- A: CCCPL001 measures the closing parenthesis from the last parameter's start line (`ParameterListLayout.cs`). A legal list warns when only that parameter spans lines. The same span is already accepted on an earlier parameter.
- B: CTYQM001's character set is U+201C–U+201F plus guillemets. The opening single quotation mark U+2018 is not flagged, while the package text says curly quotes. U+2019 stays on CTYAP001. Whether the single-quote family was left out on purpose is not established.
- C: CCCRC001 counts `?:` and does not count `&&` or `||`. The package text says conditional operators. The scorer was left unchanged after a maintainer question about `||` (`docs/agent-domain-understanding/01-domain-clues.md`, `ReturnTypeComplexityAnalyzerTests`).
- D: `IsStringConcatenation` treats addition as concatenation when either operand has type string. A user-defined operator that accepts a string and returns a number would not count. Built-in concatenation is the common case the current check already covers.
- E: Typography props replace an empty Includes value with `**` at import time. The documented place to set Includes is a property group in the consuming project, which is evaluated later. Whether an earlier `Directory.Build.props` assignment survives was not established.

Excluded as the same outcome already taken: method blank-line endings (`feature/pevo/method-newlines-261025`, done), `nameof` scoring (`feature/pevo/return-nameof-261025`, done), literal whitespace in CCCPL002 (`feature/pevo/parameter-literals-261025`, open), unsigned right shift (`feature/pevo/return-unsigned-shift-261025`, open), and comma-containing Typography globs (`feature/pevo/typography-commas-261025`, open). No open GitHub issue or pull request covers A.

## Assess — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicyAssess/Contract.md` and `Default.md`.

- A has a direct contradiction between the list shape and the closing-line check, a common attribute layout, and an order-dependent result that existing single-line tests will keep stable. The repair removes a false warning. It does not add a warning to any list whose last parameter already starts and ends on one line. Rollback is a normal revert.
- B would add warnings for characters the current set omits. The promise is less specific than A's written shape rule, and the disruption is wider.
- C is real wording tension, but the maintained behavior is already locked by a test and by the decision to leave the scorer unchanged. A clarification would help less than removing the false CCCPL001 warning.
- D is a plausible false negative for uncommon operators. Binding, incomplete source, and operators that return string need a larger matrix than A.
- E is not shown for the documented project-file assignment. Treating it as a defect would risk changing the default scan without a reproduced consumer path.

A is the conservative choice: clearer evidence, smaller semantic change, and a regression net that already exists.

## Select & Bound — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicySelectBound/Contract.md` and `Default.md`.

Select A. The useful result is that CCCPL001 judges the closing parenthesis from the last line of the last parameter, so an attribute or default may span lines without a shape warning when `)` follows that last line immediately.

Include `ParameterListLayout`, focused regressions, and the consumer README, NuGet README, and package notes that describe this shape. Exclude CCCPL002's length calculation, comment and documentation lines inside a parameter, return scoring, Typography, severity, diagnostic IDs, and package version changes.

A fresh branch and issue-name check immediately before this worktree found no `parameter-list-end` ref and no issue for this outcome. Parameter-list work on `parameter-literals-261025` is the whitespace-length outcome, not this one.

### Acceptance criteria

1. A method and a record primary constructor whose last parameter spans lines, with `)` on the immediately following line, do not report CCCPL001. Cover an attribute on its own line and a multiline string default.
2. Those same last parameters still report when `)` is on the parameter's last line, and when a blank line sits between that last line and `)`.
3. Existing quiet and reported shapes stay put: one line, a multiline parameter that is not last, shared start lines, a parameter on the opening line, and `)` on the last single-line parameter.
4. Show the new accepted cases failing against the original check, then pass the full CodeClarity suite with its configured 100% line, branch, and method coverage. Build and pack the package.
5. Repository and NuGet explanations and the package notes describe the last-line rule. Public IDs, severities, CCCPL002, and the other analyzers stay unchanged.

### Consumers, risk, and rollback

Consumers are C# developers who see CCCPL001 in the IDE or in a build. Lists that already pass keep passing when the last parameter is one line. Lists that warned only because the last parameter spanned lines become quiet; CCCPL002 can still report when the collapsed parameters fit in the configured length. That fit rule is intentionally unchanged.

The main risk is treating the end position as the following line when a span ends at column 0. The new tests use an attribute and a verbatim default whose last token is on the expected line. Rollback is reverting the implementation and the accompanying explanation. No configuration or data migration is required.

## Baseline and execution

- Status: Open.
- Integration target: `origin/main` at `ab21473c4e2051b8b0cc2abcbe38ddb35e7f7c8d`, refreshed on 2026-10-25.
- Feature branch: `feature/pevo/parameter-list-end-261025`.
- Temporary ignored worktree: `.worktrees/parameter-list-end-261025`.
- This issue is the only content of the first commit. Publish that commit and verify the remote ref before any product edit.
- Run from `src/sln/Coree.Analyzers.CodeClarity`: documented restore, serialized build, full test, and pack. A filtered red run may disable coverage because a filtered run cannot meet the whole-assembly threshold.
- Verification, final publication, and cleanup are pending.

## Incidental Discoveries (Out of Scope)

- Expression-bodied `=> a + b + c` is already expected to stay quiet in `ReturnTypeComplexityAnalyzerTests`. That is observed behavior, not a selected gap.
- CodeClarity package notes on this baseline stop at 0.1.5 and do not mention CCCPL002, CCCNS001, or CCCMB001. Only the note required by this closing-line change is in scope.
- `ProjectDirectoryTextFiles.ContainsNul` inspects only the first 512 characters. Whether a later NUL should suppress a Typography scan was not investigated.
