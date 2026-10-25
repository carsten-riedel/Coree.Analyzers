## Before

CodeClarity's CCCMB001 reports a missing blank line between two methods even when an empty line separates them in a C# file using carriage-return-only or Unicode line endings. `MethodBlankLine.HasBlankLine` splits only on LF, so these valid source lines are never examined. Consumers cannot satisfy the diagnostic without changing their file's line endings.

## Change

Planned: use Roslyn's source-text line boundaries to recognize blank lines consistently with the compiler. Add contrasting analyzer tests for separated methods, adjacent methods, whitespace-only lines, comments, and mixed line endings. Keep the existing rule and configuration intact.

## After

Not implemented yet.

## Evidence

At base `ab21473c4e2051b8b0cc2abcbe38ddb35e7f7c8d`, [MethodBlankLine.cs](../../src/prj/Coree.Analyzers.CodeClarity/MethodBlankLine.cs) uses `gap.Split('\n')`; its existing carriage-return test actually supplies CRLF. [C# line terminators](https://learn.microsoft.com/dotnet/csharp/language-reference/language-specification/lexical-structure#632-line-terminators) include CR, LF, CRLF, NEL, line separator, and paragraph separator. The existing [domain contract](../../docs/agent-domain-understanding/context/method-blank-line.md) defines an empty or whitespace-only line without limiting line endings. Runtime reproduction and post-change verification are pending. Consumer benefit has not been measured.

## Assignment and status

- User invoked `execute-product-evolution` without a profile or a specific feature request, authorizing one autonomous bounded improvement, issue recording, commits, branch publication, and temporary workspace cleanup. No PR or integration is requested.
- Status: Open. Date: 2026-10-25.
- All three phase strategies were selected as Default (Conservative) before reading policy references; there were no refinements or policy changes.

## Discover — Default.md + Contract.md

Policy: `.agents/skills/execute-product-evolution/references/PolicyDiscover/{Contract,Default}.md`.

Read both package introductions, solution build/test instructions, relevant domain records, analyzer implementations and relevant tests. Four concrete opportunities survived screening. Open/closed GitHub issues, open PRs, local Issues files and local/remote branches showed no overlapping work, including the refreshed pre-selection check.

- A: CCCMB001 recognizes only LF-containing line endings. Source and intended outcome are described above; existing tests cover LF/CRLF but not the other C# terminators.
- B: CCCPL002's `ParameterListFit.Normalize` collapses all whitespace in parameter text, including spaces inside default string literals. This can underestimate the width and incorrectly recommend collapsing a long parameter list. Exact boundary scenarios remain untested.
- C: CCCPL001's `ParameterListLayout.IsDisallowed` uses parameter start lines when checking the closing-parenthesis position and shared lines. Multiline parameter declarations can therefore disagree with the documented shape. Intended treatment of multiline literals/attributes would need clarification.
- D: CCCRC001's `ReturnExpressionComplexity.IsNameof` recognizes only the identifier's value text. Calls to a user-defined escaped `@nameof` method are treated as the language operator and not counted. Semantic disambiguation and error-state behavior would need verification.

## Assess — Default.md + Contract.md

Policy: `.agents/skills/execute-product-evolution/references/PolicyAssess/{Contract,Default}.md`.

- A: Strong source/specification evidence, directly removes a false diagnostic for valid source. Narrow change, low semantic risk, and a complete positive/negative test matrix is feasible with existing Roslyn tests. Less common line endings limit the affected population; there is no usage data.
- B: Plausible practical value for string defaults, but preserving literal text while normalizing trivia introduces more syntax and comment-boundary decisions. Defer rather than combine it with A.
- C: Observable mismatch, but larger ambiguity around whether multiline parameters should be accepted at all. Lower confidence and broader rule-definition consequences than A.
- D: Focused false-negative opportunity, but less common syntax and more semantic-context investigation than A. Deferred.

## Select & Bound — Default.md + Contract.md

Policy: `.agents/skills/execute-product-evolution/references/PolicySelectBound/{Contract,Default}.md`.

Choose A for its clear existing contract, minimal consumer disruption, and decisive verification. Include only blank-line boundary recognition, regression tests, directly affected explanation/release notes, and required local operational records. Exclude B/C/D, changes to other analyzers, code fixes, severity changes, dependency updates, release/version changes, CI changes, and merging.

### Acceptance criteria

1. Empty and whitespace-only lines between consecutive methods stay quiet for LF, CRLF, CR, U+0085, U+2028, U+2029, and mixed supported line endings.
2. A single line break, comment-only gap, or next-method indentation still reports CCCMB001 on the later method. CRLF remains one line break.
3. Existing member/body exclusions, severity settings, messages and locations remain protected by the existing suite.
4. Demonstrate the regression against the original implementation and the passing result after repair. The complete CodeClarity suite must retain its required 100% line/branch/method coverage; build and pack the changed package.
5. Review the complete diff for bounded scope and preserve the caller's branch and all existing work.

### Baseline, risk, and rollback

- Integration target: `main`; refreshed base: `origin/main` at `ab21473c4e2051b8b0cc2abcbe38ddb35e7f7c8d`.
- Feature branch: `feature/pevo/method-newlines-261025`.
- Temporary linked worktree: `.worktrees/method-newlines-261025`, ignored through the local shared Git exclude file.
- Early issue-only branch publication: pending; required before product edits.
- Consumers: CodeClarity users in IDE and command-line builds, especially imported or generated source with uncommon line terminators. Generated-code exclusion remains unchanged.
- Main risk: accidentally counting indentation or a CRLF pair as a blank line. Contrast tests must distinguish complete interior lines from partial first/last lines.
- Rollback: revert the implementation and documentation commits; no data migration or public API change.
- Verification uses existing MSTest/Roslyn infrastructure and solution commands. No consumer validation or performance claim is planned.

## Execution record

Original decision recorded before product changes. Implementation, verification, final publication, cleanup and integration are pending.
