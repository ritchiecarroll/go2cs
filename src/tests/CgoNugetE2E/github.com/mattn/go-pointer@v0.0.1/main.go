// The sample program for github.com/mattn/go-pointer v0.0.1, a cgo package: a Go value saved behind a C pointer,
// restored through it, then released. It never prints a pointer, whose value differs run to run.
package main

import (
	"fmt"

	pointer "github.com/mattn/go-pointer"
)

type point struct{ X, Y int }

func main() {
	p := pointer.Save(point{3, 4})
	fmt.Println("saved:", p != nil)
	fmt.Println("restored:", pointer.Restore(p))

	q := pointer.Save("second")
	fmt.Println("distinct:", p != q, pointer.Restore(q))

	pointer.Unref(p)
	fmt.Println("restored after Unref is nil:", pointer.Restore(p) == nil)
	fmt.Println("the other survives:", pointer.Restore(q))

	pointer.Unref(q)
	fmt.Println("Save(nil) is nil:", pointer.Save(nil) == nil, "Restore(nil) is nil:", pointer.Restore(nil) == nil)
}
