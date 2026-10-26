// Package keys is the NAMED import of the pair: the same package name as the dot-imported one.
package keys

type Key struct{ id int }

func NewKey(id int) *Key { return &Key{id: id} }

func (k *Key) ID() int { return k.id }
