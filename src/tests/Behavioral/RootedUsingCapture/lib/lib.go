// Package lib embeds the standard library's sync.Mutex in a file that also calls a method from the
// module-local internal/sync, as crypto/tls's halfConn does beside its internal/godebug calls.
package lib

import (
	"sync"

	isync "RootedUsingCapture/internal/sync"
)

type Guarded struct {
	sync.Mutex
	n int
}

func New(n int) *Guarded { return &Guarded{n: n} }

func (g *Guarded) Add(d int) int {
	g.Lock()
	defer g.Unlock()
	g.n += d
	return g.n
}

func Base() int { return isync.NewGauge(3).Value() }
