package PromotedIfaceTestEmbed_test

import (
	"testing"

	"example.com/PromotedIfaceTestEmbed"
)

// The EXTERNAL test package's struct embeds the package-under-test's Base and is used as its Namer. The module
// path has a dotted host, as a third-party module does: the package's C# namespace is then not `go`.
type outer struct {
	PromotedIfaceTestEmbed.Base
}

func TestExternalPromoted(t *testing.T) {
	if got := PromotedIfaceTestEmbed.Show(outer{PromotedIfaceTestEmbed.Base{N: 1}}); got != "base" {
		t.Fatal(got)
	}
	var n PromotedIfaceTestEmbed.Namer = &outer{PromotedIfaceTestEmbed.Base{N: 3}}
	if n.Count() != 3 {
		t.Fatal(n.Count())
	}
}
