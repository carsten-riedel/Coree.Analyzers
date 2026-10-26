# Method blank line

CodeClarity diagnostic CCCMB001 warns when two methods with a body are consecutive members of the same type and no blank line separates them. The warning is on the later method.

A whitespace-only line counts as that blank line. A comment, a documentation comment, or an attribute between the methods does not. Constructors, fields, properties, operators, indexers, local functions, and methods without a body stay quiet.

Blank lines are recognized with LF, CRLF, CR, next line (U+0085), line separator (U+2028), and paragraph separator (U+2029), including mixed endings in one file.

In a C# document, the editor can insert the missing blank line. The new line uses the line ending already in the file. Text between the methods, including a comment or an attribute, stays below that line. Two methods written on one line are split by the empty line. The compiler does not apply the change. The warning remains until you apply the fix. Fix-all inserts that line for each CCCMB001 in the document, the project, or the solution.

This is the behavior of the fixer in this repository. It does not say how often people apply it.
