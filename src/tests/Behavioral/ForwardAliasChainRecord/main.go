package main

import (
	"fmt"

	"ForwardAliasChainRecord/lib"
)

// sqlite-cs W7: an importer of an alias whose chain is declared forward. The record lib published for
// Sqlite3_vtab_cursor kept the marker the .cs files had resolved, so this file's `global using` did not parse
// (CS1730 and a parse cascade). Both aliases name one type, so values pass between them freely.
func main() {
	var c lib.Sqlite3_vtab_cursor = lib.Open(2)
	c.FpVtab++

	var d lib.Cursor = c
	d.FnRef *= 10

	fmt.Println(c.FpVtab, c.FnRef, d.FpVtab, d.FnRef)
}
