## Before

A method whose parameters share a line, with the closing parenthesis on that line, reports CCCPL001. The same shape on an operator stays silent:

```csharp
public static bool operator ==(
    Meter left, Meter right)
```

`Meter left, Meter right` is short enough for the default 120-character fit limit, so a method with that break also reports CCCPL002 and the editor can join it. The operator reports neither warning, and neither edit is offered.

A conversion operator is the same gap for the fit rule. It has one parameter, so CCCPL001 would stay quiet on a method as well. The break still fits:

```csharp
public static explicit operator int(
    Meter value)
```

A method with that break reports CCCPL002. The conversion operator does not. A one-line operator, such as `public static Meter operator +(Meter left, Meter right)`, stays quiet, and that part is already what the rules say for any other one-line list.

CCCPL001 and CCCPL002 register methods, constructors, local functions, and primary constructors. They do not register `operator` or conversion-operator declarations. CCCRC001 does register both, and scores a returned arrow on them. Calls, indexers, delegates, and lambdas are named as outside the parameter-list rules. Operators are not named either way.

## Change

CCCPL001 and CCCPL002 use their existing shape and fit rules on `operator` and conversion-operator parameter lists. A binary operator with the mixed list in Before reports CCCPL001 and, because those two parameters still fit, CCCPL002. The editor offers the one-line join it already uses for CCCPL002, and it does not expand a list that CCCPL002 wants on one line. A broken conversion operator whose single parameter still fits reports CCCPL002 only. A one-line operator stays quiet. A list that does not fit, or that contains a `//` comment, a `///` documentation comment, a preprocessor directive, or a literal line break, stays quiet for CCCPL002, the same as a method. A unary operator is the same one-parameter case as the conversion operator.

The repository README, the CodeClarity NuGet readme, the package notes, the CodeClarity user and product pages, and the parameter-list domain topic name operators in this declaration set. Which shapes and which lengths report does not change for methods, local functions, constructors, or primary constructors.

## After

The operator in Before reports CCCPL001 and CCCPL002. Joining it produces `public static bool operator ==(Meter left, Meter right)`, and both warnings are gone from that list. The expand edit is not offered for it, because the parameters still fit.

The conversion operator in Before reports CCCPL002 and does not report CCCPL001. Joining it produces `public static explicit operator int(Meter value)`, and CCCPL002 is gone. A broken unary `operator ~` does the same: CCCPL002 only, because it has one parameter.

`public static Meter operator +(Meter left, Meter right)` reports neither diagnostic. A broken `operator +` that contains `//` does not report CCCPL002. The compiler does not apply either edit. Whether a developer applies the fix was not measured.

## Evidence

Before the edit, at `origin/main` `5891bb9d89b448b2d3f4ec4a193085a218d61c6a`, `ParameterListLayoutAnalyzer.Initialize` and `ParameterListFitAnalyzer.Initialize` registered `MethodDeclaration`, `ConstructorDeclaration`, `LocalFunctionStatement`, and the class, struct, record, and record-struct declarations. They did not register `OperatorDeclaration` or `ConversionOperatorDeclaration`. `ReturnTypeComplexityAnalyzer.Initialize` already registered both operator kinds. `ParameterListLayout.IsDisallowed` returns false when a list has fewer than two parameters. `ParameterListFit.UsesBreakButFits` does not require two parameters. The layout and join fixes find a `ParameterListSyntax` from the diagnostic span, without checking the parent declaration.

`git ls-remote --heads origin` on 2026-10-28 showed only `refs/heads/main`.

After the edit, `OperatorsUseTheSameShapes` expects CCCPL001 only on the mixed `operator ==`. The one-line `operator +`, the expanded `operator -`, the broken conversion, and the broken unary `operator ~` stay quiet for CCCPL001. `OperatorsThatFitReport` expects CCCPL002 on the mixed `==`, the expanded `-`, the conversion, and the unary `~`, and not on the one-line `+`. `OperatorLineCommentStaysQuiet` expects no CCCPL002 when the operator list contains `//`. `OperatorAndConversionOperatorJoin` joins the mixed `==` and the conversion, then both analyzers stay quiet on that result. `ShortOperatorThatFitsRegistersNoExpandAction` expects no expand edit for the short mixed `==`.

From `src/sln/Coree.Analyzers.CodeClarity`, `dotnet test -m:1` passed 192 tests. Coverlet reported 100% line, branch, and method coverage of `Coree.Analyzers.CodeClarity`. `dotnet build --no-restore -m:1` succeeded with 0 errors and the six existing DebugHost sample warnings: CCCNS001, CCCMB001, CCCRC001, two CCCPL002, and one CCCPL001. DebugHost has no operator, so those warnings are unchanged. `dotnet pack --no-restore -m:1` wrote `src/prj/Coree.Analyzers.CodeClarity/bin/Pack/Coree.Analyzers.CodeClarity.1.0.21-gb212f82100.nupkg` with the analyzer dll under `analyzers/dotnet/cs`, props under `build/` and `buildTransitive/`, the readme, and no `lib` group. That package's release notes include the operator bullet. Implementation commit: `b212f82100104331a38b75c2c19199a73a47579f`. Issue-only commit: `e5be136dbe5a3ba54337b4aad6bb2f2dd803e647`, published to `origin/feature/pevo/operator-lists-261028` before the product edit. Consumer uptake was not measured.

## Understood assignment and provenance

The user invoked execute-product-evolution by its skill path and named Ambitious. The same request said to skip `AGENTS.md` and everything under `AGENTS/RUNONCE`, and not to execute them. That constraint did not name a product area or tailor one phase. Before any policy file was opened, Discover, Assess, and Select & Bound were each set to `Ambitious.md`. No later instruction changed that.

## Discover — PolicyDiscover/Ambitious.md + PolicyDiscover/Contract.md

No open pull request, remote head, or `Issues/` file already claims operator parameter lists. Done issues in this area record the method, constructor, local-function, and primary-constructor rules, and one of them notes the operator omission as unknown rather than implemented.

- A: `operator` and conversion-operator parameter lists are not examined by CCCPL001 or CCCPL002. The same declarations' returned arrows are examined by CCCRC001. Evidence: the three `Initialize` methods; README and `docs/codeclarity/product/parameter-list-layout.md`. Whether the omission was deliberate is not written down. Operators are not in the named stay-out list.
- B: The CodeClarity install sample in `README.md` and `src/prj/Coree.Analyzers.CodeClarity/Properties/NugetMetadata/Readme.md` pins `PackageReference` version `0.1.2`. `dotnet package search` on nuget.org reported latest `1.0.20` on 2026-10-28. Both analyzer projects also package-reference CodeClarity `0.1.2`. The sentence beside the sample says that `0.1.2` is the version shown there.
- C: Each Typography props file sets that analyzer's `Includes` to `**` when the property is empty. The README says an empty includes list turns extra-file scanning off. An empty value written before the package props import can therefore be replaced. This run did not evaluate a project that sets the empty value in `Directory.Build.props`.
- D: CCCNS001 is described in the README and in `docs/agent-domain-understanding/context/type-namespace.md`. `docs/codeclarity/user/` and `docs/codeclarity/product/` have pages for the other CodeClarity rules and none for this one. `PackageReleaseNotes.txt` does not name CCCNS001.
- E: The README sample for skipping extra folders sets `EmDashAnalyzerAdditionalExcludes` only. The other five Typography checks have their own properties. It is unknown whether a reader treats that sample as applying to every check.
- F: CCCRC001 on a returned arrow says to move the arrow into a block and then extract locals. Nothing performs that edit. Turning the arrow into `return` would leave CCCRC001 on that return, because the score would be unchanged. Clearing it would require choosing local names the warning does not give.

`ProjectDirectoryTextFiles.ContainsNul` still returns false when the first NUL is after 512 characters, and a test locks that limit. No file in this run was shown to warn because of it, so it is not a candidate.

## Assess — PolicyAssess/Ambitious.md + PolicyAssess/Contract.md

- A adds the existing parameter-list rules to the two declaration kinds CCCRC001 already treats as returned values. The editor fixes already rewrite whatever parameter list the diagnostic points at. The practical case is a short broken operator list, which CCCPL002 would join. A binary operator can also report CCCPL001 when the shape is mixed and CCCPL002 stays quiet, or when the joined parameters do not fit. New warnings appear on operator signatures that are clean today. That is an expansion of the published declaration set, not a change to what a method reports. Rollback is reverting the registration. Confidence is moderate: the code gap is visible, and the omission was never written as a decision.
- B is a confirmed mismatch between the install sample and the package on nuget.org. Copying the sample installs `0.1.2` while the same README describes later rules. A pinned version in that sample goes stale on the next published build, so the durable text change is to stop presenting `0.1.2` as the package a reader adds. It does not give operators the parameter-list rules. The project `PackageReference` pins are a maintainer dogfood choice, separate from the sample a consumer copies.
- C would make an empty includes list mean off even when it is assigned before the props import, which is what the README already says. The early-empty path was not reproduced here. Changing the default without that evaluation could turn scanning off for a project that had been relying on the replacement. It stays eligible only after that evaluation.
- D explains a rule the README already states. It does not change what CCCNS001 reports. Useful for the audience pages, and smaller than a declaration-set gap.
- E is a sample that shows one property. The surrounding text says to override what you need. The misreading is possible and unconfirmed.
- F does not clear CCCRC001 unless the edit also invents locals. Offering a fix that leaves the same warning is a poor editor result. F is not a worthwhile increment.

A is the capability the other candidates do not provide. B is the stronger consumer-facing correction, and it remains a documentation fix. C is not ready to change. D and E are narrower explanations. F fails the practical test that the edit should remove the warning it answers.

## Select & Bound — PolicySelectBound/Ambitious.md + PolicySelectBound/Contract.md

Select A. The result is that an `operator` or conversion-operator parameter list is judged by the same CCCPL001 shapes and the same CCCPL002 length as a method, and the existing editor fixes apply when those diagnostics report.

Included: registration of both operator declaration kinds on both analyzers; tests that a mixed short binary operator reports both diagnostics and can be joined, that a broken one-parameter conversion operator reports only CCCPL002, that a one-line operator stays quiet, and that a `//` comment still keeps CCCPL002 quiet; the README, the NuGet readme, the package notes, and the CodeClarity user and product pages that name the declaration set. The parameter-list domain topic states that set too, so its sentence has to match.

Excluded: calls, indexers, delegates, and lambdas; any change to when a method, local function, constructor, or primary constructor reports; the fit maximum, severity, and diagnostic ids; CCCRC001's arrow wording and any arrow edit; CCCNS001's pages; the `0.1.2` install sample and the dogfood package pins; Typography includes, excludes, and the NUL scan.

A fresh remote-head check before this worktree found only `origin/main` at `5891bb9d89b448b2d3f4ec4a193085a218d61c6a`. No `operator-lists` ref existed.

### Acceptance criteria

1. The binary operator in Before reports CCCPL001 and CCCPL002 at the default maximum. Joining it removes both from that list.
2. The conversion operator in Before reports CCCPL002 and does not report CCCPL001. Joining it removes CCCPL002.
3. `public static Meter operator +(Meter left, Meter right)` reports neither diagnostic.
4. A broken operator list that contains a `//` comment does not report CCCPL002.
5. A method, constructor, local function, and primary constructor still follow the tests that already cover them.
6. From `src/sln/Coree.Analyzers.CodeClarity`, `dotnet test -m:1` passes with 100% line, branch, and method coverage of `Coree.Analyzers.CodeClarity`. `dotnet build -m:1` and `dotnet pack -m:1` succeed.
7. The README, the NuGet readme, the package notes, the user and product parameter-list pages, and the parameter-list domain topic name operators in the declaration set. Calls, indexers, delegates, and lambdas stay out.

### Consumers, risk, and rollback

Consumers are C# developers who write `operator` or conversion-operator signatures. Methods and the other declarations already in scope do not gain or lose a warning. A project that treats warnings as errors can fail on an operator list that used to be silent. The diagnostic ids, the default severities, and the 120-character maximum stay. The compiler does not apply either edit. Rollback is reverting this branch. Whether a developer applies the fix was not something this run can measure ahead of the edit.

## Baseline and execution

- Status: Done. Implementation verified against the acceptance criteria. Not merged.
- Integration target: `origin/main` at `5891bb9d89b448b2d3f4ec4a193085a218d61c6a`, refreshed on 2026-10-28.
- Branch: `feature/pevo/operator-lists-261028`
- Worktree: `.worktrees/operator-lists`
- Issue-only commit `e5be136dbe5a3ba54337b4aad6bb2f2dd803e647` was published before the product edit. Publication of this closure commit, review merge, and worktree cleanup: pending.

## Final outcome

Status: Done. The implementation is verified and ready for review. It is not merged.

The mixed `operator ==` in Before reports CCCPL001 and CCCPL002, and the one-line join removes both. The broken conversion reports only CCCPL002, and the join removes it. A one-line operator stays quiet. A `//` comment keeps CCCPL002 quiet. Calls, indexers, delegates, and lambdas stay out. Rollback is reverting `b212f82100104331a38b75c2c19199a73a47579f`.

Publication of this closure commit, a pull request, and removal of `.worktrees/operator-lists` are pending. The remote branch already contains the issue-only commit.

## Incidental Discoveries (Out of Scope)

- `ProjectDirectoryTextFiles.ContainsNul` returns false when the first NUL is after 512 characters. `ContainsNulReadsOnlyTheFirst512Characters` locks that limit. Whether a later NUL should skip an additional file was not established.
