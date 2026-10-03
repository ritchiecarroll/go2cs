package main

import (
	"fmt"
	"reflect"
	"sort"
)

// reflect.Value.SetMapIndex with a NIL interface key (yaml.v3's gocheck panics), and a typed-nil
// POINTER key into an interface-keyed map, which Go keeps as (type=*T, value=nil).

type T struct{ n int }

type holder struct{ P *T }

func show(label string, m map[interface{}]interface{}) {
	var keys []string
	for k, v := range m {
		keys = append(keys, fmt.Sprintf("%T(%v)=%v", k, k, v))
	}
	sort.Strings(keys)
	fmt.Println(label, len(m), keys)
}

func main() {
	m := map[interface{}]interface{}{}
	v := reflect.ValueOf(m)
	keyType := v.Type().Key()

	// A nil interface key.
	v.SetMapIndex(reflect.Zero(keyType), reflect.ValueOf("nil-key"))
	show("after nil key:", m)
	fmt.Println("MapIndex(nil):", v.MapIndex(reflect.Zero(keyType)))
	fmt.Println("m[nil]:", m[nil])

	// A typed-nil pointer key, taken from a struct field (its Value is typed, its datum nil).
	h := holder{}
	pk := reflect.ValueOf(h).Field(0)
	v.SetMapIndex(pk, reflect.ValueOf("typed-nil-key"))
	show("after typed-nil key:", m)
	fmt.Println("m[(*T)(nil)]:", m[(*T)(nil)])
	fmt.Println("m[nil] still:", m[nil])

	// Overwrite and delete the nil key.
	v.SetMapIndex(reflect.Zero(keyType), reflect.ValueOf("again"))
	fmt.Println("m[nil] after overwrite:", m[nil])
	v.SetMapIndex(reflect.Zero(keyType), reflect.Value{})
	show("after delete nil key:", m)

	// Read back through reflect.
	keys := v.MapKeys()
	fmt.Println("MapKeys:", len(keys))
	for _, k := range keys {
		fmt.Println(" key kind:", k.Kind(), "elem:", k.Elem().Type(), "elem nil:", k.Elem().IsNil())
	}
}
