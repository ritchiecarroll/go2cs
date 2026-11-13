// Package boxlib declares a generic type and a non-generic interface whose method names the type
// argument, and converts the one instantiation that satisfies it itself. The package that imports
// it converts the same instantiation again.
package boxlib

type Point struct{ X, Y int }

type Box[T any] struct{ V T }

func (b Box[T]) Val() T { return b.V }

type Valuer interface{ Val() Point }

// Wrap converts in the declaring package.
func Wrap(x, y int) Valuer {
	return Box[Point]{Point{x, y}}
}
