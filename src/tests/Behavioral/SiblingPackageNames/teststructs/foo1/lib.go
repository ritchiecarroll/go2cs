// Package foo shares its name with teststructs/foo2's package: two import paths, one parent, one
// package name.
package foo

// Name reports which sibling answered.
func Name() string {
	return "foo1"
}

// Pair exists only in this sibling.
type Pair struct {
	Left, Right int
}

// Sum adds the pair.
func (p Pair) Sum() int {
	return p.Left + p.Right
}
