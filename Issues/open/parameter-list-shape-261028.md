## Before

A long parameter list can break the shape CCCPL001 allows and still be too long for the one-line spelling:

```csharp
public void M(
    int parameterName01, int parameterName02,
    int parameterName03,
    int parameterName04,
    int parameterName05,
    int parameterName06)
```

Those six names join to more than 120 characters, so CCCPL002 stays quiet. CCCPL001 still reports, because two parameters share a line and the closing parenthesis sits on the last parameter's line. The same warning reports for a constructor, a local function, and a primary constructor. The developer rewrites the list by hand. A short list that still fits, such as `float a, float b, float c`, is a different warning: CCCPL002 says to write that list on one line. That edit is claimed by `feature/pevo/parameter-list-join-261028` and is not this change.

A comment, a documentation comment, or a directive between the parameters also keeps CCCPL002 quiet. Putting each parameter on its own line has to keep that text, or the edit would delete it.

## Change

CCCPL001 will gain a code fix that rewrites a flagged parameter list into the expanded shape: each parameter starts on its own line, and the closing parenthesis stands on the line after the last parameter. The fix is offered only when CCCPL002 would not ask for one line. Tabs, line breaks, and repeated spaces that are only formatting can change. Text inside a parameter, including a block comment and a literal line break, stays. If a comment, a documentation comment, a directive, or other non-whitespace text sits outside the parameters and the rewrite would drop it, the fix is not offered and the warning stays.

The characters outside the parameter list stay where they are. The compiler does not apply the edit. The warning remains until the fix is applied. Fix-all uses that same edit for each CCCPL001 in the document, the project, or the solution that this fix can rewrite.

Which lists report does not change. The repository README, the CodeClarity NuGet readme, the package notes, and the CodeClarity user and product pages for this shape will say when the editor can expand the list.

## After

Not implemented yet.

## Evidence

At `origin/main` `14ccdee7a2ae1b66d99339f6fcf4d5078986fddd`, `ParameterListLayoutAnalyzer` reports CCCPL001 when `ParameterListLayout.IsDisallowed` is true. Its message names the one-line shape and the one-parameter-per-line shape. `ParameterListLayoutAnalyzerTests.MixedLayoutsReport` expects that warning for packed and broken lists, including a constructor and a primary constructor. `ParameterListFit.UsesBreakButFits` is false when the joined length is over the maximum, and also when the list contains a line comment, a documentation comment, a preprocessor directive, or a literal line break. A search of `src/prj/Coree.Analyzers.CodeClarity` finds `MethodBlankLineFixProvider` and no code fix for CCCPL001.

`feature/pevo/parameter-list-join-261028` is published and its open issue claims the one-line edit for lists CCCPL002 reports. This issue does not take that edit.

## Understood assignment and provenance

The user invoked execute-product-evolution by its skill path and named Balanced. The same request said to skip `AGENTS.md` and everything under `AGENTS/RUNONCE`, and not to execute them. That constraint did not name a product area or tailor one phase. Before any policy file was opened, Discover, Assess, and Select & Bound were each set to `Balanced.md`. No later instruction changed that.

## Discover — PolicyDiscover/Balanced.md + PolicyDiscover/Contract.md

No open pull request or `Issues/` file on `main` claimed an editor edit for these diagnostics. The done issues in this area cover when the diagnostics report.

- A: CCCPL002 tells the reader to write a fitting wrapped list on one line, and the editor does not. Evidence: `ParameterListFitAnalyzer` message; `ShortBreaksReport`; only `MethodBlankLineFixProvider` under CodeClarity. The parameter-list topic says the 120 default exists because short lists were wrapped too early.
- B: CCCPL001 tells the reader to use one line or one parameter per line, and the editor does not rewrite a mixed list into the expanded shape. Evidence: `ParameterListLayoutAnalyzer` message; `MixedLayoutsReport`. A short mixed list is also a CCCPL002 case.
- C: CCCRC001 on a returned arrow says to move the arrow into a block first. Nothing does that edit. Turning the arrow into `return` would leave CCCRC001 on that return. Evidence: `ReturnTypeComplexityAnalyzer.ArrowAdvice`; `ExpressionBodiedReturnedValuesReport`. The user page says the sentence does not say whether a reader follows it.
- D: CCCNS001 is described in the README, the package gallery text, and `docs/agent-domain-understanding/context/type-namespace.md`. `PackageReleaseNotes.txt` does not name it. `docs/codeclarity/user/` and `docs/codeclarity/product/` have pages for the return, blank-line, and parameter-length rules, and none for this one.
- E: The Typography sample under "skip extra folders" sets only `EmDashAnalyzerAdditionalExcludes`. The other five character checks have their own properties. Evidence: `README.md`; `EmDashAnalyzer.props` and the five sibling props.

Not counted: operator parameter lists stay outside the published CCCPL declaration set. The in-repo CodeClarity package reference stays on 0.1.2 because earlier issues left that pin in place. CTYED001 names only the en dash and the em dash. Empty Typography `Includes` was already judged unsafe to split from the default `**` scan.

## Assess — PolicyAssess/Balanced.md + PolicyAssess/Contract.md

- A adds an optional edit the warning already spells. It does not change which lists report. It is the clearer editor gap. It is not available for this run: the overlap recheck found it claimed.
- B adds an optional edit for the lists A does not take. Expanding a short list that CCCPL002 wants on one line would leave that warning, so B has to stay off those lists. Indentation of the expanded form is a new choice. A comment between parameters cannot be dropped. Rollback is removing the fixer.
- C does not clear CCCRC001 unless the fixer also invents locals. The warning does not specify those names. C is a weak edit.
- D repeats a rule the README and gallery text already state. The practical difference is a missing page and a missing note, not a missing warning.
- E is a separate Typography goal. The per-analyzer properties are the published model. A mistake in a shared glob changes which files are read from disk. The sample names `EmDashAnalyzerAdditionalExcludes`, so it is not established that a reader treats it as every check.

B is the strongest remaining edit. A would have won, and it is already claimed. C, D, and E are smaller or less safe.

## Select & Bound — PolicySelectBound/Balanced.md + PolicySelectBound/Contract.md

A was the first choice. Before this issue, branch, or worktree existed, `git fetch --prune origin` showed `origin/feature/pevo/parameter-list-join-261028`, and that worktree's `Issues/open/parameter-list-join-261028.md` claims the one-line CCCPL002 edit. A is discarded. B is selected.

The result is that a developer can apply the expanded parameter list when CCCPL001 reports and CCCPL002 does not ask for one line. Included: that code fix, document/project/solution fix-all for the lists it can rewrite, tests of the edit and of the lists it must leave alone, and the README, NuGet readme, package notes, and CodeClarity user and product pages that describe this shape. Excluded: the one-line CCCPL002 edit, operator and conversion-operator lists, any change to when CCCPL001 or CCCPL002 reports, the length limit, CCCRC001, CCCNS001's notes, Typography excludes, severity, and diagnostic ids.

## Selected improvement and baseline

Consumers are C# developers who see CCCPL001 on a list CCCPL002 leaves quiet. The warning, its id, its severity, and the fit maximum stay. A list that CCCPL002 reports is not rewritten by this fix. The compiler does not apply the edit.

Acceptance:

1. The six-parameter list in Before can be fixed so each parameter starts on its own line and `)` is on the following line, and CCCPL001 is gone from that list. A constructor, a local function, and a primary constructor that report the same way are expanded the same way.
2. A short list that CCCPL002 reports is not offered this fix.
3. A block comment or a literal line break inside a parameter stays. A comment, documentation comment, or directive between parameters keeps the fix from being offered.
4. The file outside the parameter list is unchanged.
5. From `src/sln/Coree.Analyzers.CodeClarity`, `dotnet test` passes, and Coverlet still requires 100% line, branch, and method coverage of `Coree.Analyzers.CodeClarity`.

Rollback is reverting the fixer, its tests, and the notes.

Integration base: `origin/main` `14ccdee7a2ae1b66d99339f6fcf4d5078986fddd`. Local branch: `feature/pevo/parameter-list-shape-261028`. Worktree: `.worktrees/parameter-list-shape`. Early publication of this issue-only commit is the next step, before product edits.

## Incidental Discoveries (Out of Scope)

- `ProjectDirectoryTextFiles.ContainsNul` returns false when the first NUL is after 512 characters. `ContainsNulReadsOnlyTheFirst512Characters` locks that limit. Whether a later NUL should skip an additional file was not established.
- The Typography "skip extra folders" sample sets one analyzer's additional excludes. Noticed in `README.md`. It is unknown whether a reader treats that sample as applying to every check.
