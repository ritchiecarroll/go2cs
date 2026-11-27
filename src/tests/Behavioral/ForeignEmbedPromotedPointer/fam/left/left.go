// Package left embeds a SIBLING package's struct (right.T, same namespace) and a struct from
// the module's root namespace (lib.T, an ancestor of left's), each converted through a pointer.
package left

import (
	"ForeignEmbedPromotedPointer/fam/right"
	"ForeignEmbedPromotedPointer/lib"
)

// LR embeds the sibling's struct.
type LR struct {
	right.T
}

// LL embeds the ancestor namespace's struct.
type LL struct {
	lib.T
}

// Right converts *LR to right.Whoer.
func Right() string {
	var w right.Whoer = &LR{}
	return w.Who()
}

// Lib converts *LL to lib.Counter.
func Lib() string {
	ll := &LL{}
	var c lib.Counter = ll
	c.Inc()
	return c.Name()
}
