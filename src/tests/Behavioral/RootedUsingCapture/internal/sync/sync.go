// Package sync is a module-local package that shares its name, and the name of its Mutex type, with
// the standard library's sync. Its namespace holds a sync_package class beside the standard one.
package sync

// Mutex is a different type from the standard library's sync.Mutex.
type Mutex struct{ held bool }

func (m *Mutex) Lock()   { m.held = true }
func (m *Mutex) Unlock() { m.held = false }

// Gauge has a method, so a package calling it imports this package's namespace for the call.
type Gauge struct{ v int }

func NewGauge(v int) Gauge { return Gauge{v} }

func (g Gauge) Value() int { return g.v }
