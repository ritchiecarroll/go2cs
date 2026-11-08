// A method receiver named like an imported package the method's signature also names,
// `func (vfs *Common) Stat(path string) vfs.FileInfo` (the TypeScript compiler's vfs). The
// receiver shadows the package inside the body, so the converter renames the receiver, and the
// declaration must carry the same name as its uses.
package main

import (
	"fmt"
	"io"
	"strings"
)

type Text struct{ s string }

func newReader(s string) *strings.Reader { return strings.NewReader(s) }

// A pointer receiver; the package appears in the result type.
func (strings *Text) Reader() *strings.Reader { return newReader(strings.s) }

// A value receiver.
func (strings Text) Upper() *strings.Reader { return newReader(toUpper(strings.s)) }

// The package in a parameter type.
func (strings *Text) Prefix(r *strings.Reader, n int) string {
	buf := make([]byte, n)
	k, _ := r.Read(buf)
	return strings.s + ":" + string(buf[:k])
}

// A pointer receiver with a pointer parameter (the signature is rebuilt for the parameter's alias).
func (strings *Text) CopyTo(dst *Text) *strings.Reader {
	dst.s = strings.s + "!"
	return newReader(dst.s)
}

// The receiver's address is taken (it moves to the heap).
func (strings Text) Addr() (*Text, *strings.Reader) {
	p := &strings
	p.s += "?"
	return p, newReader(strings.s)
}

// Same receiver name, the package unused: no rename is needed.
func (strings *Text) Len() int { return len(strings.s) }

func toUpper(s string) string { return strings.ToUpper(s) }

func readAll(r io.Reader) string {
	b, _ := io.ReadAll(r)
	return string(b)
}

func main() {
	t := &Text{s: "abc"}
	fmt.Println("reader:", readAll(t.Reader()))
	fmt.Println("upper:", readAll(t.Upper()))
	fmt.Println("prefix:", t.Prefix(strings.NewReader("xyz"), 2))

	var dst Text
	fmt.Println("copy:", readAll(t.CopyTo(&dst)), dst.s)

	p, r := t.Addr()
	fmt.Println("addr:", p.s, readAll(r), t.s)
	fmt.Println("len:", t.Len())
}
