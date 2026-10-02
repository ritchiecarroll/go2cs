package PromotedIfaceTestEmbed

import "testing"

// The INTERNAL test package's struct embeds its own package's Base and is used as a Namer.
type inner struct {
	Base
}

func TestInternalPromoted(t *testing.T) {
	if got := Show(inner{Base{N: 1}}); got != "base" {
		t.Fatal(got)
	}
	var n Namer = &inner{Base{N: 2}}
	if n.Count() != 2 {
		t.Fatal(n.Count())
	}
}
