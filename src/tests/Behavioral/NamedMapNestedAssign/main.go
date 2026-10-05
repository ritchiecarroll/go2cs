// `m[k1][k2] = v` assigns through the outer map's element, an rvalue in C#: the converter emits
// `m[k1].Set(k2, v)`, a method call that writes through the shared backing store. golib's map has
// Set; a NAMED map wrapper must have it too, whether its map is inline or held (a self-containing
// map's wrapper holds its map in a StrongBox).
package main

import "fmt"

type Inner map[string]int

// Outer is a named map of a named map.
type Outer map[string]Inner

// OuterU is a named map of an UNNAMED map: its element is golib's own map<K,V>, which has Set.
type OuterU map[string]map[string]int

// Tree is a self-containing named map, whose wrapper holds its map in a holder.
type Tree map[string]Tree

func namedOfNamed() {
	d := Outer{"e": Inner{}}
	d["e"]["f"] = 1
	d["e"]["g"] = 2
	fmt.Println("named of named:", d["e"]["f"], d["e"]["g"], len(d["e"]))
}

func unnamedOfNamed() {
	n := map[string]Inner{"e": {}}
	n["e"]["f"] = 3
	fmt.Println("unnamed of named:", n["e"]["f"], len(n["e"]))
}

func namedOfUnnamed() {
	u := OuterU{"e": {}}
	u["e"]["f"] = 4
	fmt.Println("named of unnamed:", u["e"]["f"], len(u["e"]))
}

func holder() {
	t := Tree{"a": Tree{}}
	t["a"]["b"] = Tree{}
	t["a"]["b"]["c"] = nil
	_, hasC := t["a"]["b"]["c"]
	fmt.Println("holder:", len(t), len(t["a"]), len(t["a"]["b"]), hasC, t["a"]["b"]["c"] == nil)
}

func main() {
	namedOfNamed()
	unnamedOfNamed()
	namedOfUnnamed()
	holder()
}
