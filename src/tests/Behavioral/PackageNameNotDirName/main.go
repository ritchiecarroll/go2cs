package main

import (
	"fmt"
	"unsafe"

	"PackageNameNotDirName/lib"
)

// sqlite-cs S19: modernc.org/sqlite imports "modernc.org/sqlite/lib", whose package is NAMED sqlite3. busy.go's
// `db := (*sqlite3.Tsqlite3)(unsafe.Pointer(c.db)); db.FbusyTimeout = tmout` is recorded `GoFieldAccess(typeof(lib.Tsqlite3))`,
// and vtab.go's `sqlite3.Sqlite3_int64(info.EstimatedRows)` is emitted `(lib.Sqlite3_int64)`: the directory name where the
// package name belongs. The control (s19ctl) is the same code with the directory named sqlite3.
func busy(p unsafe.Pointer, t int32, rows int64) {
	db := (*sqlite3.Tdb)(p)
	db.FbusyTimeout = t
	db.Frows = sqlite3.Sqlite3_int64(rows)
}

func main() {
	buf := make([]int64, 4) // the struct's memory, never declared as the struct: busy.go only casts pointers to it
	busy(unsafe.Pointer(&buf[0]), 250, 9)
	fmt.Println(int32(buf[0]), buf[3]) // Go: 250 9
}
