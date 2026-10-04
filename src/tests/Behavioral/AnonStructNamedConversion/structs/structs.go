package structs

type AssignB struct{ A int }

func (x AssignB) Sum() int { return x.A * 10 }
