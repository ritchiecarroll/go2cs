// A Go method declared on a named map or channel ALWAYS wins over a member the generated wrapper
// declares for its own use (Set, Add, Remove, Clear, ContainsKey, Send...), even when its signature is
// exactly the wrapper member's: C# prefers an instance member to the extension method a Go method is
// emitted as, so a same-shaped wrapper member would run instead, silently (Add even panicked: the
// wrapper's is Dictionary.Add). And the converter's own nested-map assignment `m[k1][k2] = v` still
// writes the map entry, never the Go method of that name, whether its receiver is a value or a pointer.
package main

import (
	"fmt"
	"strings"
)

// Env's Set has EXACTLY the map's key and value types.
type Env map[string]string

func (e Env) Set(k, v string) { e[strings.ToLower(k)] = v }

// Bag's Add has EXACTLY the map's key and value types.
type Bag map[string]int

func (b Bag) Add(k string, v int) { b[k] += v }

// Cache declares Remove, Clear and ContainsKey with the wrapper's own shapes.
type Cache map[string]int

var cleared int

func (c Cache) Remove(k string) bool {
	_, ok := c[k]
	delete(c, k)
	return !ok // inverted on purpose: a wrapper member would answer the opposite
}

func (c Cache) Clear() {
	cleared++
	for k := range c {
		delete(c, k)
	}
}

func (c Cache) ContainsKey(k string) bool { return k == "always" }

// PEnv's Set has a POINTER receiver: it still claims the name.
type PEnv map[string]string

func (p *PEnv) Set(k, v string) { (*p)[strings.ToUpper(k)] = v }

// Uniq is a GENERIC named map (hashset's shape) whose Remove has exactly the wrapper's shape.
type Uniq[T comparable] map[T]struct{}

func (u Uniq[T]) Remove(item T) bool {
	_, ok := u[item]
	delete(u, item)
	return !ok // inverted on purpose, as Cache's
}

// Pipe's Send has exactly the channel wrapper's shape.
type Pipe chan int

var sends int

func (p Pipe) Send(v int) {
	sends++
	p <- v * 10
}

// Set is a map type NAMED Set, with no methods: its wrapper may not declare a member Set (CS0542),
// so its nested assignment takes the door as well.
type Set map[string]int

// Hdr's Set takes (string, string) over map[string][]string: NOT the wrapper's shape (the control).
type Hdr map[string][]string

func (h Hdr) Set(k, v string) { h[strings.ToUpper(k)] = []string{v} }

func main() {
	e := Env{}
	e.Set("Key", "v")
	_, lower := e["key"]
	_, raw := e["Key"]
	fmt.Println("exact Set ran:", lower, raw)

	b := Bag{}
	b.Add("n", 2)
	b.Add("n", 3)
	fmt.Println("exact Add accumulated:", b["n"])

	c := Cache{"a": 1}
	fmt.Println("exact Remove:", c.Remove("a"), c.Remove("a"), len(c))
	c["b"] = 2
	c.Clear()
	fmt.Println("exact Clear:", cleared, len(c))
	fmt.Println("exact ContainsKey:", c.ContainsKey("always"), c.ContainsKey("b"))

	h := Hdr{}
	h.Set("accept", "x")
	fmt.Println("control Set:", len(h["ACCEPT"]), len(h["accept"]))

	// The nested assignment writes the entry itself: Env's own Set (which lowercases) must not run.
	envs := map[string]Env{"x": {}}
	envs["x"]["Mixed"] = "m"
	_, nestedRaw := envs["x"]["Mixed"]
	_, nestedLower := envs["x"]["mixed"]
	fmt.Println("nested assign raw:", nestedRaw, nestedLower)
	named := struct{ m map[string]Env }{m: map[string]Env{"y": {}}}
	named.m["y"]["Q"] = "q"
	fmt.Println("nested assign in field:", named.m["y"]["Q"])

	pe := PEnv{}
	pe.Set("low", "p")
	penvs := map[string]PEnv{"z": {}}
	penvs["z"]["low"] = "r"
	fmt.Println("pointer Set:", pe["LOW"], len(pe), "nested raw:", penvs["z"]["low"], len(penvs["z"]))

	u := Uniq[string]{"a": {}}
	fmt.Println("generic Remove:", u.Remove("a"), u.Remove("a"), len(u))

	p := make(Pipe, 1)
	p.Send(4)
	fmt.Println("exact Send:", sends, <-p)

	sets := map[string]Set{"a": {}}
	sets["a"]["k"] = 7
	fmt.Println("named Set nested:", sets["a"]["k"], len(sets["a"]))
}
