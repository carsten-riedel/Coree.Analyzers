## Before

A C# file contains an em dash that a word processor left behind:

```csharp
class C { string s = "a—b"; }
```

Coree.Analyzers.Typography reports CTYED001 on that character and tells the developer to use an ASCII hyphen-minus. The IDE offers no fix. The developer deletes the dash and types `-` by hand. The same gap exists for the other five diagnostics: a curly quote, guillemet, or single quotation mark (CTYQM001), a typographic apostrophe (CTYAP001), a horizontal ellipsis (CTYEL001), a minus sign (CTYMN001), and a no-break or narrow no-break space (CTYNB001). Each warning already names the ASCII character that stays silent. Nothing applies that replacement.

## Change

A C# document can replace each flagged character with the ASCII character that warning names:

- U+2013, U+2014, and U+2212 become `-`
- U+201C, U+201D, U+201E, U+201F, U+00AB, and U+00BB become `"`
- U+2018, U+201A, U+201B, and U+2019 become `'`
- U+2026 becomes three `.`
- U+00A0 and U+202F become a space

In a comment the character is written as itself. In a regular or interpolated string, `"` is written as `\"`. In a verbatim string, `"` is written as `""`. In a character literal, `'` is written as `\'`. A raw string (`"""`) is not offered a fix, and the diagnostic still reports. Fix-all applies that same edit to each C# document in the document, the project, or the solution, from the end of the text so an ellipsis does not move a later span. Additional files that are not C# documents still report and are not edited. Which characters are reported does not change.

The repository README, the Typography NuGet readme, the unreleased package notes, and `docs/typography/user/ascii-replacement.md` describe these replacements.

## After

Applying the fix to `class C { string s = "a—b"; }` replaces the em dash with `-`, and CTYED001 is gone from that span. The same action writes the ASCII character named above for the other five diagnostics. A curly double quote in a comment becomes `"`. Inside a normal string `"x“y"` it becomes `"x\"y"`. A raw string that contains U+201C still reports CTYQM001 and is not offered a fix. The compiler does not apply the edit. Whether a developer applies the fix was not measured.

## Evidence

At `origin/main` `085bac71d656d7740ec728cb13137750cd183fc9`, the Typography project has six `DiagnosticAnalyzer` types and no `CodeFixProvider`. `EmDashAnalyzer` reports CTYED001 and its message says "Use ASCII hyphen-minus." `FunctionalTests.EmDashInStringReportsDiagnostic` expects that warning on `"a\u2014b"` and does not apply a fix. The same file locks the other five diagnostics. A search of `src` for `CodeFix` and `RegisterCodeFixes` found no fixer.

After implementation `ed72438302043d3c08fee6cd969fe34354f0c636`, from `src/sln/Coree.Analyzers.Typography`: `dotnet test -m:1` passed 87 tests, and Coverlet reported 100% line, branch, and method coverage of `Coree.Analyzers.Typography`. `dotnet build -m:1` succeeded. The analyzer project warns RS1038 because the assembly references Workspaces; DebugHost, included in that build, still reports CTYED001, CTYQM001, CTYAP001, CTYEL001, CTYMN001, and CTYNB001 on `Program.cs` and `SampleTypography.txt`. `dotnet pack -m:1` produced `Coree.Analyzers.Typography.1.0.7-ged72438302.nupkg` with one analyzer dll under `analyzers/dotnet/cs/`, props under `build/` and `buildTransitive/`, no `lib/`, and no package dependencies. That dll contains `TypographyCharacterFixProvider`. `TypographyCharacterFixTests` covers each replacement, quote escaping, a raw string with no fix, two dashes, and two ellipses. Consumer uptake is unmeasured.

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

Status: Done. Implementation is verified and ready for review. It is not merged.

- Integration target: `main`, refreshed `origin/main` at `085bac71d656d7740ec728cb13137750cd183fc9`.
- Feature branch: `feature/pevo/typography-fix-261025`.
- Worktree: `.worktrees/typography-fix-261025`.
- Issue path: `Issues/done/typography-fix-261025.md`.
- Issue-only baseline: `45847906125a98c8613a95bf35d15383982d4980`, published to `origin` before product edits. Quote escaping is `c7a012f64089ad685c2f8155300155a28aa2ab7e`. Fix-all scopes are `4c7e89bed32aea814d00291f90c8f29bd747f4d7`.
- Implementation: `ed72438302043d3c08fee6cd969fe34354f0c636`.
- Publication of this closure commit, pull-request creation, merge, and worktree cleanup are still pending.

## Decision revision — quote and apostrophe escaping

The issue-only baseline's Change stood for dashes, the ellipsis, the minus sign, and no-break spaces: one flagged character becomes the raw ASCII text. The Change section above now states the rule that was implemented.

That baseline text does not stand for a raw `"` or `'` dropped into a literal. That was not visible from the diagnostic text. A curly quote inside a normal string is one character of the literal. Replacing it with a raw `"` ends the literal (`"x"y"`). A typographic apostrophe inside a character literal does the same with `'`. The warning tells the developer to use the ASCII mark. It does not tell them to break the file. The functional tests that put these characters in strings made that obvious only when the replacement was written into that same position.

Revised rule for CTYQM001 and CTYAP001 only:

- In a comment or other trivia, insert the raw ASCII quote or apostrophe.
- In a regular or interpolated string, insert `"` as `\"`. An apostrophe stays `'`.
- In a verbatim string, insert `"` as `""`.
- In a character literal, insert `'` as `\'`. A double quote stays `"`.
- In a raw string literal (`"""`), do not offer the fix. The quotes that keep that literal valid depend on its delimiter. The diagnostic still reports.

Acceptance criteria 2 and 3 are read with this rule. Add: a raw string that contains U+201C still reports CTYQM001 and is not offered a fix. The other criteria are unchanged.

Fix-all uses that same replacement for every C# document in the scope the IDE requests: the document, the project, or the solution. The first write-up named only the document. A project or solution fix-all is the same edit, applied to each document the developer included. Raw strings in those documents are still skipped.

## Final outcome

The fixer, its tests, and the notes were implemented with the two decision revisions already on this branch: quotes and apostrophes are escaped inside literals, raw strings are left alone, and fix-all covers the document, project, and solution. Diagnostic ids, reported characters, severity, and additional-file scanning stayed as they were. RS1038 remains a warning on the analyzer project. DebugHost still loads the diagnostics from that same assembly, so the warning was not suppressed and the fixer was not split into a second dll.

Rollback is reverting the fixer, its tests, and the accompanying notes together. No configuration migration is required. Consumer uptake remains unmeasured.

## Incidental Discoveries (Out of Scope)

- `ProjectDirectoryTextFiles.ContainsNul` returns false when U+0000 appears after the first 512 characters. `ContainsNulReadsOnlyTheFirst512Characters` locks that limit. Whether a later NUL should skip the rest of an additional file was not established.
- CodeClarity's `PackageReleaseNotes.txt` does not name CCCNS001. The README and the NuGet readme already do.
