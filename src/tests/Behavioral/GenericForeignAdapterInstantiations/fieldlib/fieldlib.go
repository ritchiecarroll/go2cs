// Package fieldlib declares the generic type and the interface in a package OTHER than the one
// that converts it, the shape of tsc's internal/packagejson (Expected[T], TypeValidatedField)
// used from internal/module.
package fieldlib

import "fmt"

type Field interface {
	IsPresent() bool
	Describe() string
}

type Expected[T any] struct {
	Value T
	Valid bool
}

func (e *Expected[T]) IsPresent() bool { return e.Valid }

func (e *Expected[T]) Describe() string { return fmt.Sprintf("%v", e.Value) }

// Box's required method MENTIONS its type parameter, so exactly one instantiation, Box[string],
// satisfies Getter. That pair keeps the closed, non-generic adapter (fieldlib_BoxжGetter).
type Getter interface {
	Get() string
}

type Box[T any] struct {
	Item T
}

func (b *Box[T]) Get() T { return b.Item }
