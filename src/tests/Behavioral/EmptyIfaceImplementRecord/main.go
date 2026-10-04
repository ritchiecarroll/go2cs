package main

import "fmt"

type EmptyInterface interface{}

type name struct{ s string }

func (n name) String() string { return n.s }

func args(xs ...interface{}) int { return len(xs) }

func main() {
	var s fmt.Stringer = name{"ok"}
	fmt.Println(args(struct{ EmptyInterface }{}), s.String())
}
