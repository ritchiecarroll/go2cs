// Two dot-imported packages and the main package each declare the same synthesized pointer adapters:
// `bytes_BufferжWriter` (each converts a *bytes.Buffer to io.Writer) and `sinkжWriter` (each converts its own
// *sink). A method's body stands in a holder class, outside the package class, where `using static` brings all
// three packages' adapters with equal standing. gopkg.in/yaml.v3's tests are the instance (`. "gopkg.in/check.v1"`,
// CS0104 on `bytes_BufferжWriter`).
package main

import (
	"bytes"
	"fmt"
	"io"
	"strings"

	. "DotImportAdapterNameInHolder/a/sinks"
	. "DotImportAdapterNameInHolder/b/sinks"
)

type sink struct{ lines []string }

func (s *sink) Write(p []byte) (int, error) {
	s.lines = append(s.lines, string(p))
	return len(p), nil
}

type report struct{ name string }

// A method named like the package's function plain: a program's methods stay in its package class, unless one would
// hide a package-level name there, so this one moves every method of report into a holder class (methodHolder.go).
func (r *report) plain() string { return r.name + ":method" }

// In a method: the casts the holder must name through the main package.
func (r *report) buffer() string {
	var buf bytes.Buffer
	var w io.Writer = &buf
	fmt.Fprintf(w, "%s:buffer", r.name)
	return buf.String()
}

func (r *report) count() int {
	s := &sink{}
	var w io.Writer = s
	fmt.Fprint(w, r.name, "|", "sink")
	return len(s.lines)
}

// A control in the holder: only the main package adapts *strings.Builder to io.Writer.
func (r *report) builder() string {
	var sb strings.Builder
	var w io.Writer = &sb
	fmt.Fprintf(w, "%s:builder", r.name)
	return sb.String()
}

// In a plain function, which stands in the package class: the bare name already binds the package's own adapter.
func plain() string {
	var buf bytes.Buffer
	var w io.Writer = &buf
	fmt.Fprint(w, "plain")
	return buf.String()
}

func main() {
	r := &report{name: "r"}
	fmt.Println(r.buffer(), r.count(), r.builder(), r.plain(), plain())

	w, buf := NewWriter()
	fmt.Fprint(w, "a")
	cw, counted := Counted()
	fmt.Fprint(cw, "abc")
	fmt.Println(buf.String(), counted())

	var other bytes.Buffer
	ww := Wrap(&other)
	fmt.Fprint(ww, "b")
	dw, doubled := Doubled()
	fmt.Fprint(dw, "abc")
	fmt.Println(other.String(), doubled())
}
