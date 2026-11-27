// wait.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import "sort"

// The wait-twin reach census. sync is hand-owned, and its WaitGroup.Wait and Cond.Wait have no twin, so a function that
// parks ONLY through one of them is not twinned today and holds its carrier while it waits. A hand-written
// WaitGroup.Waitꞏtwin or Cond.Waitꞏtwin would newly twin exactly those functions. The census classifies every function by
// the KINDS of park point its static closure reaches:
//
//	W  sync.(*WaitGroup).Wait
//	C  sync.(*Cond).Wait
//	O  anything else that parks: a channel send or receive, a select, a range over a channel, a go statement, a mutex
//	   or RWMutex lock (sync or internal/sync), time.Sleep
//
// A function reads W when its closure reaches W and nothing else, C when it reaches C and nothing else, and WC when it
// reaches both and nothing else. That is the LITERAL reading, where a lock is another park point. The census also gives
// a LOCKS-ASIDE reading, where the mutex and RWMutex locks (kind L) are set aside, as the twin generator sets them aside:
// a lock never makes its caller a twin (N1, TwinGenerator.ParkCuts.cs). Every Cond.Wait caller holds its lock, so the
// literal reading cannot see a Cond.Waitꞏtwin's reach at all. Calls the census cannot resolve (func values, interface methods, type-parameter
// methods) are not followed (census.go), so a function whose wait is reached only through such a call is not counted.
const (
	kindW uint8 = 1 << iota
	kindC
	kindO
	kindL // a mutex or RWMutex lock: "other" in the literal reading, set aside in the locks-aside one
)

// kindOf maps a direct park's reason (census.go's note) to its kind.
func kindOf(reason string) uint8 {
	switch reason {
	case "sync.WaitGroup.Wait":
		return kindW
	case "sync.Cond.Wait":
		return kindC
	}
	if lockPrimitives[reason] {
		return kindL
	}
	return kindO
}

// waitRow is one target package set's wait-twin reach.
type waitRow struct {
	Module                    string
	Funcs, Parkers            int
	W, C, WC                  int // parks only through WaitGroup.Wait, only through Cond.Wait, only through both
	WExported, CExported      int
	WCExported                int
	DirectW, DirectC          int // functions whose own body calls WaitGroup.Wait / Cond.Wait
	WGeneric, CGeneric, WCGen int // of W, C and WC, how many are generic (E1 already leaves them without a forwarder)
}

// waiter is one W, C or WC function with its static fan-in over the whole load.
type waiter struct {
	Name, Class string
	Exported    bool
	Callers     int
}

// waitOnly classifies a function: "W", "C", "WC" or "". locksAside sets the lock kind aside; otherwise a lock is
// another park point.
func waitOnly(f *fn, locksAside bool) string {
	kinds := f.kinds
	if locksAside {
		kinds &^= kindL
	} else if kinds&kindL != 0 {
		kinds |= kindO
	}
	switch kinds {
	case kindW:
		return "W"
	case kindC:
		return "C"
	case kindW | kindC:
		return "WC"
	}
	return ""
}

// waitSummary counts the target packages' functions and lists the W, C and WC ones, most-called first (callers counted
// over every function the load reached, each caller once).
func (c *census) waitSummary(module string, locksAside bool) (waitRow, []waiter) {
	callers := map[*fn]map[*fn]bool{}
	for _, f := range c.order {
		for _, callee := range f.callees {
			if g := c.fns[callee]; g != nil && g != f {
				if callers[g] == nil {
					callers[g] = map[*fn]bool{}
				}
				callers[g][f] = true
			}
		}
	}

	r := waitRow{Module: module}
	var list []waiter
	for _, f := range c.order {
		if !c.targets[f.pkg] {
			continue
		}
		r.Funcs++
		if f.kinds != 0 {
			r.Parkers++
		}
		if f.dkinds&kindW != 0 {
			r.DirectW++
		}
		if f.dkinds&kindC != 0 {
			r.DirectC++
		}
		class := waitOnly(f, locksAside)
		if class == "" {
			continue
		}
		exp := exported(f.obj)
		switch class {
		case "W":
			r.W++
			if exp {
				r.WExported++
			}
			if f.generic {
				r.WGeneric++
			}
		case "C":
			r.C++
			if exp {
				r.CExported++
			}
			if f.generic {
				r.CGeneric++
			}
		case "WC":
			r.WC++
			if exp {
				r.WCExported++
			}
			if f.generic {
				r.WCGen++
			}
		}
		list = append(list, waiter{Name: key(f.obj), Class: class, Exported: exp, Callers: len(callers[f])})
	}
	sort.Slice(list, func(i, j int) bool {
		if list[i].Callers != list[j].Callers {
			return list[i].Callers > list[j].Callers
		}
		return list[i].Name < list[j].Name
	})
	return r, list
}

// waitClass returns the named function's class (key form) in the chosen reading, and whether it was seen at all.
func (c *census) waitClass(name string, locksAside bool) (string, bool) {
	for _, f := range c.order {
		if key(f.obj) == name {
			return waitOnly(f, locksAside), true
		}
	}
	return "", false
}
