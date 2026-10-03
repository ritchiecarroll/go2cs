// A deferred VALUE-receiver method call copies its receiver at the defer statement, as Go does, for a field
// of a pointer, a field of a value, a local, and a dereferenced pointer (CS1113 when it was a method group).

package main

import "fmt"

type path struct{ name string }

func (p path) Pop(k int)            { fmt.Println("pop", p.name, k) }
func (p path) Done()                { fmt.Println("done", p.name) }
func (p path) Pair(a int, b string) { fmt.Println("pair", p.name, a, b) }

type state struct{ cur path }

func (s *state) fieldOfPointer(k int) {
	defer s.cur.Pop(k)
	defer s.cur.Pair(k, "b")
	s.cur = path{"replaced"}
}

func (s state) fieldOfValue(k int) {
	defer s.cur.Pop(k)
	s.cur = path{"replaced"}
}

func (s *state) nullary() {
	defer s.cur.Done()
	s.cur = path{"replaced"}
}

func local(k int) {
	p := path{"local"}
	defer p.Pop(k)
	p = path{"replaced"}
}

func viaPointer(p *path) {
	defer p.Done()
	*p = path{"replaced"}
}

func main() {
	s := &state{cur: path{"orig"}}
	s.fieldOfPointer(1)
	s.cur = path{"orig"}
	s.fieldOfValue(2)
	s.nullary()
	local(3)
	viaPointer(&path{"pointee"})
}
