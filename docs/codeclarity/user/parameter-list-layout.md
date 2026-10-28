# Parameter list layout

CodeClarity diagnostic CCCPL001 warns when a parameter list is neither of two shapes. It applies to a method, local function, constructor, or primary constructor. Calls, indexers, delegates, and lambdas stay out of it. An empty list and a single parameter stay quiet.

One shape keeps the opening parenthesis, every parameter, and the closing parenthesis on the same line.

The other shape starts each parameter on its own line. The closing parenthesis stands on the line after the last line of the last parameter. An attribute or a default may occupy earlier lines of that parameter. A later parameter that starts on the earlier parameter's last line still shares that line, and the list reports.

This reports CCCPL001, because two parameters share a line and `)` is on the last parameter's line:

```csharp
public void M(
    float a, float b,
    float c)
```

This does not:

```csharp
public void M(
    float a,
    float b,
    float c
)
```

CCCPL002 is a separate warning. It reports a broken list whose parameters would still fit on one line. The editor expansion below is not offered for that list. The sample with `)` on the last line of a short list can be one of those, and this edit leaves it alone.

In a C# document, when CCCPL001 reports and CCCPL002 does not, the editor can put each parameter on its own line and put `)` on the following line. The lightbulb action is "Put each parameter on its own line." The new lines use the indent of the line that contains `(`, plus one more level: a tab when that indent is tabs, otherwise four spaces. Text inside a parameter stays, including a block comment and a line break in a literal. A `//` comment, a `///` documentation comment, or a preprocessor directive between the parameters keeps the edit from being offered, because the rewrite would drop that text. Characters outside the parameter list stay where they are.

The compiler does not apply the change. The warning remains until you apply the fix. Fix-all expands each such CCCPL001 in the document, the project, or the solution. Which lists report does not change.

This is the behavior of the fixer in this repository. It does not say how often people apply it.
