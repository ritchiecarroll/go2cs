// PromotedUnexportedEmbedChain guards a POINTER-receiver method promoted to an exported type of ANOTHER package through
// a chain of value embeds that passes an UNEXPORTED embed (board pp, found converting github.com/yuin/goldmark v1.7.13:
// ast.Link embeds the unexported baseLink). The call must be the plain promoted call on the outer value, as Go writes
// it; the explicit field path names the unexported embed's accessor, which the other assembly cannot see.
package main

import (
	"fmt"

	lib "go/ppembedlib"
)

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
}
