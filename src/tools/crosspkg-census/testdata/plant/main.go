package main

import (
	"bytes"
	"fmt"
	"strings"
	"time"
)

// field-shadow: the field Unix shadows time.Time.Unix (depth 0 beats depth 1).
type Shadow struct {
	time.Time
	Unix int
}

// ambiguous: bytes.Buffer and strings.Builder both have Len/String/Write... at depth 1.
type Amb struct {
	bytes.Buffer
	strings.Builder
}

// declared: T declares String itself (the generator already skips it).
type Decl struct{ time.Time }

func (Decl) String() string { return "decl" }

func main() { fmt.Println(Shadow{}, Amb{}, Decl{}) }

// clash: a package-level function shares a promoted method name.
func Year() int { return 0 }
