// A named map whose VALUE contains the map itself -- directly, or through struct fields, fixed-size
// arrays, slices or maps -- cannot keep its golib `map<K,V>` inline in the generated wrapper: .NET
// fails to load the wrapper type (SIGSEGV or TypeLoadException, before any code of the type runs).
// go2cs-gen holds such a map in a reference holder. Every branch below crashed on .NET 10.0.12 with
// the map inline; the pointer arm is the control that keeps it inline.
package main

import (
	"fmt"
	"reflect"
	"runtime"
	"sort"
)

// Direct: the value is the map itself (go-cmp's cycleTests M).
type Direct map[int]Direct

func (d Direct) String() string { return fmt.Sprintf("Direct(%d)", len(d)) }

// DirectStr: the same through a string key (encoding/gob's recursiveMap).
type DirectStr map[string]DirectStr

type ViaStruct map[int]viaStructV

type viaStructV struct {
	n int
	m ViaStruct
}

type ViaNested map[int]viaNestedA

type viaNestedA struct{ b viaNestedB }

type viaNestedB struct{ m ViaNested }

type ViaArray map[int][2]ViaArray

type ViaSlice map[int][]ViaSlice

type ViaMap map[int]map[int]ViaMap

// ViaNamed: through a NAMED slice, whose wrapper's fields go2cs-gen generates (the walk reads its
// [GoType] definition instead).
type ViaNamed map[int]namedSlice

type namedSlice []ViaNamed

// Generic: a generic wrapper reaching itself.
type Generic[T any] map[int]Generic[T]

// ViaPtr: through a pointer, a reference -- the control, whose wrapper keeps its map inline.
type ViaPtr map[int]*ViaPtr

type DefinedOverStruct viaStructV

type PDirect *Direct

func direct() {
	var nilD Direct
	fmt.Println("direct nil:", nilD == nil, len(nilD))

	d := Direct{1: Direct{2: nil}, 3: {}}
	d[4] = Direct{5: Direct{6: nil}}
	fmt.Println("direct len:", len(d), len(d[1]), len(d[4][5]), d[1][2] == nil, d[3] == nil)

	v, ok := d[7]
	fmt.Println("direct missing:", v == nil, ok)

	delete(d, 3)
	keys := []int{}
	for k := range d {
		keys = append(keys, k)
	}
	sort.Ints(keys)
	fmt.Println("direct keys:", keys)

	m := make(Direct, 4)
	m[0] = m
	fmt.Println("direct self:", len(m[0][0][0]))
}

func directStr() {
	d := DirectStr{"a": DirectStr{"b": nil}, "c": {}}
	d["e"] = d["a"]
	e := d["e"]
	e["f"] = nil
	fmt.Println("directStr:", len(d), len(d["a"]), d["a"]["b"] == nil, d["c"] == nil)
}

func viaStruct() {
	s := ViaStruct{1: {n: 10, m: ViaStruct{2: {n: 20}}}}
	fmt.Println("viaStruct:", len(s), s[1].n, s[1].m[2].n, s[1].m[2].m == nil)
}

func viaNested() {
	x := ViaNested{1: {b: viaNestedB{m: ViaNested{2: {}}}}}
	fmt.Println("viaNested:", len(x), len(x[1].b.m), x[1].b.m[2].b.m == nil)
}

func viaArray() {
	a := ViaArray{1: {ViaArray{2: {}}, nil}}
	fmt.Println("viaArray:", len(a), len(a[1][0]), a[1][1] == nil)
}

func viaSlice() {
	s := ViaSlice{1: {ViaSlice{2: nil}, nil}}
	fmt.Println("viaSlice:", len(s), len(s[1]), len(s[1][0]), s[1][1] == nil)
}

func viaMap() {
	m := ViaMap{1: {2: ViaMap{3: nil}}}
	fmt.Println("viaMap:", len(m), len(m[1]), len(m[1][2]), m[1][2][3] == nil)
}

func viaNamed() {
	n := ViaNamed{1: namedSlice{ViaNamed{}, nil}}
	fmt.Println("viaNamed:", len(n), len(n[1]), n[1][0] != nil, n[1][1] == nil)
}

func generic() {
	g := Generic[string]{1: Generic[string]{2: nil}}
	fmt.Println("generic:", len(g), len(g[1]), g[1][2] == nil)
}

func viaPtr() {
	p := ViaPtr{}
	p[1] = &p
	fmt.Println("viaPtr:", len(p), len(*p[1]), (*p[1])[1] == p[1])
}

// reflection reaches every golib reader of a generated wrapper's value field with a holder wrapper.
func reflection() {
	d := Direct{1: Direct{2: nil}}

	t := reflect.TypeOf(d)
	fmt.Println("reflect type:", t.Kind(), t.Elem() == t, t.Key().Kind(), t.Size())
	fmt.Printf("reflect %%T: %T\n", d)

	v := reflect.ValueOf(d)
	fmt.Println("reflect len:", v.Len(), v.MapIndex(reflect.ValueOf(1)).Len())

	v.SetMapIndex(reflect.ValueOf(3), reflect.ValueOf(Direct{}))
	fmt.Println("reflect set:", len(d), d[3] != nil)

	n := 0
	for iter := v.MapRange(); iter.Next(); {
		n++
	}
	fmt.Println("reflect range:", n)

	fmt.Println("reflect deepequal:", reflect.DeepEqual(Direct{1: Direct{2: nil}}, Direct{1: Direct{2: nil}}), reflect.DeepEqual(Direct{1: nil}, Direct{2: nil}))

	s := DirectStr{"a": DirectStr{"b": nil}}
	fmt.Printf("fmt: %v %d\n", s, len(s))

	ds := DefinedOverStruct{n: 7, m: ViaStruct{1: {}}}
	fv := reflect.ValueOf(ds)
	fmt.Println("reflect fields:", fv.NumField(), fv.Field(0).Int(), fv.Field(1).Len())

	var st fmt.Stringer = d
	back, ok := st.(Direct)
	fmt.Println("adapter:", st.String(), ok, len(back))

	p := PDirect(&d)
	runtime.SetFinalizer(p, func(PDirect) {})
	runtime.SetFinalizer(p, nil)
	fmt.Println("finalizer:", p != nil, len(d))
}

func main() {
	direct()
	directStr()
	viaStruct()
	viaNested()
	viaArray()
	viaSlice()
	viaMap()
	viaNamed()
	generic()
	viaPtr()
	reflection()
}
