# Parameter list length

CodeClarity diagnostic CCCPL002 warns when a parameter list is broken across lines even though the parameters would still fit on one line. It applies to a method, local function, constructor, or primary constructor.

The measured length joins the parameters with a comma and a space. Tabs, line breaks, and repeated spaces outside tokens collapse. Whitespace inside a literal stays, because it belongs to the default value. Parentheses are not counted. `ParameterListFitAnalyzerMaximum` sets the limit. The default is 120. An empty, negative, or non-numeric maximum keeps that default.

`float a, float b, float c` is 25 characters, so this reports at the default:

```csharp
void M(
    float a,
    float b,
    float c)
```

The same parameters on one line stay quiet. A longer list that is broken because it does not fit stays quiet.

CCCPL002 stays quiet when the list contains a `//` comment. Putting that list on one line would comment out the rest of the signature. It also stays quiet when the list contains a preprocessor directive, such as `#if`, `#region`, `#nullable`, `#pragma`, or `#line`. A directive has to occupy its own line, so the one-line spelling the diagnostic recommends cannot keep it. The same characters inside a string are ordinary text. A parameter default of `"http://example"` or `"#if true"` can still report.

A block comment does not get that exception. `int /* amplitude */ a` is valid on one line, and a wrapped list that contains it still reports CCCPL002 when the parameters fit.

CCCPL001 is a separate shape rule. A list can satisfy that shape and still report CCCPL002. A directive or a `//` comment does not turn CCCPL001 off.
