// gc's EARLY forms: builtins, some conversions, a type assertion, a slice expression, a map index by string(bytes),
// an && or || that holds a call, and a value boxed to an interface that is not addressable and is a bool, a one-byte
// integer, a struct, an array or &T{...}. gc runs each at its place among the statement's calls, while a plain operand
// is read after every call. Each row reads the package var g first, so the later call must move ahead of it, and the
// early form must move with it, in order.
package main

import "fmt"

type pt struct{ x, y int }

type holder struct{ got int }

var (
	g    int
	s    []int
	str  string
	bs   []byte
	m    map[string]int
	a    any
	flag bool
	cx   complex128
	h    *holder
	dst  = make([]int, 4)
)

func reset() {
	g, s, str, bs, m, a, flag, cx = 1, []int{1}, "a", []byte("k"), map[string]int{"k": 1}, 1, false, 1
	h = &holder{got: 1}
}

func mut() int {
	g, s, str, bs, m, a, flag, cx = 2, []int{1, 2}, "ab", []byte("j"), map[string]int{"k": 2, "j": 2}, 2, true, 2
	h.got = 2
	return 0
}

func ints(a, b, c int) string            { return fmt.Sprint(a, " ", b, " ", c) }
func slices(a int, b []int, c int) string { return fmt.Sprint(a, " ", b, " ", c) }
func bytes(a int, b []byte, c int) string { return fmt.Sprint(a, " ", b, " ", c) }
func runes(a int, b []rune, c int) string { return fmt.Sprint(a, " ", b, " ", c) }
func floats(a int, b float64, c int) string {
	return fmt.Sprint(a, " ", b, " ", c)
}
func complexes(a int, b complex128, c int) string { return fmt.Sprint(a, " ", b, " ", c) }

func say(text string) int {
	fmt.Print(text, " ")
	return 1
}

func main() {
	reset()
	fmt.Println("len:", ints(g, len(s), mut()))
	reset()
	fmt.Println("cap:", ints(g, cap(s), mut()))
	reset()
	fmt.Println("append:", slices(g, append([]int{}, s...), mut()))
	reset()
	fmt.Println("min:", ints(g, min(g, 5), mut()))
	reset()
	fmt.Println("max:", ints(g, max(g, 0), mut()))
	reset()
	fmt.Println("make:", slices(g, make([]int, g), mut()))
	reset()
	fmt.Println("real:", floats(g, real(cx), mut()))
	reset()
	fmt.Println("imag:", floats(g, imag(cx*1i), mut()))
	reset()
	fmt.Println("complex:", complexes(g, complex(float64(g), 0), mut()))
	reset()
	fmt.Println("copy:", ints(g, copy(dst, s), mut()))
	reset()
	fmt.Println("[]byte:", bytes(g, []byte(str), mut()))
	reset()
	fmt.Println("[]rune:", runes(g, []rune(str), mut()))
	reset()
	fmt.Println("type assertion:", ints(g, a.(int), mut()))
	reset()
	fmt.Println("slice expression:", slices(g, s[0:], mut()))
	reset()
	fmt.Println("map by bytes:", ints(g, m[string(bs)], mut()))
	reset()
	fmt.Println("boxed bool:", g, g > 1, mut())
	reset()
	fmt.Println("boxed not:", g, !flag, mut())
	reset()
	fmt.Println("boxed byte:", g, uint8(g), mut())
	reset()
	fmt.Println("boxed struct:", g, pt{g, 0}, mut())
	reset()
	fmt.Println("boxed array:", g, [2]int{g, 0}, mut())
	reset()
	fmt.Println("boxed pointer literal:", g, &pt{g, 0}, mut())
	reset()
	fmt.Println("boxed int stays late:", g, g+0, mut())
	reset()
	fmt.Println("printf comparison:", fmt.Sprintf("%v %d", h.got == 1, mut()))
	reset()
	fmt.Println("&& unit:", fmt.Sprint(g, say("L") == 0 || say("R") == 1, say("after")))
	reset()
	fmt.Println("&& without a call:", g == 1 && !flag, mut())
}
