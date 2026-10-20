// A generic call whose type argument Go infers from an UNTYPED CONSTANT gets the constant's
// default type (int, float64, rune, complex128, string, bool), which C# inference does not.
package main

import "fmt"

const named = 7

func show[E any](e E) string { return fmt.Sprintf("%T %v", e, e) }

func pair[E any](a, b E) string { return fmt.Sprintf("%T", b) }

func many[E any](es ...E) string { return fmt.Sprintf("%T %d", es, len(es)) }

func keyed[K comparable, V any](k K, v V) string { return fmt.Sprintf("%T %T", k, v) }

func main() {
	fmt.Println(show(3))
	fmt.Println(show(2.5))
	fmt.Println(show('a'))
	fmt.Println(show(1 + 2i))
	fmt.Println(show(named))
	fmt.Println(show(1 << 10))
	fmt.Println(show("s"))
	fmt.Println(show(true))
	fmt.Println(many(1, 2, 3))
	fmt.Println(keyed("k", 4))

	// CONTROLS: a typed argument decides the type argument.
	var x int8 = 1
	fmt.Println(pair(x, 3))
	fmt.Println(show(x))
	fmt.Println(show(int16(5)))
}
