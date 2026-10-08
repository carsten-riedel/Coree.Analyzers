# Parameter list layout

CCCPL001 is the CodeClarity rule for a parameter list that breaks across lines in a mixed way.

The maintainer called that mixed break strange. The example was a record primary constructor: three parameters stay on the signature line, then the line breaks, then three more parameters follow. They called two shapes logical. Either the whole list stays on one line, or each parameter stands on its own line.

They then added two shapes that this analyzer must accept. Another analyzer might flag those later. They did not ask for that second analyzer.

- The first parameter stays on the signature line, and each later parameter starts on its own line.
- The declaration name stays on its line, and every parameter starts on the single following line.

The rule that matches those statements: when parameters start on more than one line, no line may start more than one of them. A list whose parameters all start on one line is allowed, including when that line sits under the declaration name. Indentation, and whether the closing parenthesis has its own line, are outside this rule.

The maintainer said to start simply, with methods, and used the record as the example. The analyzer that was added also reports the same layout on local functions, constructors, and primary constructors of classes, structs, and records. Calls, indexers, delegates, and lambdas do not report. That wider set of declarations was reported with the implementation. Each of those kinds was not confirmed on its own.

Evidence: maintainer discussion on 2026-10-08; src/prj/Coree.Analyzers.CodeClarity/ParameterListLayoutAnalyzer.cs; README.md CodeClarity section.
