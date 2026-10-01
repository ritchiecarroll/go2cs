package main

import (
	"fmt"
	"time"
)

// A VALUE-receiver method promoted through an embedded struct from ANOTHER package must bind. go2cs-gen promotes no
// method across packages (a metadata embed promotes fields only), so the converter has to reach the method through
// the embedded field itself; golang-jwt's NumericDate (it embeds time.Time and calls Truncate and Add) is the shape.

// Stamp embeds time.Time by VALUE, the jwt shape.
type Stamp struct {
	time.Time
}

// PtrStamp embeds *time.Time: a POINTER embed, reaching the same value methods.
type PtrStamp struct {
	*time.Time
}

// Outer reaches time.Time through a same-package hop first: the crossing is the SECOND hop.
type inner struct {
	time.Time
}

type Outer struct {
	inner
}

// The same-package CONTROL: the generator forwards this promotion, and its emission must not move.
type localBase struct{ n int }

func (b localBase) Twice() int { return b.n * 2 }

type Wrap struct {
	localBase
}

var base = time.Date(2026, 9, 30, 12, 34, 56, 789000000, time.UTC)

func main() {
	s := Stamp{base}
	p := &Stamp{base}

	// Calls: value X and pointer X, returning structs and scalars.
	fmt.Println(s.Truncate(time.Second).Format(time.RFC3339Nano))
	fmt.Println(p.Add(-time.Hour).Format(time.RFC3339))
	fmt.Println(s.Unix(), p.Nanosecond())

	// A pointer embed.
	t := base
	ps := PtrStamp{&t}
	pp := &PtrStamp{&t}
	fmt.Println(ps.Truncate(time.Minute).Format(time.RFC3339), pp.Year())

	// A two-hop chain whose second hop crosses packages.
	o := Outer{inner{base}}
	po := &o
	fmt.Println(o.Truncate(time.Hour).Format(time.RFC3339), po.Month())

	// A method VALUE and a method EXPRESSION through the value embed.
	f := s.Truncate
	fmt.Println(f(time.Minute).Format(time.RFC3339))
	g := Stamp.Add
	fmt.Println(g(s, time.Hour).Format(time.RFC3339))

	// The same-package control.
	w := Wrap{localBase{21}}
	fmt.Println(w.Twice())
}
