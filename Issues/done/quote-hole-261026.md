## Before

A C# file contains a curly double quote in a normal string that sits in an interpolation hole of a verbatim interpolated string:

```csharp
class C { string s = $@"pre { "x“y" } post"; }
```

Coree.Analyzers.Typography reports CTYQM001 and offers a fix. Applying it doubles the quote, because the fixer treats the hole as part of the outer `$@"` string. The source becomes `$@"pre { "x""y" } post"`, and the inner literal no longer parses.

The same quote in a hole of a raw interpolated string is not offered a fix:

```csharp
class C { string s = $"""pre { "x“y" } post"""; }
```

The warning still reports. The published rule for a normal string is to write the straight quote as `\"`. The hole is a normal string. The outer raw string is why the fix is withheld.

A curly quote in the text of `@"..."`, `$@"..."`, `"..."`, or `$"..."` already follows that text's own literal. A raw string that is not an interpolated hole is still left unchanged.

## Change

The fixer now escapes the literal that contains the character. `OwningInterpolatedString` skips an interpolated string reached through an interpolation hole, and `IsRawString` and `IsVerbatim` use that owner.

A normal string inside a hole is escaped for that inner literal. A straight double quote there is written as `\"`, including when the outer string is verbatim or raw. A verbatim string inside a hole still doubles the quote, including when that hole is in a raw interpolated string. Text that belongs to the outer string is unchanged: `$@"x“y"` still doubles, and `$"""x“y"""` is still not edited.

Reported characters, diagnostic ids, and severity are unchanged. UTF-8 string suffixes were not part of this change. The repository README, the Typography NuGet readme, the package notes, and `docs/typography/user/ascii-replacement.md` say that a hole follows the inner literal.

## After

Applying the fix to `class C { string s = $@"pre { "x“y" } post"; }` writes `$@"pre { "x\"y" } post"`. The inner string still parses, and CTYQM001 is gone from that quote.

Applying it to `class C { string s = $"""pre { "x“y" } post"""; }` writes `$"""pre { "x\"y" } post"""`. A curly quote in the text of `$"""x“y"""` still reports CTYQM001 and is not offered a fix. A verbatim string in that same kind of hole, `$"""pre { @"x“y" } post"""`, still doubles the quote.

The compiler does not apply the edit. Whether a developer uses the fix was not measured.

## Evidence

Before the change, at `origin/main` `f8e6c5f3e35f864aad9613843eacd0cee0c16ce9`, `IsRawString` and `IsVerbatim` walked to the first interpolated string and ignored an `InterpolationSyntax` in between. Roslyn 4.14.0 parses the hole quote as a `StringLiteralToken` under that outer string.

After the change, from `src/sln/Coree.Analyzers.Typography`, `dotnet test` passed 94 tests. Coverlet reported 100% line, branch, and method coverage of `Coree.Analyzers.Typography`. `QuoteInVerbatimInterpolationHoleIsEscaped`, `QuoteInRawInterpolationHoleIsEscaped`, `VerbatimStringInsideInterpolationHoleDoublesQuote`, `VerbatimStringInsideRawInterpolationHoleDoublesQuote`, `TwoQuotesInOneInterpolationHoleStayInsideTheInnerLiteral`, `RawInterpolatedTextIsNotOfferedAFix`, and `RawTextStaysWhileHoleQuoteIsEscaped` cover the hole. The existing normal, verbatim, raw, and interpolated-text cases still pass. Consumer uptake is unmeasured.

## Understood assignment and provenance

The user invoked execute-product-evolution by its skill path and did not name a profile, a product area, or a phase. Before any policy file was opened, Discover, Assess, and Select & Bound were each set to `Default.md` (Conservative). The user did not add a phase constraint. That choice did not change.

## Discover → Assess → Select & Bound — decision trail

### Discover — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicyDiscover/Contract.md` and `Default.md`.

Read the repository README, the package readmes, the Typography fixer, the CodeClarity analyzers and tests, the user pages, the domain notes, and `Issues/done/`. Fetched `origin`. `origin/main` is `f8e6c5f3e35f864aad9613843eacd0cee0c16ce9`. GitHub issues and pull requests were empty. Local work already claims two outcomes, recorded below; those are not candidates.

- A: a curly quote in a normal string inside an interpolation hole is escaped as the outer string. A verbatim outer string doubles it and breaks the inner literal. A raw outer string offers no fix. Evidence: `TypographyAsciiReplacement.IsVerbatim` and `IsRawString`; the two parses above.
- B: `char c = '…';` is offered a fix that inserts `...`, so the literal becomes `'...'`, which is not a character literal. `Escape` only rewrites an apostrophe in that token. Evidence: `TypographyAsciiReplacement.Escape`; Roslyn token kind `CharacterLiteralToken` for U+2026.
- C: CCCRC001's message says to extract locals for every report, including an arrow such as `int Property => a + b + c`, which has nowhere to put a local. Evidence: the message on `ReturnTypeComplexityAnalyzer.Rule`; `AnalyzePropertyArrow` and `ExpressionBodiedReturnedValuesReport`.
- D: the README, the NuGet readme, and the CCCRC001 description say "conditional operators". `ReturnExpressionComplexity` counts `?:` only. `docs/codeclarity/user/return-expression-complexity.md` already says `?:`. Several done issues left `&&` and `||` unscored after a maintainer question about `||`.
- E: an empty Typography `Includes` value is replaced with `**` when it is empty at props import. The README says an empty includes value turns extra-file scanning off. `EmptyIncludesPropertyReportsNothing` shows that an empty compiler-visible property does that. Whether `Directory.Build.props` is imported before those props was not rechecked in this session.

### Assess — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicyAssess/Contract.md` and `Default.md`.

- A writes source that does not parse, or withholds the fix the normal-string rule already states. The change is the parent walk in the escape helper. Existing text-of-string behavior has tests. Rollback is reverting that walk, its tests, and the notes. How often holes contain typographic quotes was not measured.
- B is the same class of broken edit, limited to an ellipsis in a character literal. One extra length check would stop it. It is rarer than a quoted string in a hole.
- C misleads every complex arrow the diagnostic already reports. It does not change what is reported and does not break a build. The wording has to stay true for a `return` inside a block as well.
- D would restate a score earlier cycles left unchanged. The user page already names `?:`. Changing the scorer would warn on returns the tests leave quiet.
- E is a real props condition. The import order that would make a repo-wide empty value lose was not rechecked here. Distinguishing unset from empty in MSBuild can change the default for projects that never set the property.

A is the sharper failure that is still unclaimed: the fix is offered and the result does not parse.

### Select & Bound — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicySelectBound/Contract.md` and `Default.md`.

Two other outcomes were already claimed at the overlap recheck and were not selected. `feature/pevo/u8-string-fix-261026` in `.worktrees/u8-string-fix` has an open issue for UTF-8 string suffixes. `feature/pevo/parameter-doc-comment-261026` is on `origin` and records CCCPL002 staying quiet for a `///` documentation comment. Neither is this hole.

Select A. A literal in an interpolation hole is escaped for that inner literal. Outer-string text keeps the behavior it has today.

Include the escape helper, tests for a normal string and a verbatim string inside a verbatim hole and a raw hole, a regression that outer `$@"` text still doubles and outer `$"""` text is still not offered a fix, and the README, NuGet readme, package notes, and `docs/typography/user/ascii-replacement.md` sentences that describe string escaping.

Exclude B, C, D, E, UTF-8 suffixes, CodeClarity, new diagnostic ids, the reported character set, severity, includes and excludes, and additional-file editing.

## Selected improvement and baseline

Consumers are C# developers who apply the Typography lightbulb or fix-all on a quote inside an interpolation hole. Diagnostic ids, reported characters, and severity stay. Text of an outer string stays on its current rule. A raw string's own text is still not edited.

Integration target is `origin/main` at `f8e6c5f3e35f864aad9613843eacd0cee0c16ce9`. Local branch: `feature/pevo/quote-hole-261026`. Worktree: `.worktrees/quote-hole`. The early push of the issue commit is recorded in the final outcome. At the time this paragraph was first committed, that push had not been done.

### Acceptance criteria

1. CTYQM001 on U+201C inside a normal string in a `$@"` hole fixes to `\"`, and the outer verbatim string is otherwise unchanged.
2. CTYQM001 on U+201C inside a normal string in a `$"""` hole fixes to `\"`. U+201C in the raw text of that same kind of string is still not offered a fix.
3. A verbatim string inside a `$@"` hole still doubles a straight quote.
4. `$@"x“y"` still doubles, `$"x“y"` still uses `\"`, a normal string still uses `\"`, a verbatim string still doubles, and a raw string without a hole is still not offered a fix.
5. From `src/sln/Coree.Analyzers.Typography`, `dotnet test` passes with the configured 100% line, branch, and method coverage of `Coree.Analyzers.Typography`.

Rollback is reverting the escape helper, the new tests, and the notes that mention interpolation holes.

## Final outcome

Status: Done. The implementation is verified. It is not merged.

The fixer, its tests, and the notes were implemented as selected. A verbatim string inside a raw interpolated hole was locked as well; that is the same inner-literal rule, not a new character set or diagnostic. UTF-8 suffixes, CodeClarity, and the other assessed candidates stayed out.

Commits on `feature/pevo/quote-hole-261026`:

- `f0dbc3620c038f269d3a0d429b54f917bd1c69fb` records this issue. That commit was pushed and `origin/feature/pevo/quote-hole-261026` matched it before the fixer change.
- `aa70e3856640a64dfbfc786ede816cf6a4f52164` escapes quotes inside interpolation holes.
- `89574220f2f6eac618f1f36c56ae2043b36fa134` locks a verbatim quote inside a raw hole.
- `74f11252e159259746ea8267c659802b552e0430` describes the hole rule in the README, the NuGet readme, the user page, and the package notes.
- `89d5a5544fa4a218c47556bcc645e572773b3378` moved this file from `Issues/open/` to `Issues/done/` before this verified text was committed. The verified Change, After, and Evidence sections are in the following commit. That commit's id is not written here.

Verification: `dotnet test` from `src/sln/Coree.Analyzers.Typography` passed 94 tests, with 100% line, branch, and method coverage of `Coree.Analyzers.Typography`. RS1038 remains the existing Workspaces warning on the analyzer project. Consumer uptake is unmeasured.

Publication of this Done commit, pull-request creation, merge, and worktree cleanup are not done yet.

Rollback is reverting this branch.

## Incidental Discoveries (Out of Scope)

- `char c = '…';` is offered a replacement of three periods. That was noticed in `TypographyAsciiReplacement.Escape` and was not selected.
- CCCRC001 tells an expression-bodied property to extract local variables. The message is on `ReturnTypeComplexityAnalyzer`. Whether a second message for arrows is worth a later change was not decided.
