package main

import "fmt"

// Stringish is a named constraint whose terms are approximate. Its type set is every type whose
// underlying type is []byte or string, the shape of clipperhouse/stringish.Interface.
type Stringish interface {
	~[]byte | ~string
}

// Iterator walks fixed-width pieces of a string or a byte slice, as uax29's iterator does.
type Iterator[T Stringish] struct {
	data       T
	start, pos int
}

func NewIterator[T Stringish](data T) *Iterator[T] {
	return &Iterator[T]{data: data}
}

func (it *Iterator[T]) Next(width int) bool {
	if it.pos >= len(it.data) {
		return false
	}
	it.start = it.pos
	it.pos += width
	if it.pos > len(it.data) {
		it.pos = len(it.data)
	}
	return true
}

// Value is a sub-slice of the data, so it has the type parameter's own type.
func (it *Iterator[T]) Value() T {
	return it.data[it.start:it.pos]
}

// count uses an inline approximate union in the other order.
func count[T ~string | ~[]byte](s T, b byte) int {
	n := 0
	for i := 0; i < len(s); i++ {
		if s[i] == b {
			n++
		}
	}
	return n
}

func head[T Stringish](s T, n int) T {
	if n > len(s) {
		n = len(s)
	}
	return s[:n]
}

func main() {
	it := NewIterator("hello, world")
	for it.Next(5) {
		fmt.Printf("%q\n", it.Value())
	}

	ib := NewIterator([]byte("abcdefg"))
	for ib.Next(3) {
		fmt.Printf("%q\n", ib.Value())
	}

	fmt.Println(count("banana", 'a'), count([]byte("banana"), 'n'))
	fmt.Printf("%q %q\n", head("gopher", 3), head([]byte("go"), 5))
}
