package main

import (
	"fmt"
	"sync"
)

// A func literal passed as an ARGUMENT inside a defer or go statement's function expression
// (`defer lazy(func() int { return s.id })()`) is its own closure. The statement evaluates the
// call lazy(...) at once and defers only the function lazy returns, so a variable the literal
// reads is the literal's capture, never the statement's.

type scope struct {
	id    int
	names []string
}

type compiler struct {
	scopes []*scope
	count  int
}

func (c *compiler) lazy(f func() int) func() {
	return func() { fmt.Println("lazy:", f()) }
}

func (c *compiler) newScope() *scope {
	c.count++
	return &scope{id: c.count}
}

// The gojq shape: the variable is assigned on both arms of an if, then reassigned, then read by a
// literal handed to a method whose result is deferred.
func (c *compiler) define(builtin bool, outer *scope) {
	var s *scope
	if builtin {
		s = outer
	} else {
		s = c.scopes[len(c.scopes)-1]
	}
	defer c.lazy(func() int { return c.count })()
	fmt.Println("defining in scope", s.id)
	s = c.newScope()
	defer c.lazy(func() int { return s.id * 10 })()
	s.names = append(s.names, "x")
}

func later(f func() string) func() {
	return func() { fmt.Println("later:", f()) }
}

// Two variables, one of them a struct value, read by a literal inside a plain function's call.
func twoCaptures() {
	label := "total"
	point := struct{ x, y int }{3, 4}
	defer later(func() string { return fmt.Sprint(label, "=", point.x+point.y) })()
	fmt.Println("two captures")
}

func spawn(f func() string, wg *sync.WaitGroup) func() {
	return func() {
		defer wg.Done()
		fmt.Println("go:", f())
	}
}

// The same shape in a go statement.
func goCapture(n int) {
	var wg sync.WaitGroup
	wg.Add(1)
	labels := []string{"n", fmt.Sprint(n)}
	go spawn(func() string { return labels[0] + "=" + labels[1] }, &wg)()
	wg.Wait()
}

func main() {
	c := &compiler{}
	c.scopes = append(c.scopes, c.newScope())
	c.define(false, nil)
	c.define(true, &scope{id: 7})
	twoCaptures()
	goCapture(3)
}
