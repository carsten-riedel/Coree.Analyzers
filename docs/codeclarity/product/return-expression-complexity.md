# Return expression complexity

CodeClarity diagnostic CCCRC001 is a readability limit on a value that is returned. It applies to a `return`, a `yield return`, and an arrow that returns a value. The limit is `ReturnTypeComplexityAnalyzerMaximum`. The default is 1. The default severity is warning.

One step is an arithmetic operator other than built-in string concatenation, a shift (`<<`, `>>`, or `>>>`), a method call other than the C# `nameof` operator, the conditional operator `?:`, or a user-defined `operator +`. A user-defined addition counts even when one operand is a string. `&&` and `||` do not count. Built-in string concatenation does not count. A method that is only named `nameof` or `@nameof` does count.

At the default maximum, `return a + b` and `yield return a + b` stay quiet. `return a + b + c` and `yield return a + b + c` report. `yield break` has no value and stays quiet. `int Height(int a, int b, int c) => a + b + c` reports. `int Height(int a, int b) => a + b` stays quiet.

A void arrow, a constructor, a setter, a lambda, and an async method that returns a non-generic `Task` or `ValueTask` stay quiet. Those arrows are not returned values. Moving a calculation from `return` to `yield return`, or to an arrow that returns that value, does not remove the warning.

The warning on a `return` or `yield return` says to extract the extra steps into local variables. The warning on a returned arrow says to move that arrow into a block first, because an arrow cannot declare a local. The diagnostic id, the title, and which expressions report are the same for both sentences.

This is the rule the analyzer implements. It does not say whether a reader follows either sentence.
