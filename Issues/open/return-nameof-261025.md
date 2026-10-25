## Before

CCCRC001 does not count an unqualified call to a method named `nameof`, including `@nameof`. For example, `return @nameof(a + b);` contains a method call and an addition but stays quiet at the default maximum of one step. Users receive inconsistent complexity diagnostics depending on a method's name or whether its call is qualified.

## Change

Planned: recognize the compiler's actual `nameof` operation rather than treating every identifier with that spelling as the language operator. Count real method calls normally and preserve the zero-step treatment of genuine `nameof` expressions. Add focused regression and contrasting tests and update the affected consumer explanation and Unreleased package notes.

## After

Not implemented yet.

## Evidence

At base `ab21473c4e2051b8b0cc2abcbe38ddb35e7f7c8d`, [ReturnExpressionComplexity.IsNameof](../../src/prj/Coree.Analyzers.CodeClarity/ReturnExpressionComplexity.cs) checks only `Identifier.ValueText == "nameof"`. The [package README](../../src/prj/Coree.Analyzers.CodeClarity/Properties/NugetMetadata/Readme.md) says method calls count and `nameof` does not. Roslyn exposes the actual language operation as [INameOfOperation](https://learn.microsoft.com/dotnet/api/microsoft.codeanalysis.operations.inameofoperation?view=roslyn-dotnet-4.14.0). Existing tests protect genuine `nameof(C)` at the default threshold but do not distinguish it from user-defined methods with that name. Runtime reproduction is pending; consumer benefit is unmeasured.

## Assignment and baseline

- User requested the next `execute-product-evolution` run, without a profile or feature specialization. This authorizes one independently selected, bounded improvement and the skill's issue, commit, publish and cleanup lifecycle. The preceding documentation discussion was accepted as an explanation, not a request to change the documentation layout.
- Status: Open. Date: 2026-10-25.
- Discover, Assess, and Select & Bound were all selected as Default (Conservative) before policy references; no policy changes or additional constraints apply.
- Integration target: `main`; refreshed base `origin/main` at `ab21473c4e2051b8b0cc2abcbe38ddb35e7f7c8d`.
- Feature branch: `feature/pevo/return-nameof-261025`; ignored temporary linked worktree: `.worktrees/return-nameof-261025`.
- Early issue-only checkpoint publication: pending; implementation must wait for verified remote visibility.

## Discover — Default.md + Contract.md

Sources: `.agents/skills/execute-product-evolution/references/PolicyDiscover/Contract.md` and `Default.md` in that folder.

Reviewed current package documentation, existing domain clues/context, relevant CodeClarity implementations/tests, Typography glob handling, and build/test guidance. Four candidates remained after screening local/remote branches, available file issues, GitHub issues and open PRs:

- A: `ReturnExpressionComplexity.IsNameof` excludes user-defined `nameof` calls by spelling alone. This is a concrete inconsistency with ordinary calls; semantic binding provides a narrow verification path.
- B: `ParameterListFit.Normalize` collapses whitespace inside string defaults and can underestimate required width. Literal/trivia and comment behavior would require wider investigation.
- C: `ParameterListLayout.IsDisallowed` compares parameter start lines even for multiline parameter declarations. The intended treatment of multiline attributes and defaults is less clear.
- D: `ReturnExpressionComplexity.IsCalculation` lists left/right shifts but omits the unsigned-right-shift syntax kind. Modern syntax coverage is a plausible gap; intended scoring and a language-version-specific regression still need investigation.

The previous run's method-newline fix is already published at `origin/feature/pevo/method-newlines-261025` with a Done issue and is excluded as a duplicate despite not being merged. Its deferred observations B/C/A above were not implemented or claimed. No other issue/branch/PR covers these outcomes; the pre-selection remote recheck found no change.

## Assess — Default.md + Contract.md

Sources: `.agents/skills/execute-product-evolution/references/PolicyAssess/Contract.md` and `Default.md` in that folder.

- A: Strong direct evidence and a clear existing distinction between compile-time name lookup and runtime calls. Small semantic boundary, no API/configuration changes, and precise positive/negative cases using the existing test harness. Expected value is consistent diagnostics; affected usage frequency is unknown.
- B: Useful false-positive opportunity, but normalizing only safe trivia requires broader syntax/literal decisions than A.
- C: Lower confidence because a repair first needs a precise definition of accepted multiline parameters and interactions with the separate fit rule.
- D: Potentially small, but expands the explicit counted-operator set and needs more scoring-contract confirmation. A has a clearer unchanged rule.

## Select & Bound — Default.md + Contract.md

Sources: `.agents/skills/execute-product-evolution/references/PolicySelectBound/Contract.md` and `Default.md` in that folder.

Select A for the narrowest well-supported correction to an existing promise. Include semantic disambiguation, tests of actual and shadowed/escaped/qualified `nameof` calls, directly affected documentation/release notes, and this issue. Exclude B/C/D, the prior method-spacing fix, refactoring the entire scorer to operations, expression-bodied-return policy, changes to maximum/severity/messages/locations, dependencies, release identities, CI, PR creation and integration.

### Acceptance criteria

1. At the default maximum of one, a user-defined `nameof` call plus an arithmetic step reports CCCRC001 on the full return expression, whether escaped, unescaped when bound as a method, or qualified.
2. A single such method call remains allowed at maximum one and reports at maximum zero; nested calls are counted independently.
3. Genuine `nameof` retains zero cost at maximum zero, including when a same-named callable exists, and ordinary calls retain their existing score.
4. Show the regression against the original implementation and the repair passing the full CodeClarity suite with its configured 100% line/branch/method threshold. Build and pack the changed package and inspect package layout and updated metadata.
5. Preserve public diagnostics/configuration, unrelated rules and the caller's clean main checkout. Publish only this run's feature branch, close the same issue in a dedicated last commit, and clean up only safely published local artifacts.

### Risks, consumers and rollback

Consumers are CodeClarity users running IDE or build diagnostics on C# containing callable identifiers named `nameof`. The intended additional diagnostics follow the existing method-call contract. Semantic-model lookups must work in the existing Roslyn 4.14 dependency and remain safe on incomplete source. Restrict any extra lookup to syntax that could be `nameof`; do not introduce new dependencies or rewrite unrelated scoring.

Verification uses the existing MSTest/Roslyn harness and documented solution commands; performance and real consumer benefit are not measured. A normal revert of the implementation/documentation commits is sufficient rollback. No data, API or configuration migration is required.

## Execution record

Original assignment, policies and scope recorded before product edits. Implementation, tests, final-head publication and cleanup remain pending; no merge is requested.
