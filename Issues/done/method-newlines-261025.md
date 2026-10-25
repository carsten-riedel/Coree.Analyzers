## Before

CodeClarity's CCCMB001 reports a missing blank line between two methods even when an empty line separates them in a C# file using carriage-return-only or Unicode line endings. `MethodBlankLine.HasBlankLine` splits only on LF, so these valid source lines are never examined. Consumers cannot satisfy the diagnostic without changing their file's line endings.

## Change

Changed `MethodBlankLine.HasBlankLine` to use Roslyn's source-text line boundaries while examining only complete interior lines. Added contrasting analyzer tests for separated methods, adjacent methods, whitespace-only lines, comments, and mixed line endings. The existing rule and configuration remain intact; consumer README descriptions and Unreleased package notes explain the corrected behavior.

## After

The same methods separated by an empty or whitespace-only line now stay quiet in CR-only and Unicode-terminated C# source. LF, CRLF, and mixed endings also recognize blank lines. A single CRLF pair, method indentation, and comment-only gaps still report on the later method. Consumers can satisfy CCCMB001 without changing their source-file line endings.

## Evidence

At base `ab21473c4e2051b8b0cc2abcbe38ddb35e7f7c8d`, [MethodBlankLine.cs](../../src/prj/Coree.Analyzers.CodeClarity/MethodBlankLine.cs) uses `gap.Split('\n')`; its existing carriage-return test actually supplies CRLF. [C# line terminators](https://learn.microsoft.com/dotnet/csharp/language-reference/language-specification/lexical-structure#632-line-terminators) include CR, LF, CRLF, NEL, line separator, and paragraph separator. The existing [domain contract](../../docs/agent-domain-understanding/context/method-blank-line.md) defines an empty or whitespace-only line without limiting line endings. With the final valid regression fixtures, the original implementation failed nine of 24 method-spacing tests; the repaired implementation passes the complete 61-test CodeClarity suite with 100% line, branch, and method coverage. Build and pack also succeed. Consumer benefit has not been measured.

## Assignment and status

- User invoked `execute-product-evolution` without a profile or a specific feature request, authorizing one autonomous bounded improvement, issue recording, commits, branch publication, and temporary workspace cleanup. No PR or integration is requested.
- Status: Done (implementation verified; not merged). Date: 2026-10-25.
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
- Early issue-only branch publication: verified at `99c80b26717ff79cbad9d1e7eaaa9041f94fe19a` before product edits.
- Consumers: CodeClarity users in IDE and command-line builds, especially imported or generated source with uncommon line terminators. Generated-code exclusion remains unchanged.
- Main risk: accidentally counting indentation or a CRLF pair as a blank line. Contrast tests must distinguish complete interior lines from partial first/last lines.
- Rollback: revert the implementation and documentation commits; no data migration or public API change.
- Verification uses existing MSTest/Roslyn infrastructure and solution commands. No consumer validation or performance claim is planned.

## Execution record

Original decision recorded and published before product changes in `99c80b2`. Implementation is committed in `2fb3c16`; the separate documentation checkpoint is `fe0e206`. No change to the original outcome, policies, or outer scope was required.

## Verification and final outcome

All commands ran from `src/sln/Coree.Analyzers.CodeClarity/` in the isolated worktree:

- Baseline: `dotnet test --no-restore --filter FullyQualifiedName~MethodBlankLineAnalyzerTests -p:CollectCoverage=false --verbosity quiet -m:1` with the final valid new fixtures and original implementation: 9 failed, 15 passed. Failures were the four non-LF terminators for blank/whitespace-only lines and the mixed-ending scenario; the six adjacent/comment-only controls passed.
- Repaired full suite: `dotnet test --no-restore --verbosity quiet -m:1`: 61 passed, zero failed/skipped; 100% line, branch, and method coverage with the configured threshold enabled. The preceding initial test invocation performed restore successfully.
- `dotnet build --no-restore -m:1 --verbosity quiet`: succeeded, zero errors; six expected sample-analyzer warnings from DebugHost. The DebugHost source and runbook explain these deliberately nonconforming samples.
- `dotnet pack --no-restore -m:1 --verbosity quiet`: succeeded. ZIP inspection confirmed the analyzer DLL/XML under `analyzers/dotnet/cs`, props under `build/` and `buildTransitive/`, no `lib/` group, updated packaged README, and corrected release notes in the nuspec.
- `git diff origin/main --check`: clean. Review confirms changes are confined to the blank-line helper, its tests, corresponding consumer documentation/release notes, and this issue. Public IDs, descriptors, severity properties, generated-code policy, other analyzers, and dependencies are unchanged.
- Context challenge: every supported terminator has a positive blank-line case, whitespace-only case, and contrasting missing/comment-only case. Mixed endings are covered; CRLF remains one break. Existing tests protect same-line methods, comments/docs/attributes, member/body exclusions, nesting, severity, message, and diagnostic locations. No unresolved in-scope blind spot was found.
- Runbook and existing domain knowledge were reviewed. No build/operation guidance changed and no unrelated knowledge was promoted. Local work-history files are omitted under the temporary-checkout exception.

Acceptance criteria 1-5 are satisfied. This verifies diagnostic behavior and package compatibility; real consumer usage and benefit remain unmeasured. Typography was not changed and its independent test suite was not run.

Final-head publication and temporary worktree/local branch cleanup are pending at this dedicated closure commit and will be verified in the handoff. The early remote feature branch exists. PR creation and integration remain separate human decisions. Rollback remains a normal revert of the implementation/documentation commits, without migration.
