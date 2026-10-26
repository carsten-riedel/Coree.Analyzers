## Before

Two methods in the same type sit next to each other with no blank line between them:

```csharp
class Packed
{
    void Walkable() { }
    void StaticSegmentClear() { }
}
```

Coree.Analyzers.CodeClarity reports CCCMB001 on `StaticSegmentClear` and says to put a blank line before that method. The developer inserts the line by hand. The same warning appears when the methods share one line, and when a comment, a documentation comment, or an attribute sits between them without an empty line above it. Typography can apply the ASCII character its warnings already name. CodeClarity has no code fix for this blank line.

## Change

CCCMB001 now has a code fix that inserts one blank line immediately after the previous method, using the line ending already in the file. A comment, documentation comment, attribute, or other text between the methods stays where it is, and the empty line is above it. Two methods on one line are split by that empty line; a same-line comment is kept. Methods that already have a blank line are left alone. The warning still reports until the fix is applied. Fix-all uses that same edit for each CCCMB001 in the document, the project, or the solution.

The repository README, the CodeClarity NuGet readme, the package notes, and `docs/codeclarity/user/method-blank-line.md` say the editor can insert that line. Which pairs report does not change.

## After

Applying the fix to the two methods in Before inserts a blank line between `Walkable` and `StaticSegmentClear`, and CCCMB001 is gone from `StaticSegmentClear`. A comment or attribute that sat between the methods stays below that line. Two methods on one line are split by an empty line. A pair that already had a blank line is left alone. The compiler does not apply the edit. Whether a developer applies the fix was not measured.

## Evidence

At `origin/main` `d6771796304860aed617c6351ed40adcff06729a`, `MethodBlankLineAnalyzer` reports CCCMB001 and its message says "Put a blank line before this method." `MethodBlankLineAnalyzerTests.MissingBlankLineReports` expects that warning for adjacent methods, a shared line, a comment, a documentation comment, and an attribute. A search of `src/prj/Coree.Analyzers.CodeClarity` found no `CodeFixProvider`. The CodeClarity test project referenced `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` and did not reference the code-fix testing package.

After the fixer, from `src/sln/Coree.Analyzers.CodeClarity`, `dotnet test -m:1` passed 144 tests. Coverlet reported 100% line, branch, and method coverage of `Coree.Analyzers.CodeClarity`. `dotnet build -m:1` succeeded with 0 errors. The DebugHost still reports its six sample warnings, including CCCMB001. `dotnet pack -m:1` wrote `src/prj/Coree.Analyzers.CodeClarity/bin/Pack/Coree.Analyzers.CodeClarity.1.0.11-gbaa7db670f.nupkg` with the analyzer dll under `analyzers/dotnet/cs`, props under `build/` and `buildTransitive/`, the readme, and no package dependencies. That dll contains `MethodBlankLineFixProvider`. The packed readme and release notes describe the blank-line fix. Referencing Workspaces reports RS1038 and does not fail the build; the DebugHost warnings show the command-line compiler still loads the diagnostics. Implementation commit: `2989565b1e0535e67e56b2d140ae4d1c17c76da6`. Documentation commit: `baa7db670f7291a1f3621b970cc2bc89250f8ebd`. Consumer uptake was not measured.

## Understood assignment and provenance

The user invoked execute-product-evolution by its skill path and did not name a preset, a product area, or a phase. Before any policy file was opened, Discover, Assess, and Select & Bound were each set to Conservative `Default.md`. No later instruction changed that.

## Discover — PolicyDiscover/Default.md + PolicyDiscover/Contract.md

- A: `char c = '…';` is offered a CTYEL001 fix that writes `...`, so the literal becomes `'...'`, which is not a character literal. `Escape` only rewrites an apostrophe in that token. Evidence: `TypographyAsciiReplacement.Escape`; `ApostropheInCharacterLiteralIsEscaped`; no character-literal ellipsis test.
- B: CCCRC001 says "Extract intermediate calculations into local variables" for every report, including `int Property => Raw + Raw + 1`, which has no place for a local until the arrow becomes a block. Evidence: `ReturnTypeComplexityAnalyzer.Rule`; `ExpressionBodiedReturnedValuesReport`.
- C: the README, the NuGet readme, and the CCCRC001 description say "conditional operators". The scorer and `docs/codeclarity/user/return-expression-complexity.md` count `?:` only. `&&` and `||` stay quiet. Several done issues left the scorer unchanged after a maintainer question about `||`. Evidence: `ReturnExpressionComplexity.cs`; domain clue S-4e18.
- D: CCCMB001 tells the developer to put a blank line before the method, and nothing in the editor does it. Evidence: the message above; no `CodeFixProvider` under CodeClarity.
- E: the CCCPL001 description names methods, constructors, and primary constructors, and does not name local functions. The analyzer registers local functions, and the README includes them. Evidence: `ParameterListLayoutAnalyzer` description and `Initialize`.

Not counted: an empty Typography `Includes` value is replaced with `**` when it is empty at props import, and a later empty value in the project body still turns scanning off. Empty and omitted look the same at that condition, so one small edit cannot keep both promises. The CodeClarity sample that shows version `0.1.2` already says that is the version shown, and `dotnet add package` without a version is on the same page. Operator parameter lists are outside the published declaration set. CTYED001 names only the en dash and the em dash. A `//` comment after `)` stays quiet because the published CCCPL002 sentence says a list that contains `//` stays quiet.

## Assess — PolicyAssess/Default.md + PolicyAssess/Contract.md

- A is a broken edit of an existing fix. It adds no warning, and a test can show the fix is withheld while the warning remains. It is the smallest reliability repair.
- B is accurate advice only after the member becomes a block. Changing the sentence does not apply an edit. The current sentence still names the repair the rule wants.
- C is a wording mismatch. The user page already says `?:`. Changing the scorer would add warnings that earlier decisions left out on purpose.
- D adds an optional edit and does not change which pairs report. The inserted line is the one the warning already asks for. Rollback is removing the fixer. The test surface is the existing blank-line cases plus the edit. Referencing Workspaces can produce RS1038, which Typography already builds with; that warning is not in `WarningsAsErrors`.
- E is a description that lags code the README already states. The warning still fires.

## Select & Bound — PolicySelectBound/Default.md + PolicySelectBound/Contract.md

A was ahead of D on the repair itself. Before this issue, branch, or worktree existed for this run, `feature/pevo/char-ellipsis-261026` was already checked out at `.worktrees/char-ellipsis` with an untracked `Issues/open/`. That claims the character-literal ellipsis outcome, so A is not selected.

D is selected over B, C, and E. Those three change wording around behavior that already happens. D lets the developer apply the blank line the warning names.

Included: the CCCMB001 code fix, document/project/solution fix-all for that diagnostic, focused tests, the Workspaces reference kept private so it is not a package dependency, and the README, NuGet readme, package notes, and CodeClarity user page that describe the edit. Excluded: any change to when CCCMB001 reports, fixes for other CodeClarity diagnostics, Typography, severity, diagnostic ids, line-ending detection, and the candidates left above.

## Selected improvement and baseline

Consumers are C# developers who see CCCMB001. The warning, its id, and its severity stay. A pair that already has a blank line stays quiet and is not given another line. The compiler does not apply the edit.

Acceptance: a missing blank line, including two methods on one line and a comment or attribute between them, can be fixed to one empty line above that following text, and CCCMB001 is gone from that method. The file's line ending is kept. An existing blank line is not offered a second one. `dotnet test` from `src/sln/Coree.Analyzers.CodeClarity` passes, with Coverlet still at 100% line, branch, and method coverage of `Coree.Analyzers.CodeClarity`. Rollback is reverting the fixer, its tests, the private Workspaces reference, and the notes.

Integration base: `origin/main` `d6771796304860aed617c6351ed40adcff06729a`. Local branch: `feature/pevo/blank-line-fix-261026`. Worktree: `.worktrees/blank-line-fix`. The issue-only commit `1fa6c00c7e12d4eeaf6ffc6c4107e33f17899cb6` was published to `origin/feature/pevo/blank-line-fix-261026` before product edits.

## Final outcome

Status: Done. The blank-line fix is implemented and the checks above passed. Done means verified and ready for review, not merged.

Publication of this closure commit, pull-request creation, merge, and worktree cleanup are pending. The remote branch still points at the issue-only commit until that publication.

Rollback remains reverting the fixer, its tests, the private Workspaces reference, and the notes.

## Incidental Discoveries (Out of Scope)

- `char c = '…';` is offered three periods. Noticed in `TypographyAsciiReplacement.Escape`. Already claimed by `feature/pevo/char-ellipsis-261026`.
- CCCRC001's extract-locals sentence does not say that an arrow has to become a block first. Noticed on `ReturnTypeComplexityAnalyzer.Rule` while reading expression-bodied reports.
