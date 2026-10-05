// Function-local named pointer conversion guard (testify's assertions_test.go: `type sPtr *s;
// dps := sPtr(ps)`).
//
// A function-local type is emitted under a LIFTED name (`topLevel_sPtr`), and a conversion to a
// local named POINTER from a non-nil pointer is rendered as a constructor call. The constructor
// named the Go spelling `sPtr`, a type that does not exist in C# (CS0246). A conversion from nil
// took a different path and was already correct; a package-level named pointer compiled because its
// lifted name and its Go name are the same.
package main

import "fmt"

type item struct{ n int }

// CONTROL: a package-level named pointer, whose emission must not change.
type itemPtr *item

func topLevel() {
	type s struct{ i int }
	type sPtr *s

	ps := &s{1}
	dps := sPtr(ps)
	fmt.Println("top level:", (*dps).i)
}

func inClosure() {
	run := func() {
		type s struct{ i int }
		type sPtr *s

		ps := &s{2}
		dps := sPtr(ps)
		(*dps).i = 3
		fmt.Println("in closure:", ps.i)
	}

	run()
}

func fromNil() {
	type s struct{ i int }
	type sPtr *s

	var dps = sPtr(nil)
	fmt.Println("from nil:", dps == nil)
}

func main() {
	topLevel()
	inClosure()
	fromNil()

	p := itemPtr(&item{4})
	fmt.Println("package level:", (*p).n)
}
