// Go interns type descriptors: every route to one type returns the same reflect.Type, so `==`,
// reflect.DeepEqual and a map or sync.Map keyed by the Type all agree. reflect.TypeFor[T]() is one
// such route, and Go 1.26's tests build expected errors with it (encoding/json's UnmarshalTypeError,
// encoding/xml's TagPathError) and compare them to decoded errors with reflect.DeepEqual.
package main

import (
	"fmt"
	"reflect"
	"sync"
)

type T struct{ X int }

type Arr [3]int

type M struct{}

func (M) Do(int) string { return "" }

type typeError struct{ Type reflect.Type }

func show(name string, a, b reflect.Type) {
	fmt.Printf("%-28s == %v  deep %v  (%v / %v)\n", name, a == b, reflect.DeepEqual(a, b), a, b)
}

func main() {
	var s T
	var p *T
	var a Arr
	var n int

	show("struct TypeOf/TypeFor", reflect.TypeOf(s), reflect.TypeFor[T]())
	show("struct TypeOf/&x.Elem", reflect.TypeOf(s), reflect.TypeOf(&s).Elem())
	show("struct TypeFor/TypeFor", reflect.TypeFor[T](), reflect.TypeFor[T]())
	show("ptr TypeOf/TypeFor", reflect.TypeOf(p), reflect.TypeFor[*T]())
	show("ptr TypeOf/&x.Elem", reflect.TypeOf(p), reflect.TypeOf(&p).Elem())
	show("ptr TypeFor/PointerTo", reflect.TypeFor[*T](), reflect.PointerTo(reflect.TypeFor[T]()))
	show("array TypeOf/TypeFor", reflect.TypeOf(a), reflect.TypeFor[Arr]())
	show("array TypeOf/&x.Elem", reflect.TypeOf(a), reflect.TypeOf(&a).Elem())
	show("int TypeOf/TypeFor", reflect.TypeOf(n), reflect.TypeFor[int]())
	show("value.Type/TypeFor", reflect.ValueOf(s).Type(), reflect.TypeFor[T]())
	show("anon struct", reflect.TypeOf(struct{}{}), reflect.TypeFor[struct{}]())

	var mm M
	mt := reflect.ValueOf(mm).Method(0).Type()
	show("method value Type/TypeOf", mt, reflect.TypeOf(mm.Do))
	show("method value Type/TypeFor", mt, reflect.TypeFor[func(int) string]())

	// An error value carrying a Type, compared the way Go's own tests compare decoded errors.
	fmt.Println("error DeepEqual TypeOf/TypeFor", reflect.DeepEqual(&typeError{reflect.TypeOf(s)}, &typeError{reflect.TypeFor[T]()}))

	m := map[reflect.Type]int{reflect.TypeFor[T](): 1, reflect.TypeFor[*T](): 2}
	_, ok1 := m[reflect.TypeOf(s)]
	_, ok2 := m[reflect.TypeOf(p)]
	fmt.Println("map key TypeFor->TypeOf", ok1, ok2)

	var sm sync.Map
	sm.Store(reflect.TypeOf(s), 1)
	_, ok3 := sm.Load(reflect.TypeFor[T]())
	sm.Store(reflect.TypeFor[Arr](), 2)
	_, ok4 := sm.Load(reflect.TypeOf(a))
	fmt.Println("sync.Map TypeOf->TypeFor", ok3, "TypeFor->TypeOf", ok4)
}
