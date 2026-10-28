# Parameter list length

CodeClarity diagnostic CCCPL002 reports a parameter list that is broken across lines when the parameters would still fit on one line. It applies to a method, local function, constructor, or primary constructor. The default severity is warning. The property for that severity is `ParameterListFitAnalyzerSeverity`. The limit is `ParameterListFitAnalyzerMaximum`. The default is 120. An empty, negative, or non-numeric maximum keeps that default.

The length joins the parameters with a comma and a space. Tabs, line breaks, and repeated spaces outside tokens collapse. Whitespace inside a literal stays. Parentheses are not counted. At the default, `float a, float b, float c` reports when the list is broken across lines, and the same parameters on one line stay quiet. A list that does not fit stays quiet.

A `//` comment, a `///` documentation comment, a preprocessor directive, or a literal line break keeps the list quiet. The one-line spelling cannot keep that text. A `//`, `///`, or directive inside a string is ordinary text and can still report. A block comment can remain on one line, so a wrapped list that contains one can still report. A line break inside that comment is formatting whitespace.

In a C# document, the editor can write the flagged list on that one line. The compiler does not. A block comment the measurement keeps stays, and a line break inside it collapses to a space. A comment the measurement does not include is left in place, and the warning stays. The warning remains until the fix is applied. Fix-all does this for each CCCPL002 in the document, the project, or the solution. Which lists report does not change.

CCCPL001 is a separate shape rule. A list can satisfy that shape and still report CCCPL002.

This is the rule and the fixer in this repository. It does not say how often the fix is applied.
