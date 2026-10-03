package main

import "fmt"

// A NAMED empty interface (mapstructure's `type DecodeHookFunc interface{}`) taken as a VARIADIC
// parameter (its ComposeDecodeHookFunc(fs ...DecodeHookFunc)).
type Hook interface{}

func compose(hs ...Hook) int { return len(hs) }

func main() {
	fmt.Println(compose(1, "two", 3.0))
}
