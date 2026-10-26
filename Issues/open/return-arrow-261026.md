## Before

CodeClarity reports CCCRC001 on a returned arrow that is over the calculation-step limit, and the warning says to extract the extra steps into local variables:

```csharp
int Property => Raw + Raw + 1;
```

That arrow has nowhere to put a local. The same sentence is used for a returned method arrow, a local-function arrow, an indexer arrow, a `get =>` arrow, an operator, and a conversion. A block can hold the local the sentence names:

```csharp
int TwoSteps(int a, int b, int c)
{
    return a + b + c;
}
```

`yield return a + b + c` is also inside a block, and that sentence fits there. The arrow forms above still report. What they report is not the problem. The sentence tells the reader to do something the arrow cannot hold.

## Change

Keep the current sentence for a `return` and a `yield return`. For a returned arrow, say to move that arrow into a block and extract the extra steps into local variables there.

The planned sentences are:

- Block: `Return expression is too complex. Extract intermediate calculations into local variables.`
- Arrow: `Return expression is too complex. Move this arrow into a block and extract the extra steps into local variables there.`

CCCRC001, the title, the severity settings, and which expressions report stay as they are. `docs/codeclarity/user/return-expression-complexity.md` and the CodeClarity package notes will describe the two sentences. A single-step arrow, a void arrow, a constructor, a setter, a lambda, and an async method that returns a non-generic `Task` or `ValueTask` stay quiet.

## After

Not implemented yet.

## Evidence

At `origin/main` `d6771796304860aed617c6351ed40adcff06729a`, `ReturnTypeComplexityAnalyzer` has one message, `Return expression is too complex. Extract intermediate calculations into local variables.` `ReportIfExceeds` uses that message for both `AnalyzeReturn` and `ReportArrow`. `ExpressionBodiedReturnedValuesReport` expects CCCRC001 on the property `Raw + Raw + 1` and on the other returned arrows, and it does not check the sentence. `docs/codeclarity/user/return-expression-complexity.md` says the warning asks for local variables, including for a returned arrow. Whether a developer is actually stopped by that sentence was not measured.

## Understood assignment and provenance

The user invoked execute-product-evolution by its skill path. No preset, product area, or phase instruction was given. Before any policy file was opened, Discover, Assess, and Select & Bound were each set to Conservative `Default.md`. Nothing later changed that choice.

## Discover — PolicyDiscover/Default.md + PolicyDiscover/Contract.md

- A: `char c = '…';` is offered a CTYEL001 fix that writes three periods, so the literal becomes `'...'`, which is not a character literal. `Escape` only rewrites an apostrophe inside that token. Evidence: `TypographyAsciiReplacement.Escape`; `ApostropheInCharacterLiteralIsEscaped`; no character-literal ellipsis test.
- B: CCCRC001 tells every report to extract local variables, including `int Property => Raw + Raw + 1`. That arrow cannot declare a local. Evidence: the message on `ReturnTypeComplexityAnalyzer`; `ExpressionBodiedReturnedValuesReport`.
- C: the README, the diagnostic description, and the older package note say "conditional operators", while the scorer counts `?:` only. `return a > b && a >= b` stays quiet. The user page already names `?:`. Earlier issues left the scorer unchanged after a maintainer question about `||`. Evidence: `ReturnExpressionComplexity.IsCalculation`; domain clue S-4e18.
- D: an empty Typography `*Includes` value is replaced with `**` when it is empty at the package props import. An empty value later in the project body still turns extra-file scanning off. Empty and omitted look the same to that condition. Evidence: `EmDashAnalyzer.props` and the five sibling props.
- E: CCCMB001 says to put a blank line before the method, and CodeClarity has no code fix that inserts it. Evidence: `MethodBlankLineAnalyzer` message; no `CodeFixProvider` under CodeClarity.

Not counted: operator parameter lists stay outside the published CCCPL declaration set. The in-repo CodeClarity package reference stays on 0.1.2 because earlier issues left that pin in place.

## Assess — PolicyAssess/Default.md + PolicyAssess/Contract.md

- A is a fix that currently writes source the compiler rejects. Withholding it adds no warning. A test can show the warning remains and the fix does not. Small, and easy to roll back.
- B does not change what is reported. It stops the arrow warning from naming a local that the arrow cannot hold. The block sentence stays for `return` and `yield return`. The risk is a message change, not a new warning.
- C is a wording mismatch. The user page is already specific, and changing the scorer would add warnings that were left out on purpose.
- D cannot treat empty and omitted as different with the current property, so a small props edit can break the default `**` scan or the project-body off switch. A note about order would be safer and smaller in effect.
- E adds an edit the warning already asks for. It does not change which pairs report. It is larger than a message change because it needs a fixer, fix-all, and a private Workspaces reference.

A and E are the stronger product changes. B is the strongest of the remaining wording changes because the current sentence does not fit the syntax it is attached to.

## Select & Bound — PolicySelectBound/Default.md + PolicySelectBound/Contract.md

Before this issue, branch, or worktree existed, `feature/pevo/char-ellipsis-261026` was already published and its worktree was implementing "leave an ellipsis in a character literal unedited." `feature/pevo/blank-line-fix-261026` had already recorded the CCCMB001 blank-line fixer. Those are the same outcomes as A and E, so they are not selected.

B is selected over C and D. C restates a rule the user page already states. D's behavior change can break one of the two include promises. B gives the arrow warning a sentence the arrow can follow, and it leaves the block sentence alone.

Included: the two CCCRC001 sentences, tests that lock each sentence, the user page, and the package notes. Excluded: scoring, diagnostic id, title, severity, quiet cases, Typography, parameter lists, the blank-line fixer, and the character-literal ellipsis fix.

## Selected improvement and baseline

Consumers are C# developers who read CCCRC001. The id stays `CCCRC001`. A block `return` or `yield return` still says to extract local variables. A returned arrow says to move into a block and extract the extra steps there. Nothing new starts reporting, and nothing that is quiet today starts reporting.

Acceptance:

1. `return a + b + c` and `yield return a + b + c` report CCCRC001 with `Return expression is too complex. Extract intermediate calculations into local variables.`
2. `int Property => Raw + Raw + 1`, and the other returned arrows in `ExpressionBodiedReturnedValuesReport`, report CCCRC001 with `Return expression is too complex. Move this arrow into a block and extract the extra steps into local variables there.`
3. A single-step arrow, a void arrow, a constructor, a setter, a lambda, and a non-generic `Task` or `ValueTask` arrow stay quiet.
4. From `src/sln/Coree.Analyzers.CodeClarity`, `dotnet test` passes, and Coverlet still requires 100% line, branch, and method coverage of `Coree.Analyzers.CodeClarity`.

Rollback is reverting the message split, the tests that lock it, and the notes.

Integration base: `origin/main` `d6771796304860aed617c6351ed40adcff06729a`. Local branch: `feature/pevo/return-arrow-261026`. Worktree: `.worktrees/return-arrow`. Early publication of this issue commit is pending.
