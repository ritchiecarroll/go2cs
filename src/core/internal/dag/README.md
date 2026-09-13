# go.internal.dag

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-6%2F6_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.12.3/internal.dag.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.12-00ADD8?logo=go)](https://pkg.go.dev/internal/dag@go1.23.12)\
[![Source](https://img.shields.io/badge/Source-@1.23.12-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/internal/dag) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/internal/dag)

Package dag implements a language for expressing directed acyclic graphs.

The general syntax of a rule is:

	a, b < c, d;

which means c and d come after a and b in the partial order (that is, there are edges from c and d to a and b), but doesn't provide a relative order between a vs b or c vs d.

The rules can chain together, as in:

	e < f, g < h;

which is equivalent to

	e < f, g;
	f, g < h;

Except for the special bottom element "NONE", each name must appear exactly once on the right-hand side of any rule. That rule serves as the definition of the allowed successor for that name. The definition must appear before any uses of the name on the left-hand side of a rule. (That is, the rules themselves must be ordered according to the partial order, for easier reading by people.)

Negative assertions double-check the partial order:

	i !< j

means that it must NOT be the case that i \< j. Negative assertions may appear anywhere in the rules, even before i and j have been defined.

Comments begin with #.

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
