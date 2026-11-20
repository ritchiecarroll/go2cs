// ReflectArrayStoreCopies: every reflect store that puts an array value into a second location must store
// a COPY. Go copies the array on SetMapIndex, Append, Send, Set into an interface slot and Interface(); a
// later in-place write to the source (SetZero, Set, Index(i).Set) must not reach the stored value.
// encoding/gob's decodeMap and encoding/json's object decoder reuse one reflect.New element for every
// map entry and zero it between entries, so a shared store loses every decoded element (gob's
// TestEndToEnd, its Marr field).
package main

import (
	"bytes"
	"encoding/gob"
	"encoding/json"
	"fmt"
	"reflect"
)

type holder struct {
	A [2]int
}

// marr is TestEndToEnd's T1.Marr as a struct field, the shape gob decodes there.
type marr struct {
	Marr map[[2]string][2]*float64
}

func main() {
	setMapIndexThenSetZero()
	setMapIndexThenSet()
	setMapIndexStruct()
	appendThenSetZero()
	sendThenSetZero()
	interfaceSlotThenSetZero()
	interfaceThenSetZero()
	gobRoundTrip()
	jsonMapOfArrays()
}

// The decodeMap shape: one element buffer, zeroed after each SetMapIndex.
func setMapIndexThenSetZero() {
	m := map[[2]string][2]*float64{}
	mv := reflect.ValueOf(m)
	keyP := reflect.New(reflect.TypeOf([2]string{}))
	elemP := reflect.New(reflect.TypeOf([2]*float64{}))
	a, b := 1.5, 2.5
	keyP.Elem().Index(0).SetString("x")
	keyP.Elem().Index(1).SetString("y")
	elemP.Elem().Index(0).Set(reflect.ValueOf(&a))
	elemP.Elem().Index(1).Set(reflect.ValueOf(&b))
	mv.SetMapIndex(keyP.Elem(), elemP.Elem())
	keyP.Elem().SetZero()
	elemP.Elem().SetZero()
	v := m[[2]string{"x", "y"}]
	fmt.Println("SetMapIndex then SetZero:", len(m), v[0] != nil && *v[0] == 1.5, v[1] != nil && *v[1] == 2.5)
}

func setMapIndexThenSet() {
	m := map[string][2]int{}
	mv := reflect.ValueOf(m)
	elemP := reflect.New(reflect.TypeOf([2]int{}))
	elemP.Elem().Index(0).SetInt(7)
	elemP.Elem().Index(1).SetInt(8)
	mv.SetMapIndex(reflect.ValueOf("k"), elemP.Elem())
	elemP.Elem().Set(reflect.ValueOf([2]int{1, 2}))
	fmt.Println("SetMapIndex then Set:", m["k"])
}

func setMapIndexStruct() {
	m := map[string]holder{}
	mv := reflect.ValueOf(m)
	elemP := reflect.New(reflect.TypeOf(holder{}))
	elemP.Elem().Field(0).Index(1).SetInt(9)
	mv.SetMapIndex(reflect.ValueOf("k"), elemP.Elem())
	elemP.Elem().SetZero()
	fmt.Println("SetMapIndex struct then SetZero:", m["k"])
}

func appendThenSetZero() {
	src := reflect.New(reflect.TypeOf([2]int{}))
	src.Elem().Index(0).SetInt(3)
	s := reflect.Append(reflect.ValueOf([][2]int{}), src.Elem())
	src.Elem().SetZero()
	fmt.Println("Append then SetZero:", s.Interface())
}

func sendThenSetZero() {
	ch := make(chan [2]int, 1)
	src := reflect.New(reflect.TypeOf([2]int{}))
	src.Elem().Index(0).SetInt(4)
	reflect.ValueOf(ch).Send(src.Elem())
	src.Elem().SetZero()
	fmt.Println("Send then SetZero:", <-ch)
}

func interfaceSlotThenSetZero() {
	var x any
	src := reflect.New(reflect.TypeOf([2]int{}))
	src.Elem().Index(0).SetInt(5)
	reflect.ValueOf(&x).Elem().Set(src.Elem())
	src.Elem().SetZero()
	fmt.Println("Set into interface then SetZero:", x)
}

func interfaceThenSetZero() {
	src := reflect.New(reflect.TypeOf([2]int{}))
	src.Elem().Index(0).SetInt(6)
	got := src.Elem().Interface()
	src.Elem().SetZero()
	fmt.Println("Interface then SetZero:", got)
}

// TestEndToEnd's Marr, alone: a map whose key and value are arrays, round-tripped through gob.
func gobRoundTrip() {
	e, pi := 2.71828, 3.14159
	in := marr{Marr: map[[2]string][2]*float64{
		{"string1", "string2"}: {&pi, &e},
		{"string2", "string1"}: {&e, &pi},
	}}
	var buf bytes.Buffer
	if err := gob.NewEncoder(&buf).Encode(in); err != nil {
		fmt.Println("encode:", err)
		return
	}
	var out marr
	if err := gob.NewDecoder(&buf).Decode(&out); err != nil {
		fmt.Println("decode:", err)
		return
	}
	for _, k := range [][2]string{{"string1", "string2"}, {"string2", "string1"}} {
		v := out.Marr[k]
		fmt.Println("gob", k, v[0] != nil && *v[0] == *in.Marr[k][0], v[1] != nil && *v[1] == *in.Marr[k][1])
	}
}

// encoding/json's object decoder reuses one map element (mapElem) and zeroes it before each entry.
// The map is a struct field, as Marr is, so the element type carries its length.
func jsonMapOfArrays() {
	var v struct{ M map[string][2]int }
	if err := json.Unmarshal([]byte(`{"M":{"a":[1,2],"b":[3,4]}}`), &v); err != nil {
		fmt.Println("json:", err)
		return
	}
	fmt.Println("json", v.M["a"], v.M["b"])
}
