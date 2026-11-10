// Package shapes declares a free function and a method with the same name whose C# signatures are
// identical once the method's receiver becomes the extension method's `this` parameter: the shape of
// tsc's internal/checker (`func IsTupleType(t *Type) bool` beside `func (t *Type) IsTupleType() bool`).
package shapes

type Type struct {
	Flags int
}

func isTupleType(t *Type) bool { return t.Flags&1 != 0 }

func (t *Type) IsTupleType() bool { return isTupleType(t) }

// IsTupleType collides with the method above: it is emitted Δ-renamed, and every caller follows,
// including callers in other packages.
func IsTupleType(t *Type) bool { return isTupleType(t) }

// Combine and its method twin collide with an extra parameter too.
func (t *Type) Combine(other int) int { return t.Flags + other }

func Combine(t *Type, other int) int { return t.Flags*100 + other }

// Size and its method twin do NOT collide: the free function takes a second parameter, so the emitted
// signatures differ and neither is renamed (the control).
func (t *Type) Size() int { return t.Flags }

func Size(t *Type, scale int) int { return t.Flags * scale }
