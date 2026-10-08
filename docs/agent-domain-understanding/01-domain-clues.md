# Domain clues

2026-10-08 21:04:10Z [S-4e18] [observed] CCCRC001 counts `?:` as a calculation step and does not count `&&` or `||`, while the package text calls the counted operators conditional operators. The scorer was left unchanged after the maintainer asked about a return that uses `||`. Evidence: README.md CodeClarity section; src/prj/Coree.Analyzers.CodeClarity/ReturnExpressionComplexity.cs.
