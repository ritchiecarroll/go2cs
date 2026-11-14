package main

import "fmt"

// sqlite-cs S15 (variant): modernc.org/sqlite/lib sqlite.go:15 `type AggInfo = TAggInfo` is declared BEFORE, and in a
// different file from, `type TAggInfo = struct {...}` (sqlite.go:4743 / the generated files).
type AggInfo = TAggInfo

func main() {
	a := AggInfo{FnColumn: 2, FnFunc: 5}
	fmt.Println(a.FnColumn, a.FnFunc) // Go: 2 5
}
