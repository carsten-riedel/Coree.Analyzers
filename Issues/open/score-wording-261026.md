## Before

A return that uses `&&` or `||` stays quiet, and a `?:` counts as one calculation step:

```csharp
bool Compare(int a, int b) { return a > b && a >= b; }
int Choice(bool ready, int a, int b) { return ready ? a : b; }
```

`Compare` does not report CCCRC001. `Choice` does not report at the default maximum of 1, because the conditional operator is the only step. `return ready ? a + b : a` does report, because the addition is a second step.

The repository README, the CodeClarity NuGet readme, and the CCCRC001 description still say that "conditional operators" count. That phrase is what a reader has available next to the rule. `docs/codeclarity/user/return-expression-complexity.md` already names `?:`. The scorer was left unchanged after a maintainer question about `||`.

## Change

State the rule the scorer already uses. CCCRC001 counts the conditional operator `?:`. It does not count `&&` or `||`.

The README, the NuGet readme, the diagnostic description, the user page, and the current package notes will say that. The 0.1 package note stays as the historical wording. Nothing in the score changes. The same expressions report or stay quiet as they do today. The diagnostic id, the message, and the severity stay as they are.

## After

Not implemented yet.

## Evidence

At `origin/main` `d6771796304860aed617c6351ed40adcff06729a`, `ReturnExpressionComplexity` increments a score for `ConditionalExpressionSyntax` and not for `&&` or `||`. `SingleCalculationStepIsSimple` keeps `return a > b && a >= b` and `return ready ? a : b` quiet. `NestedCallAndSecondOperatorAreTooComplex` reports `return ready ? a + b : a`. The descriptor description still says "conditional operators". Whether a reader was misled by that phrase was not measured.

## Understood assignment and provenance

The user invoked execute-product-evolution by its skill path and did not name a profile, a product area, or a phase. Before any policy file was opened, Discover, Assess, and Select & Bound were each set to `Default.md` (Conservative). No later instruction changed that choice.

## Discover → Assess → Select & Bound — decision trail

### Discover — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicyDiscover/Contract.md` and `Default.md`.

Read the repository README, both package readmes, the user pages, the analyzer source and tests, the domain notes, and `Issues/done/`. Fetched `origin`. GitHub issues and pull requests were empty.

These outcomes were already claimed, so they are not candidates:

- `feature/pevo/char-ellipsis-261026` records leaving U+2026 in a character literal unedited. `Escape` still rewrites only an apostrophe there, and that fix is not on `main`.
- `feature/pevo/blank-line-fix-261026` records a CCCMB001 code fix that inserts the blank line the warning already names.
- `feature/pevo/return-arrow-261026` records a separate CCCRC001 sentence for a returned arrow, because that arrow cannot hold a local.

- A: the README, the NuGet readme, and the CCCRC001 description say "conditional operators", while the scorer and the user page count `?:` only. `&&` and `||` stay quiet. Evidence: `ReturnExpressionComplexity.IsCalculation`; `SingleCalculationStepIsSimple`; domain clue S-4e18.
- B: an empty Typography `Includes` value that is already empty when the package props import is replaced with `**`. An empty value written in the project body, after that import, still turns extra-file scanning off. Evidence: `EmDashAnalyzer.props` and the five sibling props; the README sentence that an empty includes value turns scanning off.
- C: the CodeClarity sample pins `Coree.Analyzers.CodeClarity` 0.1.2. NuGet lists 1.0.9 as the newest version. The same page says 0.1.2 is the version shown in the sample, and it also shows `dotnet add package` without a version.

### Assess — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicyAssess/Contract.md` and `Default.md`.

- A is a false description of a score that tests already lock. Aligning the words does not add a warning. Changing the scorer would warn on returns earlier cycles left quiet after a maintainer question about `||`. Rollback is restoring the old sentence.
- B cannot treat an omitted value and an explicit empty value as different with the current property. A props edit can break either the default `**` scan or the project-body switch that turns scanning off. The project-file example already works. A note about import order would be the safe remainder, and it is narrower than A.
- C can send a copied sample to 0.1.2. The page already says that is the version shown, and the unversioned `dotnet add` line is already there. A hardcoded current version goes stale on the next build number. Bumping the in-repo package reference can fail the build, and earlier issues left that pin in place.

A is the mismatch a reader hits in the rule text itself.

### Select & Bound — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicySelectBound/Contract.md` and `Default.md`.

Select A. The published scoring words will match `?:`, and they will say that `&&` and `||` do not count. The scorer stays.

A fresh ref check before this worktree found the three claimed branches above and no branch for this wording. `Issues/done/` had already named the mismatch and had not changed the words.

Include the README, the NuGet readme, the CCCRC001 description, the user page, the current package notes, and a test that the descriptor names `?:` and does not say "conditional operators". Exclude any change to what is scored, the diagnostic id, the message, severity, the 0.1 historical note, Typography, parameter lists, the arrow sentence, the blank-line fixer, and the character-literal ellipsis fix.

### Acceptance criteria

1. `return a > b && a >= b` and `return ready ? a : b` stay quiet at the default maximum. `return ready ? a + b : a` still reports CCCRC001.
2. The CCCRC001 description names the conditional operator `?:` and says that `&&` and `||` do not count. It does not say "conditional operators".
3. The repository README, the CodeClarity NuGet readme, `docs/codeclarity/user/return-expression-complexity.md`, and the current package notes say the same thing. The 0.1 note is left as history.
4. From `src/sln/Coree.Analyzers.CodeClarity`, `dotnet test` passes, and Coverlet still requires 100% line, branch, and method coverage of `Coree.Analyzers.CodeClarity`.

### Consumers, risk, and rollback

Consumers are people reading CCCRC001's description or the CodeClarity package text. The warning they already get does not move. Rollback is reverting this branch. No diagnostic id, message, or severity changes.

### Baseline and workspace

- Integration base: `origin/main` `d6771796304860aed617c6351ed40adcff06729a`.
- Local branch: `feature/pevo/score-wording-261026`.
- Worktree: `.worktrees/score-wording`.
- Issue path: `Issues/open/score-wording-261026.md`.
- Publication of this issue-only commit is the next step. It is not done yet.

## Incidental Discoveries (Out of Scope)

- `ProjectDirectoryTextFiles.ContainsNul` returns false when the first NUL is after 512 characters. `ContainsNulReadsOnlyTheFirst512Characters` locks that limit. Whether a later NUL should skip an additional file was not established.
