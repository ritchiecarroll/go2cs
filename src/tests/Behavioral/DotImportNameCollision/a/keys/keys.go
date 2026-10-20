// Package keys is the DOT-imported package of the pair.
package keys

type Key struct{ name string }

func NewKey(name string) *Key { return &Key{name: name} }

func (k *Key) Name() string { return k.name }

type Count int

type Box[T any] struct{ V T }

func Wrap[T any](v T) Box[T] { return Box[T]{V: v} }
