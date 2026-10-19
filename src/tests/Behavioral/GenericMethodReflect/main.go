// Methods of a generic type, reached through reflect and through a dynamic interface assertion.
// go2cs emits each such method as a generic extension method (`String<K, V>(this Pair<K, V> p)`);
// golib must close it over the value's own type arguments before it can bind it.
package main

import (
	"fmt"
	"reflect"
)

// Plain is the non-generic control.
type Plain struct{ v int }

func (p Plain) Get() int                      { return p.v }
func (p Plain) Describe(prefix string) string { return fmt.Sprint(prefix, p.v) }
func (p *Plain) Set(v int)                    { p.v = v }

// Box has one type parameter, with value- and pointer-receiver methods.
type Box[T any] struct{ v T }

func (b Box[T]) Get() T                        { return b.v }
func (b Box[T]) Describe(prefix string) string { return fmt.Sprint(prefix, b.v) }
func (b *Box[T]) Set(v T)                      { b.v = v }

// Pair has two type parameters, and Stringer is one of its value methods.
type Pair[K comparable, V any] struct {
	k K
	v V
}

func (p Pair[K, V]) Key() K            { return p.k }
func (p Pair[K, V]) String() string    { return fmt.Sprintf("%v=%v", p.k, p.v) }
func (p *Pair[K, V]) Swap(v V) (old V) { old, p.v = p.v, v; return old }

type age int

// Ord2's type parameter carries a type-set constraint.
type Ord2[T ~int] struct{ a, b T }

func (o Ord2[T]) Less() bool { return o.a < o.b }

// Cell has the shape of database/sql's Null[T], asserted to an interface it never names.
type Cell[T any] struct {
	V     T
	Valid bool
}

func (c Cell[T]) Value() (any, error) {
	if !c.Valid {
		return nil, nil
	}
	return c.V, nil
}

type valuer interface{ Value() (any, error) }

type swapper interface{ Swap(v int) int }

// try prints one call's result, or the panic it raised, so one failure does not hide the rest.
func try(label string, f func() string) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Printf("%-36s PANIC %v\n", label, r)
		}
	}()
	fmt.Printf("%-36s %s\n", label, f())
}

func show(results []reflect.Value) string {
	s := "["
	for i, r := range results {
		if i > 0 {
			s += " "
		}
		s += fmt.Sprint(r.Interface())
	}
	return s + "]"
}

func names(t reflect.Type) string {
	s := fmt.Sprint(t.NumMethod(), ":")
	for i := 0; i < t.NumMethod(); i++ {
		s += " " + t.Method(i).Name
	}
	return s
}

func args(values ...any) []reflect.Value {
	var in []reflect.Value
	for _, a := range values {
		in = append(in, reflect.ValueOf(a))
	}
	return in
}

// probe walks the value's and the pointer's method sets by index and by name, through the bound
// method value and through the unbound func, in Go's method-table order. Every reflect call runs
// inside try, and the method names come from goSets, so one failing call hides no other.
func probe(name string, value, pointer any, in map[string][]any) {
	v := reflect.ValueOf(value)
	pv := reflect.ValueOf(pointer)

	try(name+" value methods", func() string { return names(v.Type()) })
	try(name+" pointer methods", func() string { return names(pv.Type()) })

	for i, n := range goSets[name][0] {
		try(fmt.Sprintf("%s v.Method(%d)", name, i), func() string {
			return n + " " + show(v.Method(i).Call(args(in[n]...)))
		})
		try(fmt.Sprintf("%s T.Method(%d).Func", name, i), func() string {
			m := v.Type().Method(i)
			return m.Name + " " + show(m.Func.Call(append([]reflect.Value{v}, args(in[n]...)...)))
		})
		try(fmt.Sprintf("%s v.MethodByName(%s)", name, n), func() string {
			return show(v.MethodByName(n).Call(args(in[n]...)))
		})
	}

	for i, n := range goSets[name][1] {
		try(fmt.Sprintf("%s pv.Method(%d)", name, i), func() string {
			return n + " " + show(pv.Method(i).Call(args(in[n]...)))
		})
		try(fmt.Sprintf("%s *T.Method(%d).Func", name, i), func() string {
			m := pv.Type().Method(i)
			return m.Name + " " + show(m.Func.Call(append([]reflect.Value{pv}, args(in[n]...)...)))
		})
	}

	try(name+" after pointer calls", func() string { return fmt.Sprint(pv.Elem().Interface()) })
}

// goSets are the Go method sets of the probe's types, value then pointer, written out so the
// walk never depends on the reflection under test.
var goSets = map[string][2][]string{
	"Plain":            {{"Describe", "Get"}, {"Describe", "Get", "Set"}},
	"Box[int]":         {{"Describe", "Get"}, {"Describe", "Get", "Set"}},
	"Box[string]":      {{"Describe", "Get"}, {"Describe", "Get", "Set"}},
	"Box[[]int]":       {{"Describe", "Get"}, {"Describe", "Get", "Set"}},
	"Pair[string,int]": {{"Key", "String"}, {"Key", "String", "Swap"}},
	"Ord2[age]":        {{"Less"}, {"Less"}},
}

func main() {
	probe("Plain", Plain{v: 7}, &Plain{v: 1}, map[string][]any{"Describe": {"p="}, "Set": {9}})
	probe("Box[int]", Box[int]{v: 7}, &Box[int]{v: 1}, map[string][]any{"Describe": {"b="}, "Set": {9}})
	probe("Box[string]", Box[string]{v: "s"}, &Box[string]{v: "t"}, map[string][]any{"Describe": {"b="}, "Set": {"u"}})
	probe("Box[[]int]", Box[[]int]{v: []int{1}}, &Box[[]int]{v: []int{2}}, map[string][]any{"Describe": {"b="}, "Set": {[]int{3, 4}}})
	probe("Pair[string,int]", Pair[string, int]{k: "a", v: 1}, &Pair[string, int]{k: "b", v: 2}, map[string][]any{"Swap": {5}})
	probe("Ord2[age]", Ord2[age]{a: 1, b: 2}, &Ord2[age]{a: 3, b: 2}, nil)

	try("fmt generic Stringer", func() string { return fmt.Sprint(Pair[string, int]{k: "z", v: 26}) })
	try("fmt *generic Stringer", func() string { return fmt.Sprint(&Pair[string, int]{k: "y", v: 25}) })
	try("fmt non-generic Stringer", func() string { return fmt.Sprint(Pair[string, int]{k: "x", v: 24}.String()) })

	try("assert generic to Stringer", func() string {
		s, ok := any(Pair[string, int]{k: "z", v: 26}).(fmt.Stringer)
		if !ok {
			return "false"
		}
		return "true " + s.String()
	})
	try("assert *generic to Stringer", func() string {
		s, ok := any(&Pair[string, int]{k: "z", v: 26}).(fmt.Stringer)
		if !ok {
			return "false"
		}
		return "true " + s.String()
	})
	try("assert generic to swapper", func() string {
		_, ok := any(Pair[string, int]{k: "z", v: 26}).(swapper)
		return fmt.Sprint(ok)
	})
	try("assert *generic to swapper", func() string {
		p := &Pair[string, int]{k: "w", v: 1}
		s, ok := any(p).(swapper)
		if !ok {
			return "false"
		}
		old := s.Swap(2)
		return fmt.Sprint(ok, " ", old, " ", p.v)
	})
	try("assert generic Cell to valuer", func() string {
		c, ok := any(Cell[uint]{V: 1, Valid: true}).(valuer)
		if !ok {
			return "false"
		}
		v, err := c.Value()
		return fmt.Sprint(ok, " ", v, " ", err)
	})
}
