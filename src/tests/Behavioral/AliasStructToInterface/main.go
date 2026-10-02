package main

import (
	"fmt"

	"AliasStructToInterfaceLib"
)

// A pointer to a struct that another package only ALIASES (the lib's Map is sync.Map) converts to an
// interface declared here. The adapter that makes the conversion has to exist for the alias form too.

type mapInterface interface {
	Load(key any) (value any, ok bool)
	Store(key, value any)
}

func newMap() mapInterface {
	return &AliasStructToInterfaceLib.Map{}
}

func main() {
	m := newMap()
	m.Store("a", 1)
	v, ok := m.Load("a")
	fmt.Println(v, ok)
}
