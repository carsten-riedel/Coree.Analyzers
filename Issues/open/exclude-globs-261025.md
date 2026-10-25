## Before

Typography's default `Excludes` list is supposed to keep binaries and other non-source extensions out of the extra files it scans. It does not. With the shipped props imported and `EmDashAnalyzerExcludes` left unset, `AddEmDashAnalyzerAdditionalFiles` still registers `icon.png`, a nested `pic.png`, and `root.dll`. `note.txt` is registered as well. `bin\skip.txt` stays out, because the always-on `bin`, `obj`, `.git`, and `.vs` exclusions are applied separately.

Setting `EmDashAnalyzerExcludes` to an empty value after the props import does the same for `icon.png`, and the property itself is empty. The package text says an empty or omitted `Excludes` value keeps the default list. A replacement value such as `**\*.md` still behaves as a replacement: markdown stays out and `icon.png` comes back.

## Change

Apply each trimmed exclude pattern as its own glob when the additional files are chosen, for every Typography analyzer. Do that for both `Excludes` and `AdditionalExcludes`. When `Excludes` is empty after trimming, fill in the documented default list first. Leave a non-empty list as a replacement, and leave an empty `AdditionalExcludes` as nothing added. Keep the always-on directory exclusions. Cover an omitted list, an empty list written after the props import, a one-pattern replacement, and a two-pattern additional exclude. Record the correction in the package notes.

## After

Not implemented yet.

## Evidence

At `origin/main` `ab21473c4e2051b8b0cc2abcbe38ddb35e7f7c8d`, each analyzer target trims its exclude properties with `[System.String]::Copy(...).Trim()`. After that assignment the separators are stored as `%3B`. `Include="$(EmDashAnalyzerExcludes)"` therefore keeps the whole default list as one item, so `**\*.png` and `**\*.dll` never match. `$(_TypographyAlwaysFolderExcludes)` is not trimmed that way, and `bin\**` still expands to `bin\skip.txt`.

A temporary SDK project that imported `Coree.Analyzers.Typography.props` showed this directly. Omitted excludes: the property contained `**\*.png`, and the EmDash additional files still included `icon.png`, `sub\pic.png`, and `root.dll`. Empty excludes after the import: the property was empty and `icon.png` was included. Replacement `**\*.md`: `readme.md` was absent and the png and dll files were present. `note.txt` stayed present in each case. The registration tests assert that the property text contains `*.dat` and that `bin`, `obj`, `.git`, and `.vs` are absent. They do not assert that `icon.png` or `payload.dat` are absent, and the fixture creates both. Consumer benefit is not measured.

## Understood assignment

The user invoked `execute-product-evolution` with no preset and no phase-specific tailoring. A correction then said to follow this skill directly, to drop extra wait, topic, and branch-suffix instructions, and to continue the same run. Before any policy file was opened, Discover, Assess, and Select & Bound were each resolved to Conservative `Default.md`. That choice did not change. No Balanced or Ambitious profile was read.

## Discover — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicyDiscover/Contract.md` and `Default.md`.

Read both package introductions, the Typography and CodeClarity solution notes, the analyzer implementations, the shipped props, and the registration tests. Refreshed `origin` and the open evolution issues. GitHub issues and pull requests were empty. These outcomes were already implemented or claimed and were not candidates: method line endings, `nameof` disambiguation, CCCPL002 literal whitespace, unsigned right shift, Typography comma globs, the CCCPL001 closing-line measurement, single quotation marks, a user-defined `operator +`, overlapping parameter spans, and a line comment inside a parameter.

- A: trimmed Typography exclude lists stay one glob, so the documented extension defaults do not remove `png` or `dll` files. An empty `Excludes` value written after the import also registers them. Shown by evaluating the shipped props.
- B: an empty `Includes` value that is already empty when the package props import is replaced with `**`. A value written in the project body, after that import, can still stay empty. The failing import-time path was not reproduced.
- C: `&&` and `||` are not calculation steps, while the package text says conditional operators. The scorer and its test were left that way after a maintainer question. The domain clue records that choice.

## Assess — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicyAssess/Contract.md` and `Default.md`.

- A is a reproduced mismatch with the published exclude rule. The repair is in the six additional-file targets. A non-empty list still replaces the default, which the `**\*.md` evaluation already showed. Projects that omit `Excludes` start excluding the documented extensions; projects that set it empty do too, which is what the text already promises. Rollback is a normal revert.
- B is only a disagreement for an assignment that happens before the props import. Changing the default scan without that reproduction could include files a project had deliberately turned off.
- C would either change a locked score or only restate it. The existing test already keeps `a > b && a >= b` quiet.

A is the conservative choice: the failure is visible in the file registration, the intended list is already written down, and the existing registration test can guard it.

## Select & Bound — Default.md + Contract.md

Policy files: `.agents/skills/execute-product-evolution/references/PolicySelectBound/Contract.md` and `Default.md`.

Select A. The result is that each pattern in a trimmed `Excludes` or `AdditionalExcludes` list is its own glob, and an empty `Excludes` value means the documented default list.

Include the six analyzer props targets, the registration coverage, and the package notes. Exclude comma-separated glob names, `Includes` defaulting, the analyzer character sets, CodeClarity, severity, diagnostic IDs, and package version changes.

A fresh branch and issue check before this worktree found no `exclude-globs` ref. `parameter-line-comment-261025` was present and claims the parameter-comment outcome, not this one.

### Acceptance criteria

1. With `Excludes` omitted, `icon.png` and `payload.dat` are not additional files, and a normal text file still is.
2. With every analyzer's `Excludes` set empty after the props import, those same binaries stay out and the text file is still registered by each analyzer. The compiler-visible exclude property contains the default `*.png` pattern.
3. A replacement `EmDashAnalyzerExcludes` of `**\*.md` still excludes markdown and does not restore the png exclusion for that analyzer.
4. `EmDashAnalyzerAdditionalExcludes` set to two patterns, `**\*.md;**\*.json`, excludes both for EmDash only.
5. `bin`, `obj`, `.git`, and `.vs` stay excluded. A non-empty custom list is not merged with the default.
6. Show the omitted png and dat registration failing against the original props, then pass the full Typography suite with its configured 100% line, branch, and method coverage. Build and pack the package.
7. The package notes describe the correction. Public diagnostic IDs, severities, character sets, and `Includes` behavior stay unchanged.

### Consumers, risk, and rollback

Consumers are projects that use the Typography package and its extra-file scan. Projects that omitted `Excludes` or set it empty stop handing the documented binary extensions to the analyzers. Projects that set their own list keep that list. The main risk is a pattern whose semicolon was intentionally escaped as `%3B`; the shipped lists use ordinary separators, and the registration tests cover the default, an empty value, and a replacement. Rollback is reverting the props and the registration coverage together. No configuration migration is required.

## Baseline and execution

- Status: Open.
- Integration target: `origin/main` at `ab21473c4e2051b8b0cc2abcbe38ddb35e7f7c8d`, refreshed on 2026-10-25.
- Feature branch: `feature/pevo/exclude-globs-261025`.
- Temporary ignored worktree: `.worktrees/exclude-globs-261025`.
- This issue is the only content of the first commit. Publish that commit and verify the remote ref before any product edit.
- Run from `src/sln/Coree.Analyzers.Typography`: documented restore, serialized build, full test, and pack. A filtered red run may disable coverage because a filtered run cannot meet the whole-assembly threshold.
- Verification, final publication, and cleanup are pending.

## Incidental Discoveries (Out of Scope)

- Expression-bodied `=> a + b + c` is already expected to stay quiet in `ReturnTypeComplexityAnalyzerTests`.
- CodeClarity package notes on this baseline stop at 0.1.5 and do not mention CCCPL002, CCCNS001, or CCCMB001.
- `ProjectDirectoryTextFiles.ContainsNul` inspects only the first 512 characters, and a test locks that limit.
- `SmartQuotesAnalyzer` still describes itself as a sample analyzer and says to replace the type.
