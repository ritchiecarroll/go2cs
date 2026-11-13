package main

import (
	"fmt"
	"unsafe"

	"ForeignAnonStructAliasAlias/lib"
)

// sqlite-cs S2: modernc.org/sqlite's vtab.go `type cIndexConstraint = sqlite3.Tsqlite3_index_constraint`, an alias in
// another package of an alias of an anonymous struct type.
type cIndexConstraint = lib.Tindex_constraint

func main() {
	c := cIndexConstraint{FiColumn: 7, Fop: 2}
	fmt.Println(c.FiColumn, c.Fop, unsafe.Sizeof(c))
}
