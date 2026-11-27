// Package lib declares interfaces with UNEXPORTED methods. Another package's struct satisfies them
// only by embedding an interface FIELD, so the promoted method is the field's own: the adapter
// forwards through the field and reaches the dynamic value's method, as Go does.
package lib

// I has an unexported method, which only this package's types can declare.
type I interface {
	m() string
	Name() string
}

// T is this package's implementation of I.
type T struct{ name string }

func (t T) m() string    { return "lib.m of " + t.name }
func (t T) Name() string { return t.name }

// New returns a T as an I.
func New(name string) I { return T{name: name} }

// CallM calls the unexported method through the interface.
func CallM(i I) string { return i.m() }

// Sentinel is the crypto/internal/fips140/drbg.DefaultReader shape: its only method is unexported, so
// a value satisfies it only by holding one of this package's values in an embedded field.
type Sentinel interface{ sentinel() }

type mark struct{}

func (mark) sentinel() {}

// Mark is the one Sentinel this package hands out.
var Mark Sentinel = mark{}

// IsMarked reports whether x satisfies Sentinel, asked at run time.
func IsMarked(x any) bool {
	_, ok := x.(Sentinel)
	return ok
}
