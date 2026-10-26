# ASCII replacement

Typography warns when a word processor leaves a special character in source. In a C# document, the editor can replace that character with the ASCII character the warning names. The compiler does not apply the change. One diagnostic can be fixed for the document, the project, or the solution.

An en dash, em dash, or minus sign becomes a hyphen. A curly double quote or guillemet becomes a straight double quote. A single quotation mark or typographic apostrophe becomes an apostrophe. An ellipsis becomes three periods. A no-break space or narrow no-break space becomes a normal space. A comment is left unchanged.

Inside a normal or interpolated string, a straight double quote is written as `\"`. Inside a verbatim string, it is written as `""`. Inside a character literal, an apostrophe is written as `\'`. A literal inside an interpolation hole follows that inner literal, not the outer string. A `u8` suffix follows the string it belongs to: `"..."u8` writes `\"`, and `@"..."u8` writes `""`.

A raw string's own text, including a `u8` raw string and the text of a raw interpolated string, is left unchanged. The warning still reports there. An ellipsis in a character literal is also left unchanged, and CTYEL001 still reports, because three periods are not a character literal. An ellipsis in a string still becomes three periods. A character-literal replacement that is still one character, including a hyphen or an escaped apostrophe, is still offered.

Files that are not C# documents can still report these diagnostics. The replacement is only offered for a C# document.

This is the replacement contract in this repository. It does not say how often the fix is applied.
