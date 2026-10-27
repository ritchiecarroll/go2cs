// Package xpkgvar is the library half of the CrossPackageExportedVarAddress guard: exported package-level VALUE
// vars that nothing in this package addresses, so only an importer's `&` (or a pointer-receiver method value, or a
// field or element path) asks for their real storage. Go hands the importer the variable itself; a conversion that
// boxes a copy loses every write made through it.
package xpkgvar

import "sync"

// Counter has an ordinary pointer-receiver method, so `f := xpkgvar.V.Inc` is `(&xpkgvar.V).Inc`.
type Counter struct{ n int }

func (c *Counter) Inc()      { c.n++ }
func (c Counter) Value() int { return c.n }

// Holder carries a box-only field type (sync.WaitGroup) and a plain field, for the field-path shapes.
type Holder struct {
	WG sync.WaitGroup
	N  int
}

var N int
var V Counter
var S Holder
var Arr [3]int
var Flag bool

// Table is built from constants only, so the conversion may build it on first read: its box lives in a holder.
var Table = []int{1, 2, 3}

// ReadN reads N inside its own package, after the importer wrote it through a pointer.
func ReadN() int { return N }

// ReadS reads S.N inside its own package, after the importer wrote it through a field pointer.
func ReadS() int { return S.N }
