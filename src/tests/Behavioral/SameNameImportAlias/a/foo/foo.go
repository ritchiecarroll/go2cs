package foo

type Inner struct{ N int }

type Alias = Inner

type Kind int

type S struct{}

func (S) Kind() Kind { return 1 }
