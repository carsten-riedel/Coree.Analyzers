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

Count a user-defined `operator +` as one calculation step even when an operand is a string. Leave built-in string concatenation uncounted, including `return a + " " + b` at maximum 0. Cover a string on either side, a custom addition combined with another counted step, and the existing maximum and severity settings. Say this in the return-complexity explanation and the package notes.

## After

Not implemented yet.

## Evidence

At `origin/main` `ab21473c4e2051b8b0cc2abcbe38ddb35e7f7c8d`, `ReturnExpressionComplexity.IsStringConcatenation` returns true when either operand's type is `string`, and `IsCalculation` then skips the addition. The published rule counts arithmetic other than string concatenation. Existing return tests cover built-in `a + " " + b` and int addition, not a user-defined operator. Consumer benefit is not measured.

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

- Status: Open.
- Integration target: `origin/main` at `ab21473c4e2051b8b0cc2abcbe38ddb35e7f7c8d`.
- Feature branch: `feature/pevo/custom-addition-261025`.
- Temporary worktree: `.worktrees/custom-addition-261025`, ignored by the caller checkout.
- The caller's checkout is not the implementation workspace.
- This file is the only content of the first commit. Publish that commit and verify the remote ref before product edits.
- Run from `src/sln/Coree.Analyzers.CodeClarity`: restore, serialized build, full test, and pack. A focused red/green run may turn coverage collection off because a filtered run cannot meet the whole-assembly threshold.

## Incidental Discoveries (Out of Scope)

- `int Body(int a, int b, int c) => a + b + c` is in the original quiet return test, so expression-bodied members are outside CCCRC001 today. Whether that was only a demonstration of the boundary was not settled.
- `&&` and `||` are not calculation steps. The domain note says the scorer was left unchanged after a question about `||`.
- A `//` comment between parameters can make CCCPL002 recommend a one-line list. That measurement code is already part of another open increment.
- Empty Typography includes are rewritten to `**` when they are empty at props-import time. Setting the empty value later in the project is a different evaluation order.
