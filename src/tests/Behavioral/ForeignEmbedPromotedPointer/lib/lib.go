// Package lib is a sub-library whose types are embedded from another package: its
// method hosts live in a child namespace the embedding package's adapters do not import.
package lib

// Named is satisfied by a struct that embeds T, through promotion.
type Named interface {
	Name() string
}

// Counter is satisfied only by a pointer, through T's pointer-receiver method.
type Counter interface {
	Name() string
	Inc() int
}

// T is embedded by value in the main package.
type T struct {
	n int
}

// Name has a value receiver.
func (t T) Name() string { return "lib.T" }

// Inc has a pointer receiver and writes through it.
func (t *T) Inc() int {
	t.n++
	return t.n
}
