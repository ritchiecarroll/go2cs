package main

import (
	"fmt"
	"reflect"
	"strings"
)

type pair struct {
	A string
	B int
}

func main() {
	// mapstructure's decodeStructFromMap: the keys of a map[string]any, held in a
	// map[reflect.Value]struct{} and scanned case-insensitively. Each key is its own Value.
	m := map[string]interface{}{"vstring": "foo", "Vint": 42, "vbool": true}
	dataVal := reflect.ValueOf(m)
	keys := make(map[reflect.Value]struct{})
	for _, k := range dataVal.MapKeys() {
		keys[k] = struct{}{}
	}
	fmt.Println("distinct keys:", len(keys))

	matched := 0
	for k := range keys {
		for _, field := range []string{"Vstring", "Vint", "Vbool"} {
			if strings.EqualFold(k.Interface().(string), field) {
				matched++
			}
		}
	}
	fmt.Println("matched fields:", matched)

	// Two Values of one type over different run-time values never compare equal.
	a, b := strings.Repeat("a", 2), strings.Repeat("b", 2)
	fmt.Println("different strings equal:", reflect.ValueOf(a) == reflect.ValueOf(b))
	s1, s2 := pair{a, 1}, pair{b, 2}
	fmt.Println("different structs equal:", reflect.ValueOf(s1) == reflect.ValueOf(s2))

	// A copy of a Value is the same Value.
	v := reflect.ValueOf(a)
	w := v
	fmt.Println("copy equal:", v == w)

	// Pointer-shaped kinds: Values of the same pointer, map or func are equal.
	p := &s1
	fmt.Println("same pointer equal:", reflect.ValueOf(p) == reflect.ValueOf(p))
	q := &s2
	fmt.Println("different pointers equal:", reflect.ValueOf(p) == reflect.ValueOf(q))
	fmt.Println("same map equal:", reflect.ValueOf(m) == reflect.ValueOf(m))
	other := map[string]interface{}{}
	fmt.Println("different maps equal:", reflect.ValueOf(m) == reflect.ValueOf(other))

	// The zero Value equals itself.
	fmt.Println("zero equal:", reflect.Value{} == reflect.Value{})
}
