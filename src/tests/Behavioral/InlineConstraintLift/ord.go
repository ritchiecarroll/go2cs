package main

// A generic TYPE whose inline constraint names its own parameter lifts to `Ord_T<T>`. Its methods reach the
// constraint as their receiver's copy; each instantiation is closed over its argument as a named
// constraint is: a value records `GoImplement<age, Ord_T<age>>`, a pointer takes the proxy `nodeжOrd_T`,
// and a nested generic argument records the closed pair the generator re-opens over `wrap<E>`.
type Ord[T interface{ Less(T) bool }] struct{ v T }

func (o Ord[T]) Below(t T) bool { return o.v.Less(t) }

type Lesser[T any] interface{ Less(T) bool }

type age int

func (a age) Less(b age) bool { return a < b }

type node struct{ k int }

func (n *node) Less(o *node) bool { return n.k < o.k }

type wrap[E any] struct{ n int }

func (w wrap[E]) Less(o wrap[E]) bool { return w.n < o.n }
