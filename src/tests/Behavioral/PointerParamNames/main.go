// PointerParamNames: a pointer parameter keeps its Go name unless the body also uses its deref alias.
package main

import (
	"fmt"
	"strings"
)

type counter struct{ n int }

func (c *counter) Inc() { c.n++ }

// isNil uses the parameter only as a pointer, so no deref alias exists: it keeps its Go name.
func isNil(c *counter) bool {
	return c == nil
}

// passOn only passes the pointer along, and calls a pointer method on it.
func passOn(c *counter) {
	c.Inc()
	bump(c)
}

// bump reads and writes the pointee, so the alias exists and the box keeps its mark.
func bump(c *counter) {
	c.n++
}

// shadow reuses the name in a nested scope.
func shadow(c *counter) int {
	if c != nil {
		c := 5
		return c
	}
	return -1
}

// capture closes over the parameter.
func capture(c *counter) func() bool {
	return func() bool { return c == nil }
}

// write calls a method of an imported pointer type, the testing.T shape (`t.Errorf`).
func write(b *strings.Builder, s string) {
	b.WriteString(s)
}

// deferred defers a pointer method on the parameter.
func deferred(c *counter) {
	defer c.Inc()
}

// both: one parameter with an alias and one without.
func both(a, b *counter) int {
	b.Inc()
	return a.n
}

func main() {
	var c counter
	fmt.Println(isNil(&c), isNil(nil))
	passOn(&c)
	fmt.Println(c.n)
	fmt.Println(shadow(&c), shadow(nil))
	fmt.Println(capture(&c)(), capture(nil)())
	var sb strings.Builder
	write(&sb, "go")
	write(&sb, "2cs")
	fmt.Println(sb.String())
	deferred(&c)
	fmt.Println(c.n)
	d := counter{n: 10}
	fmt.Println(both(&d, &c), c.n)
}
