// Package lib defines a named []byte type, as jsontext defines Value, and a named string and a named
// func type beside it.
package lib

import "fmt"

type Value []byte

func (v Value) IsNull() bool { return string(v) == "null" }

func Print(v Value) { fmt.Printf("lib.Print %s len=%d\n", v, len(v)) }

type Name string

func (n Name) Len() int { return len(n) }

type Handler func(int) int

func (h Handler) Run(x int) int {
	if h == nil {
		return -1
	}
	return h(x)
}
