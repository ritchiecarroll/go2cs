package main

import (
	"bufio"
	"bytes"
	"io"
	"sync"
	"time"

	"simplant/lib"
)

type local struct{}

func (local) Lock() {}

// item 7: Mutex.Lock (cross-package) and local.Lock collide at depth 1 -- Go drops Lock; today the generator emits it.
type Seven struct {
	sync.Mutex
	local
}

type loc struct{}

func (loc) ReadString(delim byte) (string, error) { return "", nil }

// item 5: ReadWriter's ReadString is Go depth 2 (via *Reader), loc's is depth 1 -- Go promotes loc's. The flattened
// harvest puts ReadWriter's forwarder at depth 1 too.
type Five struct {
	bufio.ReadWriter
	loc
}

// interface hiding (io_test Buffer shape).
type Hide struct {
	bytes.Buffer
	io.WriterTo
}

// field shadow.
type Shadow struct {
	time.Time
	Unix int
}

func main() {}

// own-function clash: a package function shares a promoted name.
func Weekday() int { return 0 }

// var clash (not a function): a package VAR shares a promoted name (CS0102 in C#).
var Month = 1

// builtin clash: lib.L.TryTypeAssert collides with go.builtin's TryTypeAssert (global using static).
type BuiltinClash struct{ lib.L }

// C4 plant: Deep -> lib.RW (crosses) -> bufio.ReadWriter -> *bufio.Reader.ReadString (depth 3, via RW's forwarder).
type Deep struct{ lib.RW }

// generic enclosing struct over a non-generic foreign embed.
type Gen[T any] struct {
	time.Time
	v T
}

// collide plant (i'): Time.Year (depth 1, crossing) wins in Go over yr.Year (depth 2, same package), so the crossing
// row exists AND its name is not unique in the tree.
type yr struct{}

func (yr) Year() int { return 0 }

type colInner struct{ yr }

type Col struct {
	time.Time
	colInner
}
