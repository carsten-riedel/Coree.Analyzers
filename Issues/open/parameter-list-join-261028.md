## Before

A method, local function, constructor, or primary constructor can keep a short parameter list on several lines:

```csharp
public void M(
    float a,
    float b,
    float c)
```

`float a, float b, float c` is 25 characters. The default limit is 120, so Coree.Analyzers.CodeClarity reports CCCPL002 and says to write the list on one line, as in `M(a, b, c)`. The developer joins the parameters by hand. The same warning appears on a constructor, a local function, and a primary constructor when that measured length still fits. A list that is already one line, a list that does not fit, and a list that contains a `//` comment, a `///` documentation comment, a preprocessor directive, or a literal line break stay quiet. CCCMB001 can insert the blank line it asks for. CCCPL002 cannot apply the one-line spelling it asks for.

## Change

CCCPL002 will gain a code fix that rewrites the flagged parameter list onto one line. The parameters are joined with a comma and a space. Tabs, line breaks, and repeated spaces outside tokens collapse. Whitespace inside a literal stays. Both parentheses stay on that same line. A block comment that the length measurement already keeps remains in that one-line spelling. If the one-line spelling would drop other non-whitespace text from inside the list, the fix is not offered and the warning stays. Lists that do not report CCCPL002 are left alone. The compiler does not apply the edit. The warning remains until the fix is applied. Fix-all uses that same edit for each CCCPL002 in the document, the project, or the solution.

Which lists report does not change. The repository README, the CodeClarity NuGet readme, the package notes, and `docs/codeclarity/user/parameter-list-length.md` will say the editor can join the list.

## After

Not implemented yet.

## Evidence

At `origin/main` `14ccdee7a2ae1b66d99339f6fcf4d5078986fddd`, `ParameterListFitAnalyzer` reports CCCPL002 when `ParameterListFit.UsesBreakButFits` is true. Its message says "Write it on one line, as in M(a, b, c)." `ParameterListFitAnalyzerTests.ShortBreaksReport` expects that warning for a wrapped method, a padded list, a wrapped type name, an empty default, a constructor, and a local function. `OneLineAndLongBreaksStayQuiet` expects no warning for a list that is already one line and for a list that does not fit. Comment, documentation-comment, directive, and literal-line-break tests expect silence. A search of `src/prj/Coree.Analyzers.CodeClarity` finds `MethodBlankLineFixProvider` and no code fix for CCCPL002.

The parameter-list topic records why the default is 120: short lists were being wrapped even though they still read on one line. That is the situation in Before. Whether a developer applies a fix was not measured.

## Understood assignment and provenance

The user invoked execute-product-evolution by its skill path and named Balanced. The same request said to skip `AGENTS.md` and everything under `AGENTS/RUNONCE`, and not to execute them. That constraint did not name a product area or tailor one phase. Before any policy file was opened, Discover, Assess, and Select & Bound were each set to `Balanced.md`. No later instruction changed that.

## Discover — PolicyDiscover/Balanced.md + PolicyDiscover/Contract.md

No open pull request, remote head, or `Issues/` file already claims a parameter-list code fix. The done issues in this area cover when CCCPL001 and CCCPL002 report, not an editor edit.

- A: CCCPL002 tells the reader to write a fitting wrapped list on one line, and the editor does not. Evidence: the message and tests above; only `MethodBlankLineFixProvider` exists under CodeClarity. The parameter-list topic says the 120 default exists because short lists were wrapped too early.
- B: CCCPL001 tells the reader to use one line or one parameter per line, and the editor does not rewrite a mixed list into the expanded shape. Evidence: `ParameterListLayoutAnalyzer` message; `MixedLayoutsReport`. A short mixed list is also a CCCPL002 case, so expanding it would create the warning A removes.
- C: `operator` and conversion-operator parameter lists are not registered by either parameter-list analyzer. CCCRC001 does register those declarations for return scoring. Evidence: `ParameterListFitAnalyzer.Initialize`; `ParameterListLayoutAnalyzer.Initialize`; `ReturnTypeComplexityAnalyzer.Initialize`. The published scope names methods, local functions, constructors, and primary constructors. It names calls, indexers, delegates, and lambdas as out. It does not name operators.
- D: CCCRC001 on a returned arrow says to move the arrow into a block first. Nothing does that edit. Turning the arrow into `return` would leave CCCRC001 on that return. Evidence: `ReturnTypeComplexityAnalyzer.ArrowAdvice`; `ExpressionBodiedReturnedValuesReport`.
- E: Skipping one extra folder for every Typography character check means setting six `*AnalyzerAdditionalExcludes` properties. The README example sets only `EmDashAnalyzerAdditionalExcludes`. Evidence: `README.md` Typography section; `EmDashAnalyzer.props` and the five sibling props. `exclude-globs-261025` already settled the per-analyzer include/exclude model.

Not counted: indexers, delegates, lambdas, and calls are already named as outside the parameter-list rules. `&&` and `||` were left uncounted on purpose. Other Unicode spaces are outside the published Typography character list.

## Assess — PolicyAssess/Balanced.md + PolicyAssess/Contract.md

- A adds an optional edit and does not change which lists report. The one-line spelling is the one the warning and `ParameterListFit` already define. The quiet cases are the ones a one-line spelling cannot keep. Rollback is removing the fixer. The main risk is dropping text inside the list; withholding the fix in that case keeps the warning and the source.
- B is a second edit with two legal targets. Applied to a short list, it fights A. Indentation of the expanded form is a new choice. It is eligible later, not as part of A.
- C would warn on operator lists that are quiet today. With severity `error`, a build that is clean now could fail. The published declaration set would change. That was not authorized. C is ineligible.
- D does not clear CCCRC001 unless the fixer also invents locals and splits the expression. That can change names and evaluation that the warning does not specify. D is ineligible as a mechanical fix.
- E is a real repetition on the Typography property surface. It is a different product from A, and a mistake in those globs changes which files are read from disk. The per-analyzer model is already the published contract.

A has the clearer consumer edit and the smaller compatibility risk. E is the nearest eligible alternative and is a separate goal.

## Select & Bound — PolicySelectBound/Balanced.md + PolicySelectBound/Contract.md

A is selected. Before this issue, branch, or worktree existed, `git fetch --prune origin` showed only `origin/main` at `14ccdee7a2ae1b66d99339f6fcf4d5078986fddd`. `gh pr list` and `gh issue list` were empty. Nothing else claims this join.

The result is that a developer can apply the one-line parameter list CCCPL002 already asks for. B, C, D, and E stay out. Included: the CCCPL002 code fix, document/project/solution fix-all for that diagnostic, tests of the edit and of the cases that must not change, and the README, NuGet readme, package notes, and CodeClarity user page that describe CCCPL002. Excluded: CCCPL001's expanded shape, operator and conversion-operator lists, any change to the length limit or to when CCCPL002 reports, CCCRC001, Typography excludes, severity, and diagnostic ids.

## Selected improvement and baseline

Consumers are C# developers who see CCCPL002. The warning, its id, its severity, and its maximum stay. A list that does not report is not rewritten. The compiler does not apply the edit.

Acceptance: the wrapped `float a`, `float b`, `float c` list in Before can be fixed to one line, parentheses included, and CCCPL002 is gone from that list. A constructor, local function, and primary constructor that report are joined the same way. Formatting whitespace collapses; whitespace inside a literal stays; a block comment the measurement keeps stays. A list that is already one line, too long, or quiet because of a `//` comment, a `///` comment, a directive, or a literal line break is not offered this fix. A fix that would drop other non-whitespace text inside the list is not offered. The file outside the list is unchanged. `dotnet test` from `src/sln/Coree.Analyzers.CodeClarity` passes, with Coverlet still at 100% line, branch, and method coverage of `Coree.Analyzers.CodeClarity`. Rollback is reverting the fixer, its tests, and the notes.

Integration base: `origin/main` `14ccdee7a2ae1b66d99339f6fcf4d5078986fddd`. Local branch: `feature/pevo/parameter-list-join-261028`. Worktree: `.worktrees/parameter-list-join`. Early publication of this issue-only commit is the next step, before product edits.

## Incidental Discoveries (Out of Scope)

- Operator and conversion-operator parameter lists are not examined by CCCPL001 or CCCPL002, while CCCRC001 does examine those declarations' returned arrows. Noticed in the three `Initialize` methods. The published parameter-list scope does not name operators. It is unknown whether that omission was deliberate.
- Moving a too-complex arrow into a block would leave CCCRC001 on the new `return`. Noticed from `ArrowAdvice`. Clearing it would require choosing local names.
- The Typography "skip extra folders" sample sets one analyzer's additional excludes. The other five character checks are separate properties. Noticed in `README.md`. It is unknown whether a reader treats that sample as applying to every check.
