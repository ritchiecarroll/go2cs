package main

import (
	"fmt"
	"reflect"
)

// A nil pointer / func / map read out of a struct FIELD, then written into an interface-typed slot
// through each reflect write path. Go packs a typed nil: the dynamic type survives the store.
type Nested struct{ X int }

type Holder struct {
	Nested *Nested
	Fn     func()
	M      map[string]int
}

func take(v interface{}) string { return fmt.Sprintf("%#v", v) }

func main() {
	h := Holder{}
	hv := reflect.ValueOf(h)

	for i := 0; i < hv.NumField(); i++ {
		f := hv.Field(i)
		name := hv.Type().Field(i).Name

		// Set into an interface{} variable
		var dst interface{}
		reflect.ValueOf(&dst).Elem().Set(f)

		// SetMapIndex into map[string]interface{}
		m := map[string]interface{}{}
		reflect.ValueOf(m).SetMapIndex(reflect.ValueOf("k"), f)

		// Append to a []interface{}
		s := reflect.Append(reflect.ValueOf([]interface{}{}), f).Interface().([]interface{})

		// Call a func(interface{})
		out := reflect.ValueOf(take).Call([]reflect.Value{f})[0].Interface()

		// Send on a chan interface{}
		ch := make(chan interface{}, 1)
		reflect.ValueOf(ch).Send(f)
		got := <-ch

		fmt.Printf("%s: set=%T map=%T append=%T call=%v send=%T\n", name, dst, m["k"], s[0], out, got)
	}
}
