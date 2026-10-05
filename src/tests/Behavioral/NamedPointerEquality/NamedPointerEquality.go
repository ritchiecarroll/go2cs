// Equality on a value of a NAMED POINTER type (`type itemPtr *item`).
//
// `ip == itemPtr(pi)` compares two pointers. The converter rendered the left operand in pointer
// context as `Ꮡip`, the box of a value local, which does not exist (CS0103). A named pointer is already
// a pointer value; both operands must compare as written, with Go's result.
package main

import "fmt"

type item struct{ n int }

type itemPtr *item

func main() {
	pi := &item{2}
	ip := itemPtr(pi)
	other := itemPtr(&item{2})
	fmt.Println(ip == itemPtr(pi), ip != other, ip == nil, ip != itemPtr(pi), ip == ip)
}
