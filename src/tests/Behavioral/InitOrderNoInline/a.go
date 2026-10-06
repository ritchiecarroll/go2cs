package main

import "fmt"

// The first file's init runs first: Go runs a package's inits in the order the files are presented
// to the compiler (sorted by name), and within a file in source order.
func init() { fmt.Println("a.go init") }
