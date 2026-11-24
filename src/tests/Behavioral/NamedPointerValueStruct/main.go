// R7 seat 2 (P3's SHOULD 1): a named POINTER type named Value, over a STRUCT. Its wrapper declares the pointee read as
// Value‿ (a member may not share its type's name), so a field the wrapper cannot forward as a property -- one named like
// a wrapper member, NilInstance or IsNull -- is read and written through Value‿, never through Value.
package main

import "fmt"

type point struct {
	X           int
	NilInstance int
	IsNull      string
}

type Value *point

// set writes the fields through a Value parameter.
func set(v Value, n int) {
	v.X = n
	v.NilInstance = n * 10
	v.IsNull = fmt.Sprint("not ", n)
}

func main() {
	p := point{1, 2, "three"}
	var v Value = &p
	fmt.Println("read:", v.X, v.NilInstance, v.IsNull)
	v.NilInstance = 20
	v.IsNull = "changed"
	fmt.Println("written:", p.NilInstance, p.IsNull)
	set(v, 4)
	fmt.Println("through a parameter:", p, (*v).X, v != nil)
	px := &v.NilInstance
	*px = 99
	fmt.Println("field address:", p.NilInstance)
}
