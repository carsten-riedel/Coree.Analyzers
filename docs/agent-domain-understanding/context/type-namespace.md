# Type namespace

CCCNS001 reports a class, struct, record, interface, enum, or delegate whose declaration sits directly in the file, outside any namespace.

A block namespace and a file-scoped namespace both satisfy the rule. Nested types are not reported on their own: the outer type carries the diagnostic, and moving that type into a namespace covers the nested ones. An empty file, a using, and top-level statements without a type stay quiet. A type that follows top-level statements and still has no namespace reports.

Generated code stays out, the same way the other CodeClarity analyzers skip it. Folder matching, a required root namespace, and the choice between a block and a file-scoped namespace are separate rules and are not part of CCCNS001.

The severity property is `TypeNamespaceAnalyzerSeverity`. The default is warning.

Evidence: maintainer confirmation on 2026-10-10; src/prj/Coree.Analyzers.CodeClarity/TypeNamespaceAnalyzer.cs; README.md CodeClarity section.
