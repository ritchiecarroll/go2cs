// Package tables declares package vars go2cs builds on their first read (docs/DESIGN-lazy-static-data.md), interleaved
// with initializers and an init function that run code, so the program can compare what Go builds at init with what
// the converted program builds later.
package tables

type Entry struct {
	Name   string
	Weight int
}

type Rule struct {
	Name  string
	Apply func(int) int
}

// Order records every initializer and init function that RUNS code, in the order Go runs them.
var Order []string

func record(name string) int {
	Order = append(Order, name)
	return len(Order)
}

// Built at init (a call): first in Go's order.
var first = record("first")

// Lazy: over the floor, constants only.
var big = [...]Entry{{"a", 1}, {"b", 2}, {"c", 3}, {"d", 4}, {"e", 5}, {"f", 6}, {"g", 7}}

// Lazy by closure (under the floor, reads big); exported, and the lazy ByName reads it, so its initial value is kept.
var Heavy = big[4:7]

// Lazy by closure: reads Heavy's INITIAL value even after an importer assigns Heavy.
var ByName = map[string][]Entry{"heavy": Heavy, "all": big[:]}

// Lazy: allocations, each the same pointer on every read.
var Roots = [...]*Entry{&Entry{"r1", 1}, &Entry{"r2", 2}, &Entry{"r3", 3}, &Entry{"r4", 4}}

// Built at init (a call): after first, in Go's order.
var second = record("second")

// Lazy: func literals (values whose bodies run only when called), over the floor.
var Rules = [...]Rule{
	{"double", func(n int) int { return n * 2 }},
	{"square", func(n int) int { return n * n }},
	{"negate", func(n int) int { return -n }},
}

// Lazy WITH its box: its address is taken (here and by the importer), so the holder carries the box and &Grid is one
// address before and after the first read.
var Grid = [...]int{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20}

// GridAddr returns Grid's address.
func GridAddr() *[20]int { return &Grid }

func init() {
	record("init")
}

// Seen reports which of the code-running initializers ran, and in what order.
func Seen() (int, int) {
	return first, second
}
