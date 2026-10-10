# Parameter list layout

CCCPL001 allows two shapes for a parameter list on a method, local function, constructor, or primary constructor.

The one-line shape keeps the opening parenthesis, every parameter, and the closing parenthesis on the same line.

The list shape puts the opening parenthesis at the end of the signature line and starts each parameter on its own following line. The closing parenthesis stands on the line after the last parameter. A single parameter and an empty list stay quiet either way.

On 2026-10-10 the maintainer withdrew shapes this rule had been accepting. A packed parameter line under the declaration name reports. So does a one-line list whose closing parenthesis drops to the next line, a list whose first parameter stays on the signature line, and a list whose closing parenthesis shares the last parameter line.

Calls, indexers, delegates, and lambdas stay out of the rule. The wider declaration set, beyond methods and record primary constructors, was reported with the implementation. Each of those kinds was not confirmed on its own.

CCCPL002 is a separate analyzer. It reports a parameter list that is broken across lines when the parameters would still fit on one line. The length joins the parameters with `, ` after whitespace collapses. Parentheses are not counted. `ParameterListFitAnalyzerMaximum` defaults to 120. The two CCCPL001 shapes stay allowed; a short list form can report CCCPL002 while CCCPL001 stays quiet.

On 2026-10-10 the maintainer raised that default from 80 to 120. Published line-length guides were treated as possibly outdated for current widescreen monitors. The stated reason is that agents wrap too early, including short lists that still read on one line, such as `public void M(float amplitude, float frequencyX, float frequencyZ, float detailScale, int age)`. That parameter text is 79 characters. This analyzer counts parameters only, so a 120-character parameter list is a full `public void M(...)` line near 135 columns.

Evidence: maintainer discussion on 2026-10-08 and 2026-10-10; src/prj/Coree.Analyzers.CodeClarity/ParameterListLayout.cs; src/prj/Coree.Analyzers.CodeClarity/ParameterListFit.cs; README.md CodeClarity section.
