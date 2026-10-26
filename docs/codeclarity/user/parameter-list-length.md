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

CCCPL002 stays quiet when the list contains a `//` comment or a `///` documentation comment. Putting that list on one line would comment out the rest of the signature. This stays quiet:

```csharp
void M(
    /// <param name="width">Width in pixels.</param>
    int width,
    int height)
```

A `///` comment on the method, above the parameter list, is not inside the list. A short wrapped list under that comment still reports.

It also stays quiet when the list contains a preprocessor directive, such as `#if`, `#region`, `#nullable`, `#pragma`, or `#line`. A directive has to occupy its own line, so the one-line spelling the diagnostic recommends cannot keep it. The same characters inside a string are ordinary text. A parameter default of `"http://example"`, `"#if true"`, or `"/// width"` can still report.

A block comment does not get that exception. `int /* amplitude */ a` is valid on one line, and a wrapped list that contains it still reports CCCPL002 when the parameters fit.

A literal that contains a line break does. This stays quiet:

```csharp
void M(
    int limit,
    string query = @"select
        id")
```

The line break is part of the string, so the one-line spelling would change it. The same is true of a raw string and of an interpolated verbatim or raw string. CR, LF, CRLF, next line (U+0085), line separator (U+2028), and paragraph separator (U+2029) are the breaks that count. An escaped `\n` in a regular string is still one source line. `string value = "select\nid"` can still report when the list is wrapped and the parameters fit.

CCCPL001 is a separate shape rule. A list can satisfy that shape and still report CCCPL002. A directive or a `//` comment does not turn CCCPL001 off.
