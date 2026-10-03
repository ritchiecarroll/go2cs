// A string literal converts to a slice spelled with the ALIASED element name exactly as it does with the alias:
// []uint8 is []byte and []int32 is []rune, in every position (R's mapstructure reading, B5).

package main

import "fmt"

type buf []uint8

type B = []byte

func take(b []uint8) int { return len(b) }

var g = []uint8("glob")

func main() {
	a := []uint8("foo")
	b := []byte("bar")
	var c buf = buf("baz")
	r := []int32("héllo")
	s := []uint8("a" + "b")
	fmt.Println(len(a), len(b), len(c), take([]uint8("qux")), len(g), len(r), len(s), len(B("ali")))
	fmt.Println(string(a), string(g), string(s), string(r), r[1])
}
