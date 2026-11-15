package lib

// modernc.org/sqlite/lib's shape (sqlite-cs W7): an exported alias declared ABOVE the anonymous struct its chain
// reaches, the middle link declared below both (sqlite.go:4702, :9300, :48415). The alias's GoTypeAlias record
// carried the unresolved dynamic-type marker an importer then copied into its `global using`.
type Sqlite3_vtab_cursor = sqlite3_vtab_cursor

type Tsqlite3_vtab_cursor = struct {
	FpVtab uintptr
	FnRef  int32
}

// A second exported alias straight to the struct, declared after it, recorded its lift name all along.
type Cursor = Tsqlite3_vtab_cursor

func Open(n int32) Sqlite3_vtab_cursor { return Sqlite3_vtab_cursor{FpVtab: 7, FnRef: n} }

type sqlite3_vtab_cursor = Tsqlite3_vtab_cursor

// The same forward chain to an anonymous INTERFACE (P3's review of W7, arm b): its lift is resolved from the record
// as the struct's is.
type Visitor = visitor

type Tvisitor = interface {
	Visit(n int32) int32
}

type Doubler struct{ By int32 }

func (d Doubler) Visit(n int32) int32 { return n * d.By }

type visitor = Tvisitor
