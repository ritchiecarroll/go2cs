// Package sort deliberately shares its SIMPLE NAME with the standard library's sort. It emits as
// class `sort_package` in namespace `go.AliasNamespaceShadow`, which is the NEARER class that
// shadows a sibling's unqualified `using sort = sort_package;` alias for the STDLIB sort.
//
// It carries no Ints, on purpose: if an alias meant for stdlib sort binds here instead, the
// consumer stops compiling rather than silently sorting nothing.
package sort

// Tag names which package answered, so a run can tell the two apart.
func Tag() string {
	return "local"
}
