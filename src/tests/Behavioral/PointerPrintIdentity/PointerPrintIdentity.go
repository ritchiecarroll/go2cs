// Go prints one number for one pointer: `%p` of p, `unsafe.Pointer(p)` and `uintptr(unsafe.Pointer(p))` all
// answer the same address, in either order, and reflect's Pointer agrees. Only booleans are printed, so the
// output is the same on every run.
package main

import (
	"fmt"
	"reflect"
	"unsafe"
)

type pair struct {
	n int
	s string
}

func check(name string, p any, up unsafe.Pointer) {
	viaP := fmt.Sprintf("%p", p)
	viaV := fmt.Sprintf("%v", up)
	viaX := fmt.Sprintf("%#x", uintptr(up))
	viaR := reflect.ValueOf(p).Pointer() == uintptr(up)
	fmt.Println(name, viaP == viaV, viaP == viaX, viaR)
}

func main() {
	// The BOARD's repro order: %p first, then the address taken.
	i := 1
	first := fmt.Sprintf("%p", &i)
	fmt.Println("int, %p first:", first == fmt.Sprintf("%v", unsafe.Pointer(&i)))
	fmt.Println("int, %p again:", first == fmt.Sprintf("%p", &i))

	// The address taken first, then %p.
	j := 2
	taken := fmt.Sprintf("%v", unsafe.Pointer(&j))
	fmt.Println("int, address first:", taken == fmt.Sprintf("%p", &j))

	k := 3
	check("int local:", &k, unsafe.Pointer(&k))

	h := new(int64)
	check("new(int64):", h, unsafe.Pointer(h))

	var pr pair
	check("reference-free field:", &pr.n, unsafe.Pointer(&pr.n))
	check("reference-bearing field:", &pr.s, unsafe.Pointer(&pr.s))

	xs := []int32{1, 2, 3}
	check("slice element:", &xs[1], unsafe.Pointer(&xs[1]))

	var arr [4]byte
	check("array:", &arr, unsafe.Pointer(&arr))

	str := "go"
	check("string local:", &str, unsafe.Pointer(&str))

	ps := &pair{n: 1, s: "x"}
	check("struct with a string:", ps, unsafe.Pointer(ps))

	// Two pointers to one variable print one address; two variables print two.
	a, b := 1, 2
	pa1, pa2 := &a, &a
	fmt.Println("one variable, one %p:", fmt.Sprintf("%p", pa1) == fmt.Sprintf("%p", pa2))
	fmt.Println("two variables, two %p:", fmt.Sprintf("%p", &a) != fmt.Sprintf("%p", &b))
}
