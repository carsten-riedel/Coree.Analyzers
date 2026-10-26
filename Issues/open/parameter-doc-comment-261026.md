## Before

CCCPL002 tells a consumer to put a short parameter list on one line. A `//` comment inside that list already keeps the diagnostic quiet, because the one-line form would comment out the rest of the signature. A `///` documentation comment does not.

This list is still measured as `int width, int height`. That text fits in the default 120 characters, so CCCPL002 reports and shows the one-line example `M(a, b, c)`:

```csharp
public void Create(
    /// <param name="width">Width in pixels.</param>
    int width,
    int height)
```

Writing that list on one line leaves the `///` in place. Everything after `///` becomes a comment, including `int width` and the closing parenthesis. A documentation comment between the parameters has the same effect. The characters `///` inside a string are not a comment, and that case is not this problem.

## Change

CCCPL002 stays quiet when the parameter list contains a `///` documentation comment. A short broken list with no documentation comment still reports. A `///` sequence inside a string still reports. A block comment still reports, because that comment can sit on one line. The length calculation, CCCPL001, the diagnostic id, and the maximum stay as they are.

The repository README, the NuGet readme, the user page for parameter-list length, the analyzer description, and the package notes say that a documentation comment keeps the list quiet for the same reason a `//` comment does.

## After

Not implemented yet.

## Evidence

At `origin/main` `f8e6c5f3e35f864aad9613843eacd0cee0c16ce9`, `ParameterListFit.ContainsLineComment` returns true only for `SingleLineCommentTrivia`. A parse of the list above shows the documentation comment as `SingleLineDocumentationCommentTrivia` on the `int` token. The three slashes are not a separate `//` trivia, so the existing check does not see them. `parameter.ToString()` omits that leading trivia, so the measured length is still the parameter text. `LineCommentBetweenParametersStaysQuiet` covers `//` only. Nothing in `ParameterListFitAnalyzerTests` places a `///` comment in the list.

The analyzer was not executed on this sample before the change. Consumer benefit is not measured.

## Understood assignment

The user invoked `execute-product-evolution` by pointing at the skill file, with no named preset and no phase-specific tailoring. Before any policy file was opened, Discover, Assess, and Select & Bound were each resolved to Conservative `Default.md`. That choice did not change. No Balanced or Ambitious profile was read.

## Discover — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicyDiscover/Contract.md` and `Default.md`.

Read the repository README, both package introductions, the CodeClarity user pages, the done issues from 2026-10-25, and the analyzer implementations. Refreshed `origin`. The only remote branch is `origin/main`. GitHub issues and pull requests were empty. Local `Issues/` contains only `done/` files. None of those claim a documentation comment inside a parameter list.

These outcomes are already implemented and are not candidates: Typography ASCII replacement, expression-bodied return scoring, yield return scoring, `nameof` disambiguation, user-defined `operator +`, unsigned right shift, CCCPL001 closing-line and shared-line checks, CCCPL002 line comments, preprocessor directives, literal line breaks, and literal whitespace, Typography exclude globs, single quotation marks, and method blank-line endings.

- A: CCCPL002 still recommends one line when a `///` documentation comment sits inside a short broken parameter list. The one-line form comments out the rest of the signature. Evidence: `ParameterListFit.ContainsLineComment`, a parse of that list, `docs/codeclarity/user/parameter-list-length.md`.
- B: The Typography ellipsis fix writes `...` into a character literal. `char c = '…';` would become `char c = '...';`, which is not a valid character literal. Strings are already replaced safely, and a raw string is already left alone. Evidence: `TypographyAsciiReplacement.Escape`, `ApostropheInCharacterLiteralIsEscaped`, `Ellipsis` string tests. No character-literal ellipsis test.
- C: A `//` comment after the closing parenthesis also suppresses CCCPL002, including `int b) // note`. The one-line form `M(int a, int b) // note` keeps that comment. The published sentence says a list that contains a `//` comment stays quiet. Evidence: parse of that trailing trivia; `ContainsLineComment`; `docs/codeclarity/user/parameter-list-length.md`.
- D: CCCMB001 tells the editor to put a blank line before a method, and CodeClarity has no code fix that does so. Typography can apply the character its warnings name. Evidence: `MethodBlankLineAnalyzer` message; no `CodeFixProvider` under CodeClarity.
- E: CCCPL001 and CCCPL002 do not look at operator or conversion-operator parameter lists. The published declaration list does not name operators. Evidence: `ParameterListLayoutAnalyzer`, `ParameterListFitAnalyzer`, `docs/agent-domain-understanding/context/parameter-list-layout.md`.

## Assess — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicyAssess/Contract.md` and `Default.md`.

- A removes a recommendation that cannot be followed without commenting out the signature. Lists with no documentation comment keep today's warning. The check matches the existing `//` exception. Rollback is a normal revert. No new warning.
- B stops a fix from producing a character literal the compiler rejects. The trigger is a typographic ellipsis used as a `char`, which is a rarer edit than a documentation comment on a parameter. Withholding that one fix is small, and it does not correct the warning a build already prints.
- C would start warning on a trailing comment that yesterday's user page can be read as already covering: a list that contains a `//` comment stays quiet. The one-line form can keep that particular comment, but the change adds warnings to a fresh exemption.
- D adds an editor action and a Workspaces reference without changing who is warned. The blank line can already be typed. The insertion point around comments and same-line methods is a larger review than A.
- E adds warnings for declaration kinds the published rule does not name. An earlier evolution left that extension out for the same reason.

A is the conservative choice: the misleading one-line advice is visible from the current check and the documentation-comment trivia, and the repair does not add warnings.

## Select & Bound — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicySelectBound/Contract.md` and `Default.md`.

Select A. The result is that CCCPL002 stays quiet when a parameter list contains a `///` documentation comment, and still reports a short broken list when it does not.

A fresh branch and issue check before this worktree found no `parameter-doc-comment` ref and no open issue for this outcome. `parameter-line-comment-261025` is done and covers `//` only.

Include the fit check, focused regressions, and the repository README, NuGet README, user page, analyzer description, and package notes that describe this exception. Exclude the ellipsis character-literal fix, trailing-comment narrowing, code fixes, operator parameter lists, CCCPL001, return scoring, Typography, severity, diagnostic ids, the maximum, and package version changes.

### Acceptance criteria

1. A short broken list does not report CCCPL002 when a `///` documentation comment sits before a parameter, or between parameters.
2. The same short shape still reports when the only `///` is inside a string, and the message still states the measured length and the limit.
3. Existing `//` silence, directive silence, literal line-break silence, block-comment reporting, short breaks, long lists, maximum handling, and severity handling stay put.
4. Show the documentation-comment case failing against the original check, then pass the full CodeClarity suite with its configured 100% line, branch, and method coverage. Build and pack the package.
5. Repository and NuGet explanations, the user page, the analyzer description, and the package notes state the documentation-comment exception. Public ids, the maximum, CCCPL001, and the other analyzers stay unchanged.

### Consumers, risk, and rollback

Consumers are projects that compile with CodeClarity. A project that treats CCCPL002 as an error stops failing this list for an edit that would comment out the signature. Lists that already fit, or that break because they do not fit, are unchanged.

Rollback is reverting this branch. No diagnostic id, severity, or maximum changes.

### Baseline and workspace

- Integration base: `origin/main` `f8e6c5f3e35f864aad9613843eacd0cee0c16ce9`.
- Local branch: `feature/pevo/parameter-doc-comment-261026`.
- Worktree: `.worktrees/parameter-doc-comment`.
- Issue path: `Issues/open/parameter-doc-comment-261026.md`.
- Early publication of this issue commit is the next step and is not done yet.

## Incidental Discoveries (Out of Scope)

- The CodeClarity package description still says "conditional operators", while `docs/codeclarity/user/return-expression-complexity.md` names `?:`. `return a > b && a >= b` stays quiet in `SingleCalculationStepIsSimple`. The scorer was not reopened.
- `Coree.Analyzers.CodeClarity.csproj` still package-references `Coree.Analyzers.CodeClarity` 0.1.2. That pin does not change the nupkg a consumer builds from this source.
