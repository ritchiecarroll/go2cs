// Package foo shares its name with teststructs/foo1's package: two import paths, one parent, one
// package name.
package foo

// Name reports which sibling answered.
func Name() string {
	return "foo2"
}

// Triple exists only in this sibling.
type Triple struct {
	A, B, C string
}

// Join concatenates the triple.
func (t Triple) Join() string {
	return t.A + t.B + t.C
}

// Alias is this sibling's exported type alias.
type Alias = Triple
