## Before

A C# file contains a horizontal ellipsis in a character literal:

```csharp
class C { char c = '…'; }
```

Coree.Analyzers.Typography reports CTYEL001 and offers a fix. Applying it writes three periods, so the source becomes `char c = '...';`. That is not a character literal.

The same ellipsis in a string is already replaced with three periods, and that string still parses. An apostrophe in a character literal is already written as `\'`. An em dash in a character literal is one hyphen. A raw string is already left unchanged, and the warning still reports.

## Change

The fixer will not edit U+2026 when it sits in a character literal. CTYEL001 still reports there. A string ellipsis still becomes three periods. An apostrophe in a character literal still becomes `\'`. A one-character replacement in a character literal, such as an em dash becoming a hyphen, still applies.

The repository README, the Typography NuGet readme, `docs/typography/user/ascii-replacement.md`, and the package notes will say that an ellipsis in a character literal is not edited and that the warning still reports. The diagnostic text stays "Use three ASCII periods." Reported characters, diagnostic ids, and severity stay as they are.

## After

Not implemented yet.

## Evidence

At `origin/main` `d6771796304860aed617c6351ed40adcff06729a`, `TypographyAsciiReplacement.ForCharacter` maps U+2026 to `...`. `Escape` then rewrites only an apostrophe inside a `CharacterLiteralToken` and returns that text. Nothing in `TypographyCharacterFixTests` covers an ellipsis in a character literal. `ApostropheInCharacterLiteralIsEscaped` locks `\'`. `EllipsisMinusAndSpacesFixToAscii` locks three periods in a string.

Whether a developer applies the fix was not measured.

## Understood assignment and provenance

The user invoked execute-product-evolution by its skill path and did not name a profile, a product area, or a phase. Before any policy file was opened, Discover, Assess, and Select & Bound were each set to `Default.md` (Conservative). The user did not add a phase constraint. That choice did not change.

## Discover → Assess → Select & Bound — decision trail

### Discover — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicyDiscover/Contract.md` and `Default.md`.

Read the repository README, both package readmes, the user pages, the analyzer and fixer source, the tests, the domain notes, and `Issues/done/`. Fetched `origin`. The only remote branch is `origin/main` at `d6771796304860aed617c6351ed40adcff06729a`. GitHub issues and pull requests were empty. No `feature/pevo/` branch existed. Outcomes already on `main`, including interpolation-hole quotes, `u8` strings, and documentation comments in parameter lists, were not candidates.

- A: CTYEL001 on U+2026 in a character literal is fixed to `...`, which is not a character literal. Evidence: `ForCharacter` and `Escape` in `TypographyAsciiReplacement.cs`. No character-literal ellipsis test.
- B: CCCRC001 always says to extract intermediate calculations into local variables, including `int Property => Raw + Raw + 1`, an indexer, an operator, and a conversion. Those arrows have no place for a local. Evidence: the message on `ReturnTypeComplexityAnalyzer.Rule`; `ExpressionBodiedReturnedValuesReport`. The user page states that same request and also documents the arrows.
- C: the README, the NuGet readme, and the CCCRC001 description say "conditional operators". The user page and `ReturnExpressionComplexity` count `?:` only. `return a > b && a >= b` stays quiet. Several done issues left the scorer unchanged after a maintainer question about `||`.
- D: the CodeClarity install sample and both analyzer projects pin `Coree.Analyzers.CodeClarity` 0.1.2. NuGet lists 1.0.9 as the newest version. The page also shows `dotnet add package` without a version, and it says the version 0.1.2 is the one shown in the sample.
- E: an empty Typography `Includes` value that is already empty when the package props import is replaced with `**`. An empty value written in the project body, after that import, still turns extra-file scanning off. Evidence: `EmDashAnalyzer.props` and the five sibling props; the README sentence that an empty includes value turns scanning off.

### Assess — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicyAssess/Contract.md` and `Default.md`.

- A writes source that is not a character literal. The string replacement and the apostrophe escape already exist. Withholding this one edit does not add a warning. Rollback is reverting that check, its tests, and the notes. How often a character literal contains U+2026 was not measured.
- B misleads where an arrow cannot hold a local. It does not change what is reported and does not edit source. The user page already says the warning asks for locals, in the same explanation that documents those arrows.
- C would restate a score earlier cycles left unchanged. The user page already names `?:`. Changing the scorer would warn on returns the tests leave quiet.
- D misleads someone who copies the sample. The unversioned `dotnet add` line is already on the page, and the sample names 0.1.2 as the version shown. A pinned version goes stale on the next publish. Bumping the in-repo reference was left in place because it can fail the build.
- E is a real props condition for a value set before the import. Distinguishing unset from empty can change scanning for projects that already have an empty value at that point. A new token is a new consumer spelling.

A is the failure that still writes source the compiler will not accept.

### Select & Bound — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicySelectBound/Contract.md` and `Default.md`.

Select A. An ellipsis in a character literal is reported and not edited. A string ellipsis still becomes three periods. A character literal whose replacement is one character, or an escaped apostrophe, still changes.

A fresh branch and issue check before this worktree found no `char-ellipsis` ref, no open issue, and no pull request for this outcome. `Issues/done/parameter-doc-comment-261026.md` and `Issues/done/quote-hole-261026.md` named the same broken edit and did not implement it.

Include the character-literal check in `Escape`, regressions for an ellipsis that is not offered a fix, an apostrophe that is still escaped, an em dash that still becomes a hyphen, a string ellipsis that still becomes three periods, and the README, NuGet readme, user page, and package notes that describe the replacement.

Exclude the CCCRC001 message, `&&` and `||`, the 0.1.2 sample, Includes defaults, the CTYEL001 message text, diagnostic ids, severity, and the other character replacements.

### Acceptance criteria

1. `class C { char c = '…'; }` still reports CTYEL001. The fixer does not change that source and does not offer a code action.
2. `class C { char c = '’'; }` still becomes `class C { char c = '\''; }`.
3. `class C { char c = '—'; }` still becomes `class C { char c = '-'; }`.
4. `class C { string s = "x…y"; }` still becomes `class C { string s = "x...y"; }`.
5. From `src/sln/Coree.Analyzers.Typography`, `dotnet test` passes with the configured 100% line, branch, and method coverage of `Coree.Analyzers.Typography`.
6. The repository README, the NuGet readme, `docs/typography/user/ascii-replacement.md`, and the package notes say that an ellipsis in a character literal is not edited and that the warning still reports.

### Consumers, risk, and rollback

Consumers are people who apply the Typography lightbulb or fix-all on CTYEL001. A character literal that contains U+2026 keeps that character and keeps the warning. Strings, comments, and one-character replacements in a character literal stay as they are.

Rollback is reverting this branch. No diagnostic id, severity, or reported character changes.

### Baseline and workspace

- Integration base: `origin/main` `d6771796304860aed617c6351ed40adcff06729a`.
- Local branch: `feature/pevo/char-ellipsis-261026`.
- Worktree: `.worktrees/char-ellipsis`.
- Issue path: `Issues/open/char-ellipsis-261026.md`.
- Publication of this issue-only commit is the next step. It is not done yet.

## Incidental Discoveries (Out of Scope)

- The CodeClarity install sample still shows package version 0.1.2 while NuGet lists 1.0.9. The page also has an unversioned `dotnet add package` line.
- An empty Typography `Includes` value set before the package props import is still replaced with `**`.
