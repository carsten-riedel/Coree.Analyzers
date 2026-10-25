# Analyzers

Coree.Analyzers is a repository of Roslyn analyzer packages that pack separately. Typography and CodeClarity are the first two. The repository text says they will not necessarily be the last.

Both packages look at C# the compiler already accepts. Typography is about characters a word processor leaves behind. The package text says compilers do not care and reviewers do. It flags the en dash, the em dash, typographic quotation marks, the typographic apostrophe, the horizontal ellipsis, the minus sign, and the no-break space, in C# and in the extra project files a consumer ships. ASCII hyphens, quotes, apostrophes, periods, and spaces stay silent.

CodeClarity is about readability of that same accepted C#. CCCRC001 flags a return, yield return, or returned arrow expression whose calculation-step score is above the configured maximum. A void arrow, a constructor, a setter, a lambda, and an async method that returns a non-generic Task or ValueTask are not scored. The default maximum is 1. The published description counts arithmetic other than string concatenation, method calls other than `nameof`, and conditional operators. How that last phrase lines up with `&&` and `||` is still open; see the domain clues. CCCPL001 is the parameter-list shape rule in the parameter list topic. CCCPL002 reports a broken parameter list whose parameters still fit in the configured length, 120 characters by default. CCCNS001 reports a class, struct, record, interface, enum, or delegate declared directly in a file, outside any namespace. Nested types are left to that outer type. The type namespace topic records the confirmed boundary. CCCMB001 reports the later of two methods with a body that are consecutive members of the same type and have no blank line between them. The method blank line topic records which neighboring members stay quiet.

A consumer chooses the severity. The default is warning.

Evidence: README.md.
