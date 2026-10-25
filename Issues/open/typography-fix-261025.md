## Before

A C# file contains an em dash that a word processor left behind:

```csharp
class C { string s = "a—b"; }
```

Coree.Analyzers.Typography reports CTYED001 on that character and tells the developer to use an ASCII hyphen-minus. The IDE offers no fix. The developer deletes the dash and types `-` by hand. The same gap exists for the other five diagnostics: a curly quote, guillemet, or single quotation mark (CTYQM001), a typographic apostrophe (CTYAP001), a horizontal ellipsis (CTYEL001), a minus sign (CTYMN001), and a no-break or narrow no-break space (CTYNB001). Each warning already names the ASCII character that stays silent. Nothing applies that replacement.

## Change

Add one code fix for those six diagnostics on C# documents. The fix replaces only the flagged character:

- U+2013 and U+2014 become U+002D (`-`)
- U+201C, U+201D, U+201E, U+201F, U+00AB, and U+00BB become U+0022 (`"`)
- U+2018, U+201A, U+201B, and U+2019 become U+0027 (`'`)
- U+2026 becomes three U+002E (`.`)
- U+2212 becomes U+002D (`-`)
- U+00A0 and U+202F become U+0020 (space)

A document-scoped fix-all for one diagnostic id applies that same mapping to every match of that diagnostic in the document, from the end of the text so a longer replacement (the ellipsis) does not move a later span. Additional files that are not C# documents still report and do not get this fix. Which characters are reported does not change.

The repository README, the Typography NuGet readme, and the unreleased package notes say the IDE can apply these replacements.

## After

Not implemented yet.

## Evidence

At `origin/main` `085bac71d656d7740ec728cb13137750cd183fc9`, the Typography project has six `DiagnosticAnalyzer` types and no `CodeFixProvider`. `EmDashAnalyzer` reports CTYED001 and its message says "Use ASCII hyphen-minus." `FunctionalTests.EmDashInStringReportsDiagnostic` expects that warning on `"a\u2014b"` and does not apply a fix. The same file locks the other five diagnostics. A search of `src` for `CodeFix` and `RegisterCodeFixes` found no fixer.

## Understood assignment and provenance

The user invoked execute-product-evolution and named Balanced. Before any policy file was opened, Discover, Assess, and Select & Bound were each set to `Balanced.md`. The user did not tailor a phase, name a product area, or authorize a break of a documented diagnostic rule. That choice did not change.

## Discover → Assess → Select & Bound — decision trail

### Discover — Balanced.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicyDiscover/Contract.md` and `Balanced.md`.

Read the repository README, both package readmes, the CodeClarity and Typography solution notes, the analyzer implementations and tests, `docs/codeclarity/user/`, the domain notes, and `Issues/done/`. Fetched `origin`. GitHub issues and pull requests were empty. No `feature/pevo/` branch exists on `origin`. Outcomes already on `main` were not candidates: yield-return scoring, parameter-list layout and fit, type namespaces, method blank lines, Typography exclude globs, and the single-quotation-mark split.

These were noticed and left unlabeled. `&&` and `||` are not CCCRC001 steps; `SingleCalculationStepIsSimple` expects `return a > b && a >= b` to stay quiet, and the scorer was left unchanged after a question about `||`. `ProjectDirectoryTextFiles.ContainsNul` reads only the first 512 characters, and a test locks that; no false diagnostic from a later NUL was observed. `PackageReleaseNotes.txt` for CodeClarity never names CCCNS001, while the README already describes it. U+2039 and U+203A stay out of CTYQM001 on purpose (`Issues/done/single-quotes-261025.md`).

- A: `int Body(int a, int b, int c) => a + b + c` produces no CCCRC001. `ReturnTypeComplexityAnalyzer` registers `return` and `yield return` only. Evidence: `SingleCalculationStepIsSimple`, `docs/codeclarity/user/return-expression-complexity.md`.
- B: each Typography `*Includes` property is set to `**` when it is empty at props-import time (`EmDashAnalyzer.props` and the five siblings). The README says an empty includes value turns extra-file scanning off. A value written in the project body, after that import, can stay empty. Unset and empty are the same string when the props import. An earlier note of this order is in `Issues/done/yield-return-261025.md`.
- C: the CodeClarity install example and both analyzer projects pin `Coree.Analyzers.CodeClarity` 0.1.2. NuGet 0.1.2 notes describe only the initial CCCRC001 release. NuGet 1.0.1 notes describe the diagnostics the README already lists. `dotnet add package` without a version is also on the page.
- D: the six Typography warnings name an ASCII replacement and ship no code fix. Evidence: the diagnostic messages in the six analyzer types, and no `CodeFixProvider` under `src`.
- E: CCCPL001 and CCCMB001 put the two characters `\` and `n` in the diagnostic text. `MethodBlankLineAnalyzerTests` asserts that exact message. The sentences around it already say what to change.

### Assess — Balanced.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicyAssess/Contract.md` and `Balanced.md`.

- A is ineligible for this cycle. The user guide, the quiet test, and acceptance criterion 4 of `Issues/done/yield-return-261025.md` define that silence. Reporting it would warn on expression-bodied members the package text currently leaves quiet, including builds that treat warnings as errors.
- B is a real import-order trap for someone who sets an empty includes value in `Directory.Build.props`. A non-empty glob that matches nothing already turns scanning off. A fix has to keep "unset means `**`" and "empty in the project body means off" apart. Those two states look the same at import time, so the safe change is a new non-empty token, not a tweak of the empty condition.
- C corrects a copied version and the repository's own package pin. The bash install line already takes the current package. The pin does not change what a consumer's nupkg reports. Smaller than a missing edit action.
- D is additive. The replacement is the character the warning already names. Reported characters, ids, and severity stay. The fix can run on C# documents. Additional files that are not documents stay report-only. Rollback is removing the fixer. The main care is the ellipsis, which replaces one character with three, and a fix-all that must not use a stale span.
- E is readable enough from the surrounding sentence. Changing it rewrites a message the test locks, for a smaller result than D.

D has the stronger combination: the warning already says what to type, and the editor still makes the developer type it.

### Select & Bound — Balanced.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicySelectBound/Contract.md` and `Balanced.md`.

Select D. The result is that a C# document can replace each flagged Typography character with the ASCII character that warning already names.

Include the fixer, document-scoped fix-all for one diagnostic id, regressions for every character listed under Change, a two-match em-dash fix-all, and the README, NuGet readme, and unreleased package notes. Exclude CodeClarity, additional-file edits, new diagnostic ids, severity, includes and excludes, the character sets, expression-bodied returns, the 0.1.2 pin, and the `\n` message text.

The overlap recheck after `git fetch --prune origin` found no Typography code-fix issue, pull request, or `feature/pevo/` branch. Done issues in this area cover exclude globs and single quotation marks, not a fixer.

### Acceptance criteria

1. CTYED001 on U+2014 and on U+2013 each fixes to U+002D, and that diagnostic is gone.
2. CTYQM001 fixes U+201C, U+201D, U+201E, U+201F, U+00AB, and U+00BB to U+0022, and U+2018, U+201A, and U+201B to U+0027.
3. CTYAP001 fixes U+2019 to U+0027.
4. CTYEL001 fixes U+2026 to three U+002E characters.
5. CTYMN001 fixes U+2212 to U+002D.
6. CTYNB001 fixes U+00A0 and U+202F to U+0020.
7. Two em dashes in one C# document can be fixed together for CTYED001, and the second character is still the dash that was flagged.
8. An ASCII hyphen, quote, apostrophe, period, or space is not rewritten. Diagnostic ids, reported characters, severity, and additional-file scanning stay as they are. A non-C# additional file is not offered this fix.
9. The repository README, the Typography NuGet readme, and the package notes describe these replacements. From `src/sln/Coree.Analyzers.Typography`, `dotnet test -m:1` passes with the configured 100% line, branch, and method coverage, and `dotnet build -m:1` and `dotnet pack -m:1` succeed. The DebugHost project still builds with the in-repo analyzer.

### Consumers, risk, and rollback

Consumers are C# developers who see CTYED001, CTYQM001, CTYAP001, CTYEL001, CTYMN001, or CTYNB001 in an IDE that loads code fixes. A build that only runs the analyzer is unchanged: the same characters still warn, and no fix is applied by the compiler. Someone who wants the typographic character in a string can leave the warning or suppress it; the fix runs only when they apply it.

The main risk is a fix-all that uses spans from before a longer replacement, so the ellipsis case is covered on its own and the two-dash case checks that the second span still lands on a dash. Referencing the code-fix assemblies must not stop `dotnet build` from loading the analyzer. Rollback is reverting the fixer, its tests, and the notes. No configuration migration is required.

## Selected improvement and baseline

- Integration target: `main`, refreshed `origin/main` at `085bac71d656d7740ec728cb13137750cd183fc9`.
- Feature branch: `feature/pevo/typography-fix-261025`.
- Worktree: `.worktrees/typography-fix-261025`.
- Issue path: `Issues/open/typography-fix-261025.md`.
- Early push of this issue-only commit is the next step and is not done yet. Publication of a later closure commit, pull request, merge, and worktree cleanup are not started.

## Incidental Discoveries (Out of Scope)

- `ProjectDirectoryTextFiles.ContainsNul` returns false when U+0000 appears after the first 512 characters. `ContainsNulReadsOnlyTheFirst512Characters` locks that limit. Whether a later NUL should skip the rest of an additional file was not established.
- CodeClarity's `PackageReleaseNotes.txt` does not name CCCNS001. The README and the NuGet readme already do.
