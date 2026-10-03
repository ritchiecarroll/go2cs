package main

import (
	"bytes"
	"fmt"
	"strings"
)

// A METHOD EXPRESSION and a METHOD VALUE of a method PROMOTED through an embedded struct of
// another package (go-cmp's shape: `type myType struct{ bytes.Buffer }`), next to the controls
// that must keep working: a promoted method value through a VALUE, and a non-promoted method
// expression and value.

type myType struct{ bytes.Buffer }

// A same-package embed, for the same two forms without a package crossing.
type inner struct{ n int }

func (i *inner) Add(d int) int { i.n += d; return i.n }
func (i inner) Get() int       { return i.n }

type outer struct{ inner }

type own struct{ s string }

func (o own) Say(p string) string { return p + o.s }

func main() {
	// Method expression of a promoted pointer-receiver method.
	write := (*myType).Write
	var m myType
	n, err := write(&m, []byte("expr"))
	fmt.Println("(*myType).Write:", n, err, m.String())

	// Method value of a promoted pointer-receiver method.
	m2 := &myType{}
	wv := m2.Write
	n, err = wv([]byte("value"))
	fmt.Println("(&myType{}).Write:", n, err, m2.String())

	// Method value through a VALUE of a promoted method (Len has a pointer receiver on Buffer, so
	// it needs an addressable value: use a variable).
	var m3 myType
	m3.WriteString("abc")
	lv := m3.Len
	fmt.Println("m3.Len:", lv())

	// Method expression of a promoted method from a same-package embed.
	add := (*outer).Add
	var o outer
	fmt.Println("(*outer).Add:", add(&o, 2), add(&o, 3))
	get := outer.Get
	fmt.Println("outer.Get:", get(o))
	gv := o.Get
	fmt.Println("o.Get:", gv())

	// Non-promoted controls.
	say := own.Say
	fmt.Println("own.Say:", say(own{"!"}, "hi"))
	sv := own{"?"}.Say
	fmt.Println("own{}.Say:", sv("why"))

	// A method expression of a promoted method passed as a func value.
	fmt.Println("as value:", strings.ToUpper(fmt.Sprint(func(f func(*myType, []byte) (int, error)) int {
		var t myType
		k, _ := f(&t, []byte("xyz"))
		return k + t.Len()
	}((*myType).Write))))
}
