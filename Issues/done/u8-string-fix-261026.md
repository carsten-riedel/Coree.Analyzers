## Before

A C# file contains a curly quote inside a UTF-8 string literal:

```csharp
class C { var s = "x“y"u8; }
```

Coree.Analyzers.Typography reports CTYQM001 on that quote and offers a fix. Applying it writes a raw `"` into the literal, so the source becomes `"x"y"u8` and no longer parses as one string. The same fix leaves a raw string that has a `u8` suffix editable. `"""x“y"""u8` is a raw string, and the package already says a raw string is not edited, but the fixer does not recognize the UTF-8 raw token and will still insert the quote.

A normal string without `u8` is escaped (`\"`). A verbatim string doubles the quote. A raw string without `u8` is not offered a fix. The `u8` suffix is the case that falls through.

## Change

A UTF-8 string literal is now treated the same way as the ordinary literal it suffixes.

- `"..."u8` uses the same escaping as a normal string. A straight double quote is written as `\"`. An apostrophe, hyphen, ellipsis, or space is written as that character.
- `@"..."u8` is the same token kind with a verbatim prefix, so a double quote is written as `""`.
- `"""..."""u8`, on one line or across lines, is not offered a fix. The diagnostic still reports.

Reported characters, diagnostic ids, and severity stay as they are. A raw string without `u8`, a verbatim string without `u8`, a comment, and an interpolated string keep the behavior they already have. The repository README, the Typography NuGet readme, the package notes, and `docs/typography/user/ascii-replacement.md` say that the `u8` suffix follows those rules. `@"..."u8` was not a separate candidate; it is the verbatim form of `Utf8StringLiteralToken`, and leaving it on the old path would still have inserted a raw quote.

## After

Applying the fix to `class C { void M() { var s = "x“y"u8; } }` writes `"x\"y"u8`, and CTYQM001 is gone from that span. Two curly quotes in one `u8` literal both become escaped quotes, and the literal still ends at its original closing quote. An em dash in `"a—b"u8` becomes `-`. A typographic apostrophe in a `u8` string becomes `'` with no backslash. `@"x“y"u8` becomes `@"x""y"u8`.

`"""x“y"""u8` and a multi-line `"""..."""u8` that contain U+201C still report CTYQM001 and are not offered a fix. A normal string, a verbatim string without `u8`, a raw string without `u8`, and a comment behave as they did before. Whether a developer applies the fix was not measured.

## Evidence

At `origin/main` `f8e6c5f3e35f864aad9613843eacd0cee0c16ce9`, `TypographyAsciiReplacement.Escape` treats `StringLiteralToken` and `InterpolatedStringTextToken` as strings, and `SingleLineRawStringLiteralToken` and `MultiLineRawStringLiteralToken` as raw strings. It does not name the UTF-8 token kinds.

Roslyn 4.14.0, language version C# 13, parses the suffixes as different tokens:

- `"“hello”"u8` is `Utf8StringLiteralToken`
- `"""x“y"""u8` is `Utf8SingleLineRawStringLiteralToken`
- a multi-line `"""..."""u8` is `Utf8MultiLineRawStringLiteralToken`

`TypographyCharacterFixTests.QuoteInNormalStringIsEscaped` locks `\"` for a normal string. `RawStringIsNotOfferedAFix` locks the no-fix result for a raw string without `u8`. Neither sample has a `u8` suffix. Before the change, the fall-through at the end of `Escape` inserted the quote with no backslash and still offered the fix.

After implementation `8ae5a6126a9d3dee901222098bdfc5b92944f4cf`, from `src/sln/Coree.Analyzers.Typography`: `dotnet test --verbosity minimal -m:1` passed 93 tests. Coverlet reported 100% line, branch, and method coverage of `Coree.Analyzers.Typography`. The new tests are `QuoteInUtf8StringIsEscaped`, `TwoQuotesInOneUtf8StringStayInsideTheLiteral`, `EmDashInUtf8StringFixesToHyphen`, `ApostropheInUtf8StringNeedsNoBackslash`, `QuoteInVerbatimUtf8StringIsDoubled`, and `Utf8RawStringIsNotOfferedAFix`. The analyzer project still warns RS1038 because it references Workspaces. Consumer uptake is unmeasured.

## Understood assignment and provenance

The user invoked execute-product-evolution by its skill path and did not name a profile, a product area, or a phase. Before any policy file was opened, Discover, Assess, and Select & Bound were each set to `Default.md` (Conservative). The user did not add a phase constraint. That choice did not change.

## Discover → Assess → Select & Bound — decision trail

### Discover — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicyDiscover/Contract.md` and `Default.md`.

Read the repository README, both package readmes, the solution notes, the analyzer and fixer source, the Typography and CodeClarity tests, `docs/typography/user/ascii-replacement.md`, `docs/codeclarity/user/`, the domain notes, and `Issues/done/`. Fetched `origin`. The only remote head is `main` at `f8e6c5f3e35f864aad9613843eacd0cee0c16ce9`. GitHub issues and pull requests were empty. No `feature/pevo/` branch exists.

Outcomes already on `main` were not candidates. That includes the Typography fixer for ordinary strings, verbatim strings, character literals, and raw strings without `u8`, and the CodeClarity rules already described in `Issues/done/`.

- A: a curly quote in `"..."u8` is fixed by inserting a raw `"`, and a raw string with a `u8` suffix is not recognized as raw. Evidence: `TypographyAsciiReplacement.Escape`; the three token kinds above.
- B: CCCPL002 stays quiet for `//` and still reports when the list contains `///`. That documentation comment is `SingleLineDocumentationCommentTrivia`, so `ParameterListFit.ContainsLineComment` does not see it. A short wrapped list would still recommend one line, and that line would comment out the rest of the signature. Evidence: `ParameterListFit.cs`; a C# 13 parse of `void M(int a, /// note, int b)`. `Issues/done/parameter-line-comment-261025.md` kept block comments reporting and did not name `///`.
- C: the CodeClarity install sample and both analyzer projects still pin `Coree.Analyzers.CodeClarity` 0.1.2. NuGet lists 1.0.5 as the newest version. The same README describes CCCPL001, CCCPL002, CCCNS001, and CCCMB001. Copying the sample does not install those rules. `dotnet add package` without a version is also on the page. Earlier issues left the pin in place because bumping the in-repo reference can fail the build.
- D: an empty `*Includes` value set before the Typography props import is replaced with `**`. Empty and unset look the same at that moment. An empty value written in the project body, after the import, still turns extra-file scanning off. Evidence: `EmDashAnalyzer.props` and the five sibling props; the README sentence that an empty includes value turns scanning off.
- E: the README, the NuGet readme, and the CCCRC001 description say "conditional operators". The user page and the scorer count `?:` only. `return a > b && a >= b` stays quiet in `SingleCalculationStepIsSimple`. Several done issues left that scorer unchanged after a maintainer question about `||`.
- F: a normal string inside a hole of a raw interpolated string is still `StringLiteralToken`, but `IsRawString` walks up to the raw interpolated parent and refuses the fix. The published rule says a raw string is not edited. Whether a hole is part of that promise was not decided in `Issues/done/typography-fix-261025.md`.

### Assess — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicyAssess/Contract.md` and `Default.md`.

- A is a reliability defect in a fix the editor applies. The ordinary-string and raw-string rules already exist; the `u8` tokens are the missing counterparts. The change stays in the escape helper. Rollback is reverting that helper, its tests, and the notes. The practical frequency of `u8` literals was not measured.
- B is the same false recommendation as the `//` fix, for a less common place to put `///`. It does not edit source by itself. Low risk, and smaller than A only because the developer still has to follow the warning.
- C misleads someone who copies the sample. The unversioned `dotnet add` line already tracks the current package. The newest published version may still lag the README, and the in-repo 0.1.2 pin was left on purpose. A doc-only version can go stale on the next publish.
- D is real for a value set in `Directory.Build.props`. A project-body empty value and `AdditionalExcludes` already work. Telling unset from empty needs a new token, which is a new consumer spelling. A note about import order would not make the early empty value work.
- E restates a locked score. The user page already names `?:`. Changing the scorer would warn on returns the tests leave quiet. Earlier cycles passed over a text-only restatement.
- F may be a hole in the raw-string refusal, or it may be what "do not edit a raw string" already means. Editing a hole can break the delimiter if the parent walk is wrong. That is a wider risk than naming the three UTF-8 token kinds.

A has the sharper failure: the fix writes source that does not parse, and it edits a raw string the package said it would leave alone.

### Select & Bound — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicySelectBound/Contract.md` and `Default.md`.

Select A. A UTF-8 string literal is escaped like a normal string, and a UTF-8 raw string is left alone like any other raw string.

Include the escape helper, regressions for a curly quote and an em dash in `"..."u8`, a single-line and a multi-line `"""..."""u8` that are not offered a fix, and the README, NuGet readme, package notes, and `docs/typography/user/ascii-replacement.md` sentences that already describe string escaping and the raw-string exception.

Exclude B, C, D, E, F, CodeClarity, new diagnostic ids, the reported character set, severity, includes and excludes, and additional-file editing.

The overlap recheck after `git fetch --prune origin` found no Typography UTF-8 fixer issue, pull request, or `feature/pevo/` branch. `Issues/done/typography-fix-261025.md` covers the fixer for strings without a `u8` suffix.

### Selected improvement and baseline

Consumers are C# developers who apply the Typography lightbulb or fix-all in a document that contains a `u8` string. Diagnostic ids, reported characters, and severity stay. Non-`u8` replacements stay. A raw string, with or without `u8`, is still not edited.

Integration target is `origin/main` at `f8e6c5f3e35f864aad9613843eacd0cee0c16ce9`. Local branch: `feature/pevo/u8-string-fix-261026`. Worktree: `.worktrees/u8-string-fix`. The issue-only baseline `f151ce5958001a021d61e0ec00bb0fa7eac4c60e` was pushed to `origin/feature/pevo/u8-string-fix-261026` before the fixer change. The implementation commit is `8ae5a6126a9d3dee901222098bdfc5b92944f4cf`. Publication of this closure commit, and removal of the worktree, are not done yet.

### Acceptance criteria

1. CTYQM001 on U+201C inside `"..."u8` fixes to `\"`, and that diagnostic is gone from the span. Two such quotes in one `u8` literal both escape, and the literal still ends at the original closing quote.
2. CTYED001 on U+2014 inside `"..."u8` fixes to U+002D.
3. A typographic apostrophe inside `"..."u8` fixes to `'` with no backslash.
4. `"""x“y"""u8` and a multi-line `"""..."""u8` that contain U+201C still report CTYQM001 and are not offered a fix.
5. A normal string still escapes `"`, a verbatim string still doubles it, a raw string without `u8` is still not offered a fix, and a comment still keeps the raw character.
6. From `src/sln/Coree.Analyzers.Typography`, `dotnet test` passes with the configured 100% line, branch, and method coverage of `Coree.Analyzers.Typography`.

Rollback is reverting the escape helper, the new tests, and the notes that mention the `u8` suffix.

### Incidental Discoveries (Out of Scope)

- CodeClarity `PackageReleaseNotes.txt` does not name CCCNS001. The repository README already describes that diagnostic. Whether an older notes section was dropped on purpose was not checked.

## Final outcome

Status: Done. The implementation is verified and not merged.

`Escape` now recognizes `Utf8StringLiteralToken`, `Utf8SingleLineRawStringLiteralToken`, and `Utf8MultiLineRawStringLiteralToken`. The verbatim `u8` form was included because `IsVerbatimStringLiteral` is already true for `@"..."u8`; skipping it would have left that token on the path that inserts a raw quote. No other scope was added.

From `src/sln/Coree.Analyzers.Typography`, `dotnet test --verbosity minimal -m:1` passed 93 tests with 100% line, branch, and method coverage of `Coree.Analyzers.Typography`. Existing non-`u8` fix tests in `TypographyCharacterFixTests` still passed. Real consumer uptake was not measured.

Rollback is reverting `8ae5a6126a9d3dee901222098bdfc5b92944f4cf`. The remote feature branch existed at the issue baseline before this closure. Pushing this commit, and deleting the local worktree, are still pending.
