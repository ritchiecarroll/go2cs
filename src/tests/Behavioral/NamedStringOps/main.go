// NamedStringOps exercises every string operation over a named (defined) string type: the cmp.Ordered generics
// (slices.Sort/Max/Min/BinarySearch, cmp.Compare, a user Ordered function, the builtin min/max), ordered and
// equality comparisons, concatenation, len, indexing, sub-slicing, conversions to and from []byte and []rune,
// range in both forms (including invalid UTF-8), switch, and use as a map key. TS readiness rows C6 and C12.
package main

import (
	"cmp"
	"fmt"
	"slices"
	"strings"
)

type Path string

type Tag string

// maxOf is a user function over cmp.Ordered, instantiated with a named string.
func maxOf[T cmp.Ordered](xs ...T) T {
	m := xs[0]
	for _, x := range xs[1:] {
		if x > m {
			m = x
		}
	}
	return m
}

func describe(t Tag) string {
	switch t {
	case "alpha":
		return "first"
	case "omega":
		return "last"
	}
	return "other"
}

func main() {
	// cmp.Ordered generics (C6).
	ps := []Path{"b/c", "a/z", "c", "a/b"}
	slices.Sort(ps)
	fmt.Println(ps)
	fmt.Println(slices.Max(ps), slices.Min(ps))
	i, found := slices.BinarySearch(ps, Path("b/c"))
	fmt.Println(i, found)
	fmt.Println(cmp.Compare(Path("a"), Path("b")), cmp.Compare(Path("b"), Path("a")), cmp.Compare(Path("a"), Path("a")))
	fmt.Println(cmp.Less(Path("a"), Path("b")), maxOf[Path]("m", "x", "c"))
	fmt.Println(min(Path("q"), Path("p")), max(Path("q"), Path("p"), Path("r")))
	fmt.Println(slices.Index(ps, "c"), slices.Contains(ps, "a/z"))

	// Comparisons and concatenation.
	a, b := Path("apple"), Path("banana")
	fmt.Println(a < b, a <= b, a > b, a >= b, a == b, a != b, a < "b", "z" > b)
	joined := a + "/" + b
	fmt.Println(joined, len(joined))

	// Indexing and sub-slicing keep the named type.
	fmt.Println(joined[0], joined[1:4], joined[len(joined)-3:])

	// Conversions to and from []byte and []rune (C12).
	p := Path([]byte("/a/b"))
	q := Path([]rune{'x', 'é', 'y'})
	bs := []byte(q)
	rs := []rune(q)
	fmt.Println(p, q, len(q), bs, rs, string(p))

	// range, both forms, including invalid UTF-8 (C12).
	n := 0
	for i, r := range p {
		if r == '/' {
			n++
		}
		_ = i
	}
	fmt.Println(n)
	for i, r := range Path("a\xffé") {
		fmt.Print(i, ":", r, " ")
	}
	fmt.Println()
	count := 0
	for i := range q {
		count += i
	}
	fmt.Println(count)

	// switch, map key, and a strings round trip.
	fmt.Println(describe("alpha"), describe("omega"), describe("beta"))
	m := map[Tag]int{"x": 1, "y": 2}
	m["x"] += 10
	keys := make([]Tag, 0, len(m))
	for k := range m {
		keys = append(keys, k)
	}
	slices.Sort(keys)
	fmt.Println(keys, m["x"], m["y"])
	fmt.Println(Tag(strings.ToUpper(string(Tag("mixed")))))
}
