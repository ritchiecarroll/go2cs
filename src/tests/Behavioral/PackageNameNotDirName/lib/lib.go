// Package sqlite3 is in a directory named lib, as modernc.org/sqlite/lib declares package sqlite3.
package sqlite3

type Sqlite3_int64 = int64

type Tdb = struct {
	FbusyTimeout int32
	FaDb         [2]int64
	Frows        Sqlite3_int64
}
