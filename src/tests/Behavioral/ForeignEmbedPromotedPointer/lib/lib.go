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

// Adder is satisfied through a promoted method that takes arguments and returns two results.
type Adder interface {
	Add(a, b int) (int, error)
}

// Add has a pointer receiver, two arguments and two results.
func (t *T) Add(a, b int) (int, error) {
	t.n += a + b

	if t.n > 100 {
		return t.n, errBig
	}

	return t.n, nil
}

type bigError struct{}

func (bigError) Error() string { return "too big" }

var errBig error = bigError{}

// Tagger is satisfied two embeds deep: an outer struct embeds V, and V embeds Inner.
type Tagger interface {
	Tag() string
}

// Inner holds the method.
type Inner struct {
	tag string
}

// SetTag and Tag have pointer receivers.
func (i *Inner) SetTag(tag string) { i.tag = tag }
func (i *Inner) Tag() string       { return "inner:" + i.tag }

// V embeds Inner by value, so Tag is promoted through V.
type V struct {
	Inner
}
