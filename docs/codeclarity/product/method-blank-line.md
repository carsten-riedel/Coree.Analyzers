# Method blank line

CodeClarity diagnostic CCCMB001 is a separation rule for methods. It reports the later of two methods with a body that are consecutive members of the same type when no blank line sits between them. The default severity is warning. The property is `MethodBlankLineAnalyzerSeverity`.

A line that is empty or whitespace only counts as that blank line. A comment, a documentation comment, or an attribute between the methods does not. A blank line above that comment, documentation, or attribute does. Constructors, fields, properties, operators, indexers, nested types, local functions, and methods without a body stay quiet. The inside of a method stays quiet.

In a C# document, the editor can insert the missing blank line after the previous method. The inserted line uses the line ending already in the file. Text between the methods, including a comment on the previous method's line, stays below that line. Two methods written on one line are split by the empty line. The compiler does not insert it. The warning remains until the fix is applied. Fix-all does this for each CCCMB001 in the document, the project, or the solution. Which pairs report does not change.

This is the rule and the fixer in this repository. It does not say how often the fix is applied.
