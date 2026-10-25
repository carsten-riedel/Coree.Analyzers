# Return expression complexity

CodeClarity diagnostic CCCRC001 warns when a `return` or `yield return` expression contains more calculation steps than allowed. The warning asks for the extra steps to be extracted into local variables. `ReturnTypeComplexityAnalyzerMaximum` sets the limit. The default is 1.

Each of these counts as one step: an arithmetic operator other than built-in string concatenation, a shift (`<<`, `>>`, or `>>>`), a method call other than the `nameof` operator, a conditional operator (`?:`), and a user-defined `operator +`. A user-defined addition counts even when one operand is a string. Built-in string concatenation does not count. The C# `nameof` operator does not count, including when the maximum is 0. A call to an ordinary method that is only named `nameof` or `@nameof` does count.

At the default maximum, `return a + b` stays quiet and `return a + b + c` reports. A `yield return` expression uses that same score. `yield return a + b` stays quiet, and `yield return a + b + c` reports. `yield break` has no value expression and stays quiet.

An expression-bodied member such as `int M() => a + b + c` is not visited by this analyzer. Moving a calculation from `return` to `yield return` does not remove the warning. Moving it to an expression-bodied member still does.
