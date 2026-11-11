// PromotedValueEmbedUser is the caller half of the cross-package guard for a PLAIN pointer-receiver
// method promoted through an unexported value embed and READ IN AN EXPRESSION. It calls Widget's
// promoted Name() (declared on the unexported embedded common) from ANOTHER package — the shape of
// x/net/nettest's timeoutWrapper reading `t.Name() == "…"`. Before the fix the promoted forwarder was
// emitted `internal` (its @string return read as unexported), so the bare cross-package `Ꮡw.Name()`
// bound the same-named FOREIGN Gadget.Name extension instead → CS1929.
package main

import (
	"fmt"

	"PromotedValueEmbedLib"
)

// describe reaches the promoted Name() through a *Widget PARAMETER (the deref-aliased `Ꮡw` box form,
// mirroring nettest's timeoutWrapper) and READS the result in a comparison expression.
func describe(w *PromotedValueEmbedLib.Widget) string {
	if w.Name() == "widget" {
		return "match"
	}
	return "nomatch"
}

func main() {
	w := PromotedValueEmbedLib.New("widget")
	// w.Name() is read in an EXPRESSION (a comparison), not merely called — the nettest shape.
	fmt.Println(w.Name(), w.Name() == "widget", describe(w))
	// Pin the same-named foreign Gadget.Name extension into scope (the flag.Name analogue); with the
	// promoted Widget.Name forwarder invisible, the bare Ꮡw.Name() above bound THIS one → CS1929.
	g := PromotedValueEmbedLib.NewGadget("gadget")
	fmt.Println(g.Name())

	// Badge embeds the EXPORTED Meta by value. Its promoted value methods return a string, a bool and a
	// tuple of both: each forwarder was internal for its return type's spelling, and each of the three
	// calls was CS1929. Self is the control: a pointer method, which this package reaches through the
	// embed's exported box and not through a forwarder.
	b := PromotedValueEmbedLib.NewBadge("badge")
	text, ok := b.Both()
	self, name := b.Self()
	fmt.Println(b.Text(), b.Ok(), text, ok, self.Text(), name)
}
