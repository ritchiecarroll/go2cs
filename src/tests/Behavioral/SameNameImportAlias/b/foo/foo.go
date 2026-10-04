package foo

type Other struct{ S string }

type Alias = Other

type Kind string

type S struct{}

func (S) Kind() Kind { return "b" }
