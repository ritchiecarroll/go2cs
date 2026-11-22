// Names reached through nested namespaces of one module, where a C# namespace or a using can capture a name Go
// reads unambiguously:
//   - lib: a sub-package named like the standard library's sync, imported unaliased beside it;
//   - check: a failed assertion from an interface that embeds one of another nested namespace;
//   - user: a package named only through an inferred type, beside a namespace of its name.
package main

import (
	"fmt"

	"NestedNamespaceReach/b/x/check"
	"NestedNamespaceReach/lib"
	"NestedNamespaceReach/user"
)

func main() {
	var c lib.Counter
	c.Inc()
	fmt.Println(c.Inc(), lib.Tag())

	fmt.Println(check.Run())

	fmt.Println(user.Value())
}
