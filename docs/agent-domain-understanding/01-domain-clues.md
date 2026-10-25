# Domain clues

2026-10-08 21:04:10Z [S-4e18] [observed] CCCRC001 counts `?:` as a calculation step and does not count `&&` or `||`, while the package text calls the counted operators conditional operators. The scorer was left unchanged after the maintainer asked about a return that uses `||`. Evidence: README.md CodeClarity section; src/prj/Coree.Analyzers.CodeClarity/ReturnExpressionComplexity.cs.

2026-10-25 08:38:18Z [S-9c7a] [observed] CCCRC001's zero-step nameof exemption applies to the language operator, not a callable with the same name; user-defined nameof and @nameof methods count like other calls. Evidence: src/prj/Coree.Analyzers.CodeClarity.Tests/ReturnTypeComplexityAnalyzerTests.cs; src/prj/Coree.Analyzers.CodeClarity/ReturnExpressionComplexity.cs.
