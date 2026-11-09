// NamedBytesByteSeq exercises a named []byte and a named string passed to a `~[]byte | ~string` type parameter:
// len, indexing, sub-slicing, spread, and the four conversions out of the type parameter ([]byte, string, the named
// []byte, the named string), with Go's sharing rules asserted by writing through each result: []byte(b) and Val(b)
// SHARE a byte-slice argument's backing and COPY a string argument's bytes. A named []byte declaring its own Go
// method Clone keeps that method. TS readiness row C16 (jsontext's `type Value []byte` meets `~[]byte | ~string`).
package main

import "fmt"

type Val []byte

type Ptr string

// Clone is a Go method on the named []byte, as jsontext's Value has.
func (v Val) Clone() Val {
	return append(Val(nil), v...)
}

func prefix[Bytes ~[]byte | ~string](b Bytes, n int) string {
	return string(b[:n])
}

func sum[Bytes ~[]byte | ~string](b Bytes) int {
	t := 0
	for i := 0; i < len(b); i++ {
		t += int(b[i])
	}
	return t
}

func spread[Bytes ~[]byte | ~string](dst []byte, b Bytes) []byte {
	return append(dst, b[1:]...)
}

// toBytes is []byte(b): it shares a byte-slice argument's backing in Go, and copies a string's bytes.
func toBytes[Bytes ~[]byte | ~string](b Bytes) []byte {
	return []byte(b)
}

func toVal[Bytes ~[]byte | ~string](b Bytes) Val {
	return Val(b)
}

func toPtr[Bytes ~[]byte | ~string](b Bytes) Ptr {
	return Ptr(b)
}

func main() {
	v := Val("abcd")
	p := Ptr("wxyz")

	fmt.Println(prefix(v, 2), prefix(v[1:], 2), prefix(p, 3), prefix("xyz", 1))
	fmt.Println(sum(v), sum(p), sum(v[2:]), sum([]byte("ab")))
	fmt.Println(string(spread([]byte("<"), v)), string(spread([]byte("<"), p)))

	// []byte(b) over a named []byte SHARES: the write shows through v.
	b := toBytes(v)
	b[0] = 'A'
	fmt.Println(string(v), string(b))

	// ...over a sub-slice of it, too, at the sub-slice's offset.
	b = toBytes(v[2:])
	b[0] = 'C'
	fmt.Println(string(v), string(b))

	// ...and over a named string it COPIES: p is unchanged.
	b = toBytes(p)
	b[0] = 'W'
	fmt.Println(string(p), string(b))

	// Val(b) shares a named []byte argument and copies a string one.
	w := toVal(v)
	w[1] = 'B'
	fmt.Println(string(v), string(w))
	w = toVal(p)
	w[1] = 'X'
	fmt.Println(string(p), string(w))
	w = toVal("pq")
	fmt.Println(string(w), len(w))

	// Ptr(b) from either member.
	fmt.Println(toPtr(v), toPtr(p), toPtr("lit"), toPtr(v[1:3]) == "BC", len(toPtr(p[1:])))

	// The named []byte's own Go method Clone: a copy, not shared.
	c := v.Clone()
	c[0] = 'z'
	fmt.Println(string(v), string(c))
}
