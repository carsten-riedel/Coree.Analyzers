# Method blank line

CCCMB001 reports the later of two methods with a body that are consecutive members of the same type when no blank line sits between them.

A blank line is a line that is empty or whitespace only. Indentation on the next method's line does not count. A comment, a documentation comment, or an attribute on the line between them does not replace the blank line. A blank line above that comment, documentation, or attribute does.

The rule looks only at method declarations that have a block body or an expression body. Constructors, fields, properties, operators, indexers, nested types, and local functions stay quiet when they sit beside a method. A method signature without a body, such as an interface or abstract declaration, stays quiet. The inside of a method stays quiet.

The severity property is `MethodBlankLineAnalyzerSeverity`. The default is warning.

Evidence: maintainer confirmation on 2026-10-10; src/prj/Coree.Analyzers.CodeClarity/MethodBlankLine.cs; README.md CodeClarity section.
