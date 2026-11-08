// A Go type NAMED Value: go2cs's named wrappers expose their underlying value through a property
// called Value, and a C# member may not share its type's name (CS0542). `type Value []byte` is
// go-json-experiment/json's jsontext.Value, which TypeScript's compiler depends on.
package main

import (
	"fmt"

	"NamedValueTypeClash/arrval"
	"NamedValueTypeClash/mapval"
	"NamedValueTypeClash/numval"
)

type Value []byte

func (v Value) Len() int { return len(v) }

func (v Value) At(i int) byte { return v[i] }

func (v *Value) Add(b byte) { *v = append(*v, b) }

func (v *Value) Set(i int, b byte) { (*v)[i] = b }

func main() {
	v := Value("abc")
	v.Add('d')
	v.Set(0, 'A')
	fmt.Println(v.Len(), string(v), v.At(3), string(v[1:3]))

	n := numval.Value(3)
	n.Inc()
	fmt.Println(n, n.Double(), numval.Conv(numval.Other(4)), numval.Back(6))

	a := arrval.Value{1, 2, 3}
	a.Set(0, 5)
	a.Bump()
	fmt.Println(a, a.First(), a.Sum(), a.Head())

	m := mapval.Value{"a": 1}
	m.Put("b", 2)
	fmt.Println(m.Count(), m.Get("b"), m.Get("z"))
}
