package main

// What C45's header views refuse, by name. A store through a slice read as a string or as a two-word string header
// moves the slice's data pointer and length in Go and keeps its capacity; the views are read-only, and the converter
// panics at the statement, where the store used to be lost with no word (Go prints 1 for len(c) after the first arm's
// store, the conversion printed 3). The near misses, a slice whose element is not a byte read as a string and a NAMED
// byte slice through either view, are served by neither view, and their reads panic by name too. A store through a
// pointer VARIABLE that holds the view is not refused at the statement: it panics by name on the next access through
// the variable, and is lost with no word when there is none (the "last access" arm prints the conversion's wrong 3).
// Each arm recovers and prints its panic up to the first parenthesis, and the last store is not recovered: the program
// ends on it.

import (
	"fmt"
	"strings"
	"unsafe"
)

type ptr = unsafe.Pointer

// zeebo/xxh3's header: a data pointer and an unsigned length.
type str struct {
	p ptr
	l uint
}

type bytes []byte

func try(name string, f func() string) {
	defer func() {
		if r := recover(); r != nil {
			text := fmt.Sprint(r)

			if i := strings.Index(text, " ("); i >= 0 {
				text = text[:i]
			}

			fmt.Printf("%s: panic: %s\n", name, text)
		}
	}()

	fmt.Printf("%s: %s\n", name, f())
}

func main() {
	try("string store", func() string {
		c := []byte("xyz")
		*(*string)(ptr(&c)) = "q"
		return fmt.Sprint(len(c))
	})
	try("string op-store", func() string {
		c := []byte("abcd")
		*(*string)(unsafe.Pointer(&c)) += "e"
		return fmt.Sprint(len(c))
	})
	try("header store", func() string {
		c := []byte("hello")
		*(*str)(ptr(&c)) = str{}
		return fmt.Sprint(len(c))
	})
	try("tuple store", func() string {
		c := []byte("xy")
		var s string
		*(*string)(ptr(&c)), s = "z", "w"
		return fmt.Sprint(len(c), s)
	})
	try("uint32 slice as string", func() string {
		w := []uint32{0x64636261}
		v := *(*string)(ptr(&w))
		return fmt.Sprint(len(v), v[0])
	})
	try("named slice as string", func() string {
		nb := bytes("named")
		return *(*string)(ptr(&nb))
	})
	try("named slice header", func() string {
		nb := bytes("named")
		h := *(*str)(ptr(&nb))
		return fmt.Sprint(h.l)
	})

	try("store through a variable, then read", func() string {
		c := []byte("xyz")
		p := (*string)(ptr(&c))
		*p = "q"
		return *p
	})
	// Silent today, and wrong: Go prints 1. The store lands in the view p holds, whose refusal runs on its next access,
	// and p is not read again. This arm pins the conversion's value until the BOARD row's cure.
	try("store through a variable, last access", func() string {
		c := []byte("xyz")
		p := (*string)(ptr(&c))
		*p = "q"
		return fmt.Sprint(len(c))
	})

	c := []byte("end")
	*(*string)(ptr(&c)) = "!"
	fmt.Println("not reached in the conversion:", len(c))
}
