// Package coll declares types whose names are ALSO method names in this package, so the converted package
// Δ-renames them (tsc's internal/collections: `type Set[T]` beside `OrderedMap.Set`; jsontext: `type Kind` beside
// `Token.Kind`). Bag is an unrenamed generic, the control.
package coll

type Set[T comparable] struct{ m map[T]struct{} }

func (s *Set[T]) Add(v T) {
	if s.m == nil {
		s.m = map[T]struct{}{}
	}
	s.m[v] = struct{}{}
}

func (s *Set[T]) Has(v T) bool { _, ok := s.m[v]; return ok }

func (s *Set[T]) Len() int { return len(s.m) }

func NewSet[T comparable](vs ...T) *Set[T] {
	s := &Set[T]{}
	for _, v := range vs {
		s.Add(v)
	}
	return s
}

// Pair collides with Entry.Pair below.
type Pair[K comparable, V any] struct {
	Key K
	Val V
}

type OrderedMap[K comparable, V any] struct {
	keys []K
	m    map[K]V
}

func (o *OrderedMap[K, V]) Set(k K, v V) {
	if o.m == nil {
		o.m = map[K]V{}
	}
	if _, ok := o.m[k]; !ok {
		o.keys = append(o.keys, k)
	}
	o.m[k] = v
}

type Entry struct{ k string }

func (e Entry) Pair() Pair[string, int] { return Pair[string, int]{Key: e.k, Val: len(e.k)} }

func (e Entry) Kind() Kind { return Kind(len(e.k)) }

// Kind is the NON-generic renamed control (it already resolves through its `collꓸKind` alias).
type Kind byte

// Bag is the unrenamed generic control.
type Bag[T any] struct{ items []T }

func (b *Bag[T]) Put(v T) { b.items = append(b.items, v) }

func (b *Bag[T]) Count() int { return len(b.items) }
