package main

import "fmt"

// The last file's init runs last.
func init() { fmt.Println("z.go init") }
