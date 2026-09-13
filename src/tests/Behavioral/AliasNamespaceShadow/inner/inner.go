// Package inner emits into namespace `go.AliasNamespaceShadow` — NOT the root `go` — which is what
// makes it able to observe the shadowing at all. It imports BOTH the standard library's sort and a
// module-local package whose simple name is also sort, so both `go.sort_package` and
// `go.AliasNamespaceShadow.sort_package` are in the reference closure at once.
//
// The alias for the STDLIB sort must therefore be emitted ROOT-QUALIFIED (`go.sort_package`). Left
// unqualified it is `sort_package`, and C# resolves outward from the enclosing namespace, so the
// NEARER `go.AliasNamespaceShadow.sort_package` wins — a class that has no Ints.
package inner

import (
	"sort"

	local "AliasNamespaceShadow/sortlocal"
)

// SortThree uses a member only the STDLIB sort has, so a mis-bound alias cannot compile.
func SortThree() []int {
	s := []int{3, 1, 2}
	sort.Ints(s)
	return s
}

// LocalTag reaches the module-local package through its OWN explicit alias, which is the binding
// that must keep working: the fix root-qualifies the shadowed alias without disturbing this one.
func LocalTag() string {
	return local.Tag()
}
