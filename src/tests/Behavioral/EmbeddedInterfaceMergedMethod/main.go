// Regression test: an interface that embeds two interfaces which BOTH declare the same method.
// Go merges identical methods of embedded interfaces into one (Go 1.14). C# keeps each embed as
// a base interface with the method declared only there, so a lookup of the method on the derived
// interface finds two members and is ambiguous (CS0121). The calls the converter writes are cast
// to the interface go/types resolved, but four sites are not casts the converter can write: the
// generated adapter that forwards to a value of the derived interface when it is assigned to
// another interface (typescript-go internal/compiler: EmitHost passed as a narrower host), a call
// on a type parameter the derived interface constrains, a method value, and hand-written C#. So
// the derived interface redeclares the merged method once, `new`, which hides both inherited
// members; every implementer fills it with the method that fills them.
package main

import "fmt"

type Path string

type PathAlias = Path

type PrinterHost interface {
	Ref(p Path) string
}

type SpecifierHost interface {
	Ref(p Path) string
	Dir() string
}

// EmitHost reaches Ref through two declaring interfaces.
type EmitHost interface {
	PrinterHost
	SpecifierHost
	Blocked(file string) bool
}

type MayBeEmittedHost interface {
	Ref(p Path) string
}

// Control: Ref reaches shared through ONE declaring interface by two paths, so nothing merges.
type left interface{ PrinterHost }
type right interface{ PrinterHost }
type shared interface {
	left
	right
}

type Labeler interface {
	Ref(p Path) string
	Label() string
}

// Nested: EmitHost already merges Ref, and Labeler declares it again.
type LabeledHost interface {
	EmitHost
	Labeler
}

// The same method as Go sees it, spelled through an alias and another parameter name.
type AliasHost interface {
	Ref(q PathAlias) string
	Root() string
}

type AliasedHost interface {
	PrinterHost
	AliasHost
}

// Control: the interface declares the method itself, so there is nothing left to merge.
type OwnHost interface {
	PrinterHost
	SpecifierHost
	Ref(p Path) string
}

// Big is assigned to EmitHost: the adapter targets the merging interface.
type Big interface {
	Ref(p Path) string
	Dir() string
	Blocked(file string) bool
	Extra() int
}

type host struct{}

func (host) Ref(p Path) string        { return "ref:" + string(p) }
func (host) Dir() string              { return "/src" }
func (host) Blocked(file string) bool { return file == "x" }
func (host) Label() string            { return "L" }
func (host) Extra() int               { return 7 }
func (host) Root() string             { return "/" }

type phost struct{ n int }

func (h *phost) Ref(p Path) string        { h.n++; return fmt.Sprint("pref:", p, h.n) }
func (h *phost) Dir() string              { return "/ptr" }
func (h *phost) Blocked(file string) bool { return true }

func emit(h MayBeEmittedHost, p Path) string { return h.Ref(p) }

func viaEmit(h EmitHost, p Path) string { return h.Ref(p) + "|" + h.Dir() }

func generic[T EmitHost](h T, p Path) string { return h.Ref(p) }

func anon(h interface {
	PrinterHost
	SpecifierHost
}) string {
	return h.Ref("anon") + h.Dir()
}

func main() {
	var h EmitHost = host{}
	fmt.Println(h.Ref("a.ts"), emit(h, "b.ts"), h.Dir())

	var s shared = host{}
	fmt.Println(s.Ref("shared"), emit(s, "s2"))

	var lh LabeledHost = host{}
	fmt.Println(lh.Ref("nested"), lh.Label(), emit(lh, "n2"), viaEmit(lh, "n3"))

	var ah AliasedHost = host{}
	fmt.Println(ah.Ref("alias"), ah.Root(), emit(ah, "a2"))

	var oh OwnHost = host{}
	fmt.Println(oh.Ref("own"), emit(oh, "o2"), oh.Dir())

	var b Big = host{}
	fmt.Println(viaEmit(b, "big"), b.Extra())

	var ph EmitHost = &phost{}
	fmt.Println(ph.Ref("p1"), emit(ph, "p2"), ph.Blocked("q"))

	var a any = host{}
	if eh, ok := a.(EmitHost); ok {
		fmt.Println(eh.Ref("assert"), eh.Blocked("x"))
	}
	if lh2, ok := a.(LabeledHost); ok {
		fmt.Println(lh2.Ref("assert2"))
	}

	fmt.Println(generic(h, "g1"), generic[EmitHost](host{}, "g2"))

	f := h.Ref
	fmt.Println(f("mv"))

	type local interface {
		PrinterHost
		Labeler
	}
	var l local = host{}
	fmt.Println(l.Ref("local"), l.Label())

	fmt.Println(anon(host{}), anon(h))
}
