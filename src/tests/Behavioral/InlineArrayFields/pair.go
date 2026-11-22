package main

// Pair has no array field. It is admitted inline only because PairHolder (main.go) holds it by value, so this file
// takes no golib using of its own and its inline-layout attribute must name golib in full.
type Pair struct {
	X int32
	Y int32
}
