// ReflectTypeAssertMarshal guards reflect.TypeAssert[T], new in Go 1.25 and called by Go 1.26's encoding/json,
// encoding/asn1, encoding/gob and encoding/xml on every marshal path. Go defines it as `v.Interface().(T)` and
// implements it by reinterpreting the Value's data word through unsafe, which the converted form cannot honor:
// json.Marshal of a Marshaler and asn1.Marshal of a struct panicked "dereference of a managed pointer with no
// address". Each shape TypeAssert answers is printed here: a concrete T, an interface T over a concrete value,
// an interface Value, a nil interface Value, an array T, and a mismatch.
package main

import (
	"encoding/asn1"
	"encoding/json"
	"fmt"
	"reflect"
)

type celsius float64

func (c celsius) MarshalJSON() ([]byte, error) {
	return []byte(fmt.Sprintf(`"%.1fC"`, float64(c))), nil
}

type reading struct {
	Where string
	Temp  celsius
}

type record struct {
	N    int
	Name string
}

type namer interface{ Name() string }

type thing struct{ s string }

func (t thing) Name() string { return t.s }

func main() {
	b, err := json.Marshal(reading{"lab", 21.5})
	fmt.Println(string(b), err)

	der, err := asn1.Marshal(record{7, "seven"})
	fmt.Printf("%x %v\n", der, err)

	var back record
	_, err = asn1.Unmarshal(der, &back)
	fmt.Println(back.N, back.Name, err)

	i, ok := reflect.TypeAssert[int](reflect.ValueOf(42))
	fmt.Println("concrete:", i, ok)

	n, ok := reflect.TypeAssert[namer](reflect.ValueOf(thing{"t"}))
	fmt.Println("interface over concrete:", n.Name(), ok)

	var held any = thing{"held"}
	h, ok := reflect.TypeAssert[thing](reflect.ValueOf(&held).Elem())
	fmt.Println("interface value:", h.s, ok)

	var none namer
	z, ok := reflect.TypeAssert[namer](reflect.ValueOf(&none).Elem())
	fmt.Println("nil interface value:", z == nil, ok)

	a, ok := reflect.TypeAssert[[2]int](reflect.ValueOf([2]int{3, 4}))
	fmt.Println("array:", a, ok)

	s, ok := reflect.TypeAssert[string](reflect.ValueOf(42))
	fmt.Printf("mismatch: %q %v\n", s, ok)
}
