## Before

CCCRC001 ignores a user-defined `operator +` whenever either operand is a string. At maximum 0, `return a + b` for two ints warns, but this stays quiet:

```csharp
struct Meter
{
    public int Raw;
    public static int operator +(string label, Meter value) => value.Raw;
}

int Named(string label, Meter value)
{
    return label + value;
}
```

That addition calls `Meter`'s operator and returns an int. It is not string concatenation. The same miss hides a second step: `return label + value + extra` stays within the default maximum of 1, while `return a + b + c` warns. A string on the right, as in `return value + label`, is skipped the same way.

## Change

`ReturnExpressionComplexity` treats a user-defined `operator +` as one calculation step even when an operand is a string. Built-in string concatenation is unchanged. The return-complexity description, repository README, NuGet readme, package description, and unreleased package notes say so. Regressions cover a string on either side, a custom addition beside another counted step, and concatenation at maximum 0.

## After

At maximum 0, `return label + value` and `return value + label` report CCCRC001. At the default maximum of 1 they stay quiet, the same as `return a + b`. `return label + value + extra` reports at maximum 1 and stays quiet at maximum 2. `return a + " " + b + c` stays quiet at maximum 0. A custom addition is no longer invisible just because one operand is a string.

## Evidence

At `origin/main` `ab21473c4e2051b8b0cc2abcbe38ddb35e7f7c8d`, `ReturnExpressionComplexity.IsStringConcatenation` returned true when either operand's type was `string`, and `IsCalculation` then skipped the addition. Before the scorer change, `UserDefinedAdditionWithStringOperandCounts` expected 1 diagnostic and got 0, and `UserDefinedAdditionReportsAtZeroMaximum` expected 2 and got 0. After the change those tests passed, along with `UserDefinedAdditionAllowsTwoStepsAtHigherMaximum` and `BuiltInStringConcatenationStaysQuietAtZeroMaximum`. From `src/sln/Coree.Analyzers.CodeClarity`, `dotnet build --no-restore -m:1` succeeded with 0 errors. The DebugHost warnings are the existing sample diagnostics. `dotnet test --no-build` passed 47 tests with 100% line, branch, and method coverage. `dotnet pack -m:1` on implementation commit `98549d98cdae22c0debd3d918067450e6b20fd4a` produced `Coree.Analyzers.CodeClarity.0.1.10-g98549d98cd.nupkg`: the analyzer is under `analyzers/dotnet/cs`, the props are under `build/` and `buildTransitive/`, there is no `lib/` group, and the nuspec notes and packed readme mention the user-defined operator. Consumer benefit is not measured.

## Understood assignment

The user invoked `execute-product-evolution` without a preset or a narrower product goal: one autonomous, bounded, verified improvement, recorded in an issue before implementation, published on its feature branch, and not merged. Before any policy file was opened, Discover, Assess, and Select & Bound were each resolved to Default (Conservative). A later instruction said to follow this skill directly and dropped extra scheduling and branch-naming constraints. That happened before this issue existed and did not change the three phase policies.

## Discover — PolicyDiscover/Default.md + PolicyDiscover/Contract.md

Reviewed both package introductions, the CodeClarity and Typography implementations and tests, the domain notes, and the solution build instructions. Open GitHub pull requests were empty. These outcomes were already implemented or claimed and are not candidates: method line endings, `nameof` disambiguation, parameter-literal length, unsigned right shift, Typography comma globs, and the parameter-list closing-parenthesis span (`origin/feature/pevo/parameter-list-end-261025`).

- B: a user-defined `operator +` with a string operand is treated as concatenation (`ReturnExpressionComplexity.cs`). The miss is visible at maximum 0 and when the custom addition sits beside another counted step.
- C: CCCPL002 can advise a one-line parameter list while a `//` comment between parameters cannot survive that layout (`ParameterListFit.Normalize`). The safe trivia boundary is not settled.
- D: an empty Typography Includes value set before the package props are imported is replaced with `**`, despite the promise that empty includes skip extra-file scanning. A value set in the project body after import can still be empty.

The final overlap recheck removed A. Single typographic quotation marks were the leading alternative, and `origin/feature/pevo/single-quotes-261025` already records that increment. Three candidates remained.

## Assess — PolicyAssess/Default.md + PolicyAssess/Contract.md

- B matches a published scoring rule with a direct code path and existing threshold tests. The repair adds warnings only for operators that were skipped. Built-in concatenation stays quiet. The syntax is uncommon; there is no usage count. The change fits the current Roslyn dependency and the existing analyzer tests.
- C is easier to encounter, but comment, directive, and block-comment cases need a layout rule that the current text does not state. `ParameterListFit` is already being changed so literal whitespace keeps its length. Overlap with that measurement change is enough to leave C out.
- D contradicts the empty-includes sentence only for values that are empty at import time. The ordinary project-file assignment is a different order, and separating omitted from empty has compatibility risk. It ranks below B.

## Select & Bound — PolicySelectBound/Default.md + PolicySelectBound/Contract.md

B is selected. A was the stronger practical candidate until the published single-quote branch claimed it. B is the clearest remaining mismatch, with a smaller behavior change than C or D.

Include the return scorer, focused regressions, and the return-complexity sentences in the repository README, the CodeClarity NuGet readme, the package description, the diagnostic description, and the package notes. Exclude parameter-list behavior, quotation marks, unsigned shifts, `nameof`, `&&` and `||`, expression-bodied members, severity defaults, diagnostic IDs, and package version changes.

### Acceptance criteria

1. `return label + value` and `return value + label`, using a user-defined operator, report CCCRC001 at maximum 0 and stay quiet at the default maximum of 1.
2. `return label + value + extra` reports at maximum 1 and stays quiet at maximum 2.
3. Built-in string concatenation, including several `+` operations that produce a string, stays quiet at maximum 0. Existing int addition, shifts, calls, conditionals, severity, and maximum parsing stay as they are.
4. The full CodeClarity test suite passes with its 100% line, branch, and method coverage threshold. Restore, build, and pack succeed.
5. This issue stays the baseline history. Implementation is a separate commit. The final commit records the verified result and moves this file to `Issues/done/`. The feature branch is published and the remote tip is checked. Only this run's worktree and local branch are removed.

### Consumers, risk, and rollback

Consumers are C# developers using CodeClarity. Projects with a user-defined `operator +` and a string operand can receive a new CCCRC001 warning or error under their current severity and maximum. Built-in concatenation does not. Diagnostic IDs, public configuration, and other analyzers stay unchanged. Rollback is reverting the scorer and the accompanying explanation together. Real consumer impact is not measured.

## Baseline and execution

- Status: Done. Implementation verified on 2026-10-25. Not merged.
- Integration target: `origin/main` at `ab21473c4e2051b8b0cc2abcbe38ddb35e7f7c8d`.
- Feature branch: `feature/pevo/custom-addition-261025`.
- Temporary worktree: `.worktrees/custom-addition-261025`, ignored by the caller checkout.
- The caller's checkout was not used for implementation.
- Issue-only baseline: `9151c08fe16ac2ce1ffdfbcda7da5c1bd1d88558`, published and matched on `origin/feature/pevo/custom-addition-261025` before product edits.
- Implementation: `98549d98cdae22c0debd3d918067450e6b20fd4a`.
- Publication of this Done commit, pull request, merge, and worktree cleanup are pending.

## Final outcome

The scorer checks for a user-defined operator before the string-operand shortcut. No acceptance criterion was dropped. Focused runs that established the regression used `-p:CollectCoverage=false` because a filtered run cannot meet the whole-assembly threshold. The full suite then met 100% line, branch, and method coverage. Expected consumer benefit is not measured. Rollback remains reverting the scorer and the accompanying explanation together. This closure commit does not yet know its own SHA. Remote publication of this commit and local cleanup are not done yet.

## Incidental Discoveries (Out of Scope)

- `int Body(int a, int b, int c) => a + b + c` is in the original quiet return test, so expression-bodied members are outside CCCRC001 today. Whether that was only a demonstration of the boundary was not settled.
- `&&` and `||` are not calculation steps. The domain note says the scorer was left unchanged after a question about `||`.
- A `//` comment between parameters can make CCCPL002 recommend a one-line list. That measurement code is already part of another open increment.
- Empty Typography includes are rewritten to `**` when they are empty at props-import time. Setting the empty value later in the project is a different evaluation order.
