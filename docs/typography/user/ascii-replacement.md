# Replace a flagged character

Typography warns when a word processor leaves a special character in source. In a C# document, the editor can replace that character with the ASCII character the warning already names. The compiler does not apply the change. You choose it from the lightbulb, or as fix-all for that diagnostic in the document, the project, or the solution.

| Diagnostic | Flagged character | Replacement |
| --- | --- | --- |
| CTYED001 | En dash (U+2013), em dash (U+2014) | Hyphen (`-`) |
| CTYQM001 | Curly double quotes, guillemets | Straight double quote (`"`) |
| CTYQM001 | Single quotation marks U+2018, U+201A, U+201B | Apostrophe (`'`) |
| CTYAP001 | Typographic apostrophe (U+2019) | Apostrophe (`'`) |
| CTYEL001 | Horizontal ellipsis (U+2026) | Three periods (`...`) |
| CTYMN001 | Minus sign (U+2212) | Hyphen (`-`) |
| CTYNB001 | No-break space (U+00A0), narrow no-break space (U+202F) | Normal space |

A comment keeps that character as it is. Inside a normal or interpolated string, a straight double quote is written as `\"`, so the literal does not end early. Inside a verbatim string, it is written as `""`. Inside a character literal, an apostrophe is written as `\'`. A `u8` suffix uses that same escaping: `"..."u8` writes `\"`, and `@"..."u8` writes `""`.

A raw string (`"""`) is left unchanged, including one with a `u8` suffix. The warning still reports there. Changing the quotes that keep a raw string valid depends on its delimiter, and this fix does not guess.

Files that are not C# documents can still report these diagnostics. This replacement is only offered for a C# document.

This is the behavior of the fixer in this repository. It does not say how often people apply it.
