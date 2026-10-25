## Before

A source file contains a word-processor opening single quote, U+2018, as in ‘quoted’. Typography warns on the closing U+2019 through CTYAP001 and stays silent on U+2018. U+201A and U+201B, the single counterparts of the double quotation marks U+201E and U+201F, are silent too. An English ‘quote’ is therefore only half reported. A German ‚quote‘ (U+201A … U+2018) is not reported at all. ASCII quotes and apostrophes are not involved.

## Change

Planned: report U+2018, U+201A, and U+201B as CTYQM001 quotation marks, together with the curly double quotes and guillemets that rule already reports. Leave U+2019 on CTYAP001 alone, so a closing single quote is not reported twice. Keep ASCII quotation marks and apostrophes silent. Cover those cases with analyzer regressions, and name the split in the Typography descriptions and package release notes.

## After

Not implemented yet.

## Evidence

At `origin/main` `ab21473c4e2051b8b0cc2abcbe38ddb35e7f7c8d`, `SmartQuotesAnalyzer` looks only for U+201C, U+201D, U+201E, U+201F, U+00AB, and U+00BB. `ApostropheAnalyzer` looks only for U+2019. The scanner reports a character only when it is in that set. Existing functional tests cover U+201C and U+2019, not U+2018, U+201A, or U+201B. The published CTYQM001 text says curly quotes and guillemets. The runtime result and any consumer effect are not measured yet.

## Understood assignment

The user invoked product evolution without a named preset or a narrower product goal: one bounded improvement, recorded before implementation, published on its own feature branch, and not merged. Before any policy file was opened, Discover, Assess, and Select & Bound were each set to Conservative `Default.md`, because the invocation did not name a preset or tailor a phase. A later instruction said to follow this skill directly and dropped an earlier coordination overlay. That overlay did not change the three policy sources. `Balanced.md` and `Ambitious.md` were not read.

## Discover — PolicyDiscover/Default.md + PolicyDiscover/Contract.md

- A: CTYQM001 omits the single quotation marks U+2018, U+201A, and U+201B while it already includes the parallel double marks, and CTYAP001 covers only U+2019. Evidence: `SmartQuotesAnalyzer.cs`, `ApostropheAnalyzer.cs`, the README diagnostics table, `FunctionalTests.cs`.
- B: CCCPL001 judges a broken list from each parameter's start line. A multiline last parameter can be reported even when `)` follows that parameter's end, while the same multiline parameter in an earlier position can stay quiet. Evidence: `ParameterListLayout.cs`. The written rule does not settle which result is intended.
- C: addition is ignored whenever either operand's type is string, which can also be true of a user-defined operator. Evidence: `ReturnExpressionComplexity.cs`. No test shows that operator case.
- D: the CodeClarity package release notes stop at the 0.1.5 entry and do not mention CCCPL002, CCCNS001, or CCCMB001, although the README and the analyzers already describe them.

Claimed or already implemented outcomes were not counted: `nameof` versus a method call, method blank-line newline handling, unsigned right-shift scoring, literal whitespace inside CCCPL002 lengths, and comma-containing Typography globs. Refreshed `origin` heads and the pull-request list showed no request and no branch for these quotation marks. Expression-bodied members that exceed the return limit stay quiet in `SingleCalculationStepIsSimple`, so that silence is the current contract.

## Assess — PolicyAssess/Default.md + PolicyAssess/Contract.md

- A is a direct gap in an existing character set. The change is local, keeps the current id and severity, and can be checked against U+2019 and ASCII. Files that contain the three marks will start receiving CTYQM001. That matches the published curly-quote wording; it does not remove a documented exemption. A revert undoes it.
- B is a real inconsistency, but the repair has to choose a layout policy the docs and tests leave open, and it can add or remove CCCPL001 warnings on ordinary signatures.
- C matters only for an uncommon operator and can disturb ordinary string concatenation.
- D is a notes lag. The README already states what those diagnostics do, and the notes file uses handwritten version headings.

## Select & Bound — PolicySelectBound/Default.md + PolicySelectBound/Contract.md

A is selected. Its result and check are clearer than B, C, and D. The outcome is that CTYQM001 reports U+2018, U+201A, and U+201B, while CTYAP001 remains the only reporter of U+2019. Include the character set, focused analyzer tests, and the Typography descriptions and release notes that state this split. Exclude single guillemets U+2039 and U+203A, any other new character, glob matching, severity, includes and excludes, diagnostic ids, and CodeClarity. A fresh remote check before this issue found no branch with the same outcome.

## Acceptance criteria

1. A syntax tree containing U+2018, U+201A, or U+201B reports CTYQM001 on that character and does not report CTYAP001.
2. U+2019 still reports CTYAP001 and does not report CTYQM001.
3. ASCII `"` and `'` stay silent for both analyzers.
4. Existing double quotes, guillemets, severity settings, and extra-file selection stay as they are.
5. From `src/sln/Coree.Analyzers.Typography`, restore, a serialized build, the full test run with its coverage threshold, and pack all succeed.
6. The repository README, the Typography NuGet readme, and the Typography package release notes name these three marks as CTYQM001 and keep U+2019 on CTYAP001.

Consumers are people compiling C# and extra project files with Typography. Diagnostic ids, public analyzer types, and severity property names stay unchanged. The new warnings are the correction. Rollback is reverting the character set, tests, and accompanying notes together. Real consumer uptake is unmeasured.

## Baseline

Status: Open.
Integration target: `origin/main` at `ab21473c4e2051b8b0cc2abcbe38ddb35e7f7c8d`, fetched 2026-10-25.
Feature branch: `feature/pevo/single-quotes-261025`.
Worktree: `.worktrees/single-quotes-261025`.
This file is the only content of the first commit. That commit is published and the remote ref checked before any product edit.

## Incidental discoveries (out of scope)

- CCCPL001's start-line check, described as B, is unresolved. Another branch, `feature/pevo/parameter-list-end-261025`, has since recorded that question; this run does not take it.
- A Typography `Includes` value left empty in `Directory.Build.props` can be replaced by the package props default `**`, because those props import later and treat empty as unset. The same empty value in the project body, after the import, is not replaced. Whether anyone depends on the early empty value was not established.
- `SmartQuotesAnalyzer`'s type comment still says it is a sample and tells the reader to replace the type.
- CodeClarity's package release notes, described as D, still omit diagnostics the README already documents.
