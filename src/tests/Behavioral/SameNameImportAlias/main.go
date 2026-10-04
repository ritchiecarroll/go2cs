package main

import (
	"fmt"

	afoo "SameNameImportAlias/a/foo"
	bfoo "SameNameImportAlias/b/foo"
)

type list []afoo.Alias

type table map[string]bfoo.Alias

type holder struct {
	a afoo.Alias
	b bfoo.Alias
	k bfoo.Kind
}

func main() {
	var x afoo.Alias = afoo.Inner{N: 1}
	var y bfoo.Alias = bfoo.Other{S: "s"}
	z := afoo.Alias{N: 2}
	fmt.Println(x.N, y.S, z.N)

	l := list{{N: 3}}
	t := table{"k": {S: "v"}}
	fmt.Println(l[0].N, t["k"].S)

	var ka afoo.Kind = afoo.S{}.Kind()
	var kb bfoo.Kind = bfoo.S{}.Kind()
	h := holder{a: x, b: y, k: kb}
	fmt.Println(ka, kb, h.a.N, h.b.S, h.k)
}
