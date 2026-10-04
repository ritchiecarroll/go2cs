// Sibling-package-name guard (go-cmp's cmp/internal/teststructs/{foo1,foo2}): two DIFFERENT import
// paths under ONE parent both declare `package foo`. A package's Go identity is its import path, so
// Go has no collision here. The C# namespace used to drop the path's last segment and name the class
// for the package (`<parent>.foo_package`), which made both siblings the same fully qualified type,
// and any compilation referencing both read CS0433. When a package's name differs from its last path
// segment, the namespace now keeps the whole import path (`…teststructs.foo1.foo_package`), so each
// sibling is its own type, while a use site still reads `foo1.Name()` as in Go.
package main

import (
	"fmt"

	foo1 "SiblingPackageNames/teststructs/foo1"
	foo2 "SiblingPackageNames/teststructs/foo2"
)

func main() {
	fmt.Println(foo1.Name(), foo2.Name())

	// A type from each sibling, in a declaration and a composite literal.
	var a foo1.Pair = foo1.Pair{Left: 1, Right: 2}
	b := foo2.Triple{A: "x", B: "y", C: "z"}
	fmt.Println(a.Sum(), b.Join())

	// An exported type alias from each sibling resolves to its own package's type.
	var c foo1.Alias = foo1.Pair{Left: 3, Right: 4}
	var d foo2.Alias = foo2.Triple{A: "p", B: "q", C: "r"}
	fmt.Println(c.Sum(), d.Join())
}
