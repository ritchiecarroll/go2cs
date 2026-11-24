// Named-pointer surface guard: a named pointer type (`type PP *point`, `type Features *flagSet`) is emitted
// as a generated wrapper over the pointer box. Go reaches through it exactly as through the unnamed pointer:
// a field select, an index, len and range, and passing it where a *T parameter is expected. Each of those
// used to name a member the wrapper lacks, or (range) was not converted at all.
package main

import (
	"fmt"
	"time"
)

type flagSet [2]uint8

type Features *flagSet

type point struct{ x, y int }

type PP *point

type holder struct{ f Features }

// first's *flagSet parameter is only read, so the converter lowers it to a ref parameter.
func first(o *flagSet) uint8 { return o[0] }

// timeLocation is a defined pointer over ANOTHER package's type (log/slog's shape). Its pointee is spelled through
// the file's `time` alias, which go2cs-gen's generated part must qualify.
type timeLocation *time.Location

func zoneName(l *time.Location) string { return l.String() }

func main() {
	p := PP(&point{x: 4})
	p.y = p.x + 1
	fmt.Println(p.x, p.y)

	g := Features(&flagSet{7, 8})
	g[1] = g[0] + 1
	fmt.Println(g[0], g[1], len(g))

	sum := 0
	for i, b := range g {
		sum += i + int(b)
	}
	fmt.Println(sum)

	h := holder{f: g}
	fmt.Println(first(g), first(h.f), h.f[1])

	tl := timeLocation(time.UTC)
	fmt.Println(zoneName(tl))
}
