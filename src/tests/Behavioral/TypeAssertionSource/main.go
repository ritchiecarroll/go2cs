// TypeAssertionSource guards that a failed single-value type assertion names the STATIC interface the
// value was held as, as Go does: "interface conversion: io.Reader is *strings.Reader, not *bytes.Buffer".
//
// The converted call takes one of three spellings by what the source interface can name:
//   - `x._<T>()` for an exported interface: go2cs-gen gives it an extension overload in the package's
//     top-level asserts class, which C# picks over golib's object form by identity conversion;
//   - `x._<I, T>()` for an unexported interface of this package, a private nested type no sibling class
//     can name, so it has no overload and binds golib's `_<TSource, T>`;
//   - `((object)x)._<T>()` where the bare call would be ambiguous (CS0121): an interface embedding two
//     others with no overload in scope. Nothing in this project reaches that shape; the converter's own
//     tests pin it.
package main

import (
	"bytes"
	"fmt"
	"io"
	"strings"

	"TypeAssertionSource/mid"
)

type namer interface{ name() string }

type a struct{}
type b struct{}

// pipe implements io.ReadWriter and io.ReadCloser, so asserting to it is possible but fails.
type pipe struct{}

func (*pipe) Read(p []byte) (int, error)  { return 0, io.EOF }
func (*pipe) Write(p []byte) (int, error) { return len(p), nil }
func (*pipe) Close() error                { return nil }

func (*a) name() string { return "a" }
func (*b) name() string { return "b" }

// catch runs f and prints what recover() saw: the value's dynamic type and its text.
func catch(what string, f func()) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Printf("%s: %T: %v\n", what, r, r)
		} else {
			fmt.Printf("%s: NO PANIC\n", what)
		}
	}()
	f()
}

func main() {
	// A named interface declared in this package.
	var n namer = &a{}
	catch("namer-to-*b", func() { _ = n.(*b) })

	// Named interfaces of another package, imported by this file.
	var r io.Reader = strings.NewReader("x")
	catch("io.Reader-to-*bytes.Buffer", func() { _ = r.(*bytes.Buffer) })

	var rw io.ReadWriter = &bytes.Buffer{}
	catch("io.ReadWriter-to-*pipe", func() { _ = rw.(*pipe) })

	// An interface held as a field type and passed on keeps its own name, not its parent's.
	var rc io.ReadCloser = io.NopCloser(strings.NewReader("x"))
	catch("io.ReadCloser-to-*pipe", func() { _ = rc.(*pipe) })

	// An anonymous interface literal.
	var anon interface{ name() string } = &a{}
	catch("anonymous-to-*b", func() { _ = anon.(*b) })

	// Controls: matching assertions do not panic.
	catch("namer-to-*a", func() { _ = n.(*a) })
	catch("io.Reader-to-*strings.Reader", func() { _ = r.(*strings.Reader) })

	// An exported interface of a package this file does not import, reached through another package's call.
	x := mid.Get()
	catch("lib.RW-to-*mid.Other", func() { _ = x.(*mid.Other) })
}
