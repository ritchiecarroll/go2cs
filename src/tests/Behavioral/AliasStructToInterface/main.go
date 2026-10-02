package main

import (
	"fmt"
	"sync"

	"AliasStructToInterfaceLib"
)

// A pointer to a struct that another package only ALIASES (the lib's Map is sync.Map) converts to an
// interface declared here. The adapter that makes the conversion has to exist for the alias form too.
// newDirect is the control: a pointer to sync.Map itself, spelled without the alias, converts to an
// interface of its own (so the alias form above cannot borrow its adapter) and must emit exactly as it
// did before the alias form converted.

type mapInterface interface {
	Load(key any) (value any, ok bool)
	Store(key, value any)
}

func newMap() mapInterface {
	return &AliasStructToInterfaceLib.Map{}
}

type storer interface {
	Store(key, value any)
}

func newDirect() storer {
	return &sync.Map{}
}

func main() {
	m := newMap()
	m.Store("a", 1)
	v, ok := m.Load("a")
	fmt.Println(v, ok)

	d := newDirect()
	d.Store("b", 2)
	v, ok = d.(*sync.Map).Load("b")
	fmt.Println(v, ok)
	_, ok = d.(*sync.Map).Load("a")
	fmt.Println(ok)
}
