# Domain clues

2026-10-08 21:04:10Z [S-4e18] [observed] CCCRC001 counts `?:` as a calculation step and does not count `&&` or `||`, while the package text calls the counted operators conditional operators. The scorer was left unchanged after the maintainer asked about a return that uses `||`. Evidence: README.md CodeClarity section; src/prj/Coree.Analyzers.CodeClarity/ReturnExpressionComplexity.cs.

2026-10-25 08:38:18Z [S-9c7a] [observed] CCCRC001's zero-step nameof exemption applies to the language operator, not a callable with the same name; user-defined nameof and @nameof methods count like other calls. Evidence: src/prj/Coree.Analyzers.CodeClarity.Tests/ReturnTypeComplexityAnalyzerTests.cs; src/prj/Coree.Analyzers.CodeClarity/ReturnExpressionComplexity.cs.

2026-10-25 19:21:17Z [S-7d42] [observed] In a C# document, Typography can replace a flagged character with the ASCII character the warning names. A straight quote inside a normal string is escaped, a verbatim string doubles it, a character literal escapes an apostrophe, and a raw string is left alone while the warning still reports. Evidence: src/prj/Coree.Analyzers.Typography/TypographyAsciiReplacement.cs; docs/typography/user/ascii-replacement.md.

2026-10-26 19:12:51Z [S-c3e8] [observed] Typography leaves U+2026 in a character literal unedited and still reports CTYEL001, because three periods are not a character literal. A string ellipsis still becomes three periods. Evidence: src/prj/Coree.Analyzers.Typography/TypographyAsciiReplacement.cs; src/prj/Coree.Analyzers.Typography.Tests/TypographyCharacterFixTests.cs.
