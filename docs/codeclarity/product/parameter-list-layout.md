# Parameter list layout

CodeClarity diagnostic CCCPL001 is a shape rule for a parameter list. It reports a method, local function, constructor, operator, conversion operator, or primary constructor whose list is neither one line, parentheses included, nor one parameter per line with the closing parenthesis on the line after the last parameter ends. Calls, indexers, delegates, and lambdas stay out of it. An empty list and a single parameter stay quiet. A conversion operator has one parameter, so CCCPL001 stays quiet on it. The default severity is warning. The property is `ParameterListLayoutAnalyzerSeverity`.

A parameter may span lines. An attribute or a default on earlier lines of that parameter does not by itself report. A later parameter that starts on the earlier parameter's last line does. A closing parenthesis on the last parameter's line does.

CCCPL002 is a separate length rule. A list can satisfy CCCPL001 and still report CCCPL002 when the parameters would fit on one line. The default fit maximum is 120 characters.

In a C# document, the editor can expand a list that reports CCCPL001 when `ParameterListFit.UsesBreakButFits` is false for that list. Each parameter starts on its own line, and `)` stands on the following line. The indent is the indent of the line that contains `(`, plus one level: a tab when that indent is only tabs, otherwise four spaces. Text inside a parameter stays. A comment, a documentation comment, or a directive between the parameters is outside those parameter spans, and the fix is not offered rather than drop that text. Characters outside the parameter list stay. The compiler does not apply the edit. The warning remains until the fix is applied. Fix-all does this for each such CCCPL001 in the document, the project, or the solution. Which lists report does not change.

A short list that still fits is left to CCCPL002. Expanding it would leave that warning in place.

This is the rule and the fixer in this repository. It does not say how often the fix is applied.
