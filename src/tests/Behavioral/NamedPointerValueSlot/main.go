// A dereference whose result is a reference-like value (a map, a slice, a pointer) reads through
// the box's nil-check-free ValueSlot. A NAMED pointer type is a generated wrapper over that box,
// so it must expose the same slot: each function below dereferences through one.
package main

import "fmt"

type M map[int]int

// P is a named pointer to a named map.
type P *M

// PM is a named pointer to an unnamed map.
type PM *map[string]int

// PS is a named pointer to a slice.
type PS *[]int

// PP is a named pointer to a pointer.
type PP **int

func namedMap() {
	m := M{1: 1, 2: 2}
	p := P(&m)
	(*p)[3] = 3
	fmt.Println("named map:", len(*p), (*p)[3], len(m))
}

func unnamedMap() {
	m := map[string]int{"a": 1}
	p := PM(&m)
	(*p)["b"] = 2
	fmt.Println("unnamed map:", len(*p), (*p)["b"], len(m))
}

func slice() {
	s := []int{1, 2, 3}
	p := PS(&s)
	*p = append(*p, 4)
	fmt.Println("slice:", len(*p), (*p)[3], len(s))
}

func pointer() {
	var ip *int
	pp := PP(&ip)
	fmt.Println("pointer nil:", *pp == nil)
	v := 7
	*pp = &v
	fmt.Println("pointer set:", **pp, *ip)
}

func main() {
	namedMap()
	unnamedMap()
	slice()
	pointer()
}
