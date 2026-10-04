// A deferred value-receiver call that takes the lambda form -- a nullary callee returning a result, or a
// variadic callee -- copies its receiver at the defer statement, as Go does, over a field or dereferenced
// receiver. An identifier receiver was already copied by the capture hoist.

package main

import "fmt"

type path struct{ name string }

func (p path) Close() error    { fmt.Println("close", p.name); return nil }
func (p path) Log(args ...any) { fmt.Println("log", p.name, args) }

type state struct{ cur path }

func (s *state) run(k int) {
	defer s.cur.Close()
	defer s.cur.Log("k", k)
	s.cur = path{"replaced"}
}

func viaPointer(p *path) {
	defer p.Close()
	*p = path{"replaced"}
}

func local() {
	k := path{"local"}
	defer k.Close()
	k = path{"replaced"}
}

func main() {
	s := &state{cur: path{"orig"}}
	s.run(1)
	viaPointer(&path{"pointee"})
	local()
}
