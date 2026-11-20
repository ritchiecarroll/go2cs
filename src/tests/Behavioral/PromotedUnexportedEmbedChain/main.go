// PromotedUnexportedEmbedChain guards a POINTER-receiver method promoted to an exported type of ANOTHER package through
// a chain of value embeds that passes an UNEXPORTED embed (board pp, found converting github.com/yuin/goldmark v1.7.13:
// ast.Link embeds the unexported baseLink). The call must be the plain promoted call on the outer value, as Go writes
// it; the explicit field path names the unexported embed's accessor, which the other assembly cannot see.
package main

import (
	"fmt"

	lib "go/ppembedlib"
)

// wrap embeds the foreign Link: inside its own pointer method the receiver reaches Add through baseLink.
type wrap struct{ lib.Link }

func (w *wrap) add(s string) { w.Add(s) }

// holder keeps a Link in a field, reached through a pointer.
type holder struct{ link lib.Link }

func main() {
	// Through a pointer: the goldmark shape, three hops with the first unexported.
	l := lib.NewLink("dest")
	l.Add("a")
	l.Add("b")
	fmt.Println("link:", l.Dest, l.Count(), l.Last())

	// Through an addressable value.
	var v lib.Link
	v.Add("v1")
	fmt.Println("link value:", v.Count(), v.Last())

	// An exported first hop, then an unexported one.
	img := &lib.Image{}
	img.Add("i1")
	img.Add("i2")
	fmt.Println("image:", img.Count(), img.Last())

	var iv lib.Image
	iv.Add("iv")
	fmt.Println("image value:", iv.Count(), iv.Last())

	// Two hops, the first unexported.
	e := &lib.Emph{}
	e.Add("e1")
	e.Add("e2")
	fmt.Println("emph:", e.Count(), e.Last())

	// Controls: an exported single hop, and an unexported single hop.
	p := &lib.Para{}
	p.Add("p1")
	fmt.Println("para:", p.Count(), p.Last())

	t := &lib.Tag{}
	t.Bump()
	fmt.Println("tag:", t.Bump())

	// The receiver of the enclosing method, through the foreign unexported hop.
	w := &wrap{}
	w.add("w1")
	w.add("w2")
	fmt.Println("wrap:", w.Count(), w.Last())

	// Addressable values that are not plain variables: a slice element, an array element and a field of a
	// struct held through a pointer. Each is read back, so a call on a copy would show.
	s := make([]lib.Link, 2)
	s[0].Add("s0")
	s[1].Add("s1")
	s[1].Add("s1b")
	fmt.Println("slice:", s[0].Count(), s[1].Count(), s[1].Last())

	var a [2]lib.Link
	a[0].Add("a0")
	a[0].Add("a0b")
	fmt.Println("array:", a[0].Count(), a[0].Last(), a[1].Count())

	h := &holder{}
	h.link.Add("h1")
	fmt.Println("holder:", h.link.Count(), h.link.Last())

	// A local whose address is taken elsewhere: the call is on the value, and is read back through the pointer.
	var ev lib.Link
	pe := &ev
	ev.Add("x")
	fmt.Println("escaped:", pe.Count(), pe.Last())
}
