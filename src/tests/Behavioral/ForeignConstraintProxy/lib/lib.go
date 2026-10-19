// Package lib declares the constraints and the generic types over them; it never instantiates them at
// elem.Node, so it publishes no concrete proxy for that pair.
package lib

// Lesser is a generic interface used as a SELF-REFERENTIAL constraint.
type Lesser[T any] interface{ Less(T) bool }

// Ord2 constrains its parameter by Lesser over itself (the named form).
type Ord2[T Lesser[T]] struct{ V T }

func (o Ord2[T]) Below(t T) bool { return o.V.Less(t) }

// Ord constrains its parameter by an inline interface naming itself (the inline form).
type Ord[T interface{ Less(T) bool }] struct{ V T }

func (o Ord[T]) Below(t T) bool { return o.V.Less(t) }
