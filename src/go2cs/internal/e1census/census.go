// census.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/token"
	"go/types"
	"sort"
	"strings"

	"golang.org/x/tools/go/packages"
)

// The M:N twin generator's rule E1 (DESIGN-mn-twin-set.md 12.1) leaves generic Go functions and methods of generic
// types out of the twin set's forwarders and names them E1 in the population report. This census sizes the lift: of
// those generics, which PARK, directly or anywhere in their static call closure. docs/phase4/RECON-mn-e1-lift.md reads it.
//
// A function parks DIRECTLY when its body (function literals inside it included) holds a channel send or receive, a
// select, a range over a channel, a go statement, or a call to one of the wait primitives below. It parks TRANSITIVELY
// when a function it calls statically parks. A call the census cannot resolve statically (a func value, an interface
// method, a method of a type parameter's constraint) is DYNAMIC: it is not followed, and a generic whose only path to a
// park could be such a call is counted in its own bucket, never as a parker.

// waitPrimitives are the calls that block a goroutine without a channel operation the census can see: the bodies under
// them reach the runtime's semaphores through bodyless linkname declarations, so the census stops at the named call.
// The mutex locks are also in lockPrimitives: the twin generator does not let a lock make its caller a twin (N1,
// TwinGenerator.ParkCuts.cs: Mutex.Lock's twin is WEAK, and RWMutex has no twin), so the census reports a second,
// generator-aligned reading that does not count them.
var waitPrimitives = map[string]string{
	"sync.(*Mutex).Lock":          "sync.Mutex.Lock",
	"sync.(*RWMutex).Lock":        "sync.RWMutex.Lock",
	"sync.(*RWMutex).RLock":       "sync.RWMutex.RLock",
	"sync.(*WaitGroup).Wait":      "sync.WaitGroup.Wait",
	"sync.(*Cond).Wait":           "sync.Cond.Wait",
	"internal/sync.(*Mutex).Lock": "internal/sync.Mutex.Lock",
	"time.Sleep":                  "time.Sleep",
}

var lockPrimitives = map[string]bool{
	"sync.Mutex.Lock":          true,
	"sync.RWMutex.Lock":        true,
	"sync.RWMutex.RLock":       true,
	"internal/sync.Mutex.Lock": true,
}

// followRuntime crosses the runtime boundary. By default a call into package runtime or internal/runtime/... is not
// followed: those bodies are the Go runtime's own machinery (allocation reaching gcStart's worker goroutines, a finalizer
// goroutine's start), which converted code never calls, since golib stands in for it. A run with the boundary crossed
// is the literal "anywhere in the closure" reading; docs/phase4/RECON-mn-e1-lift.md reports both.
var followRuntime bool

// isRuntime is the boundary's predicate.
func isRuntime(f *types.Func) bool {
	if f.Pkg() == nil {
		return false
	}
	path := f.Pkg().Path()
	return path == "runtime" || strings.HasPrefix(path, "internal/runtime/")
}

// fn is one function or method declaration with a body, in any package the load reached.
type fn struct {
	obj       *types.Func
	pkg       *packages.Package
	decl      *ast.FuncDecl
	generic   bool
	direct    string // the first direct park in the body, or ""
	strong    string // the first direct park that is not a lock, or ""
	callees   []*types.Func
	dynamic   int         // calls not resolved statically
	parks     bool        // after the fixpoint
	via       *types.Func // the callee that made it park, when it parks only transitively
	strongs   bool        // parks without counting locks (the generator-aligned reading)
	viaStrong *types.Func // the callee that made it park in that reading, when only transitively
	dkinds    uint8       // the kinds of park its own body reaches (wait.go: W, C, O)
	kinds     uint8       // the kinds its whole static closure reaches, after the fixpoint
}

// census is the result for one load.
type census struct {
	targets map[*packages.Package]bool
	fns     map[*types.Func]*fn
	order   []*fn
}

// key spells a function as waitPrimitives does: pkgpath.Name or pkgpath.(*Recv).Name / pkgpath.Recv.Name.
func key(f *types.Func) string {
	sig, _ := f.Type().(*types.Signature)
	if sig == nil || sig.Recv() == nil {
		if f.Pkg() == nil {
			return f.Name()
		}
		return f.Pkg().Path() + "." + f.Name()
	}
	recv := sig.Recv().Type()
	ptr := false
	if p, ok := recv.(*types.Pointer); ok {
		recv, ptr = p.Elem(), true
	}
	name := "?"
	if named, ok := types.Unalias(recv).(*types.Named); ok {
		name = named.Obj().Name()
	}
	path := ""
	if f.Pkg() != nil {
		path = f.Pkg().Path() + "."
	}
	if ptr {
		return path + "(*" + name + ")." + f.Name()
	}
	return path + name + "." + f.Name()
}

// isGeneric is rule E1's predicate in Go terms: a function with type parameters, or a method whose receiver has them.
func isGeneric(f *types.Func) bool {
	sig, ok := f.Type().(*types.Signature)
	return ok && (sig.TypeParams().Len() > 0 || sig.RecvTypeParams().Len() > 0)
}

// exported mirrors the population's public test: the function's name and, for a method, its receiver type's name are
// exported. (A converted Go package keeps internal/ paths public in C#, so the path is not consulted.)
func exported(f *types.Func) bool {
	if !f.Exported() {
		return false
	}
	sig, _ := f.Type().(*types.Signature)
	if sig == nil || sig.Recv() == nil {
		return true
	}
	recv := sig.Recv().Type()
	if p, ok := recv.(*types.Pointer); ok {
		recv = p.Elem()
	}
	if named, ok := types.Unalias(recv).(*types.Named); ok {
		return named.Obj().Exported()
	}
	return false
}

// build walks every declaration with a body in every package the load reached and runs the fixpoint.
func build(roots []*packages.Package) *census {
	c := &census{targets: map[*packages.Package]bool{}, fns: map[*types.Func]*fn{}}
	for _, p := range roots {
		c.targets[p] = true
	}
	var all []*packages.Package
	packages.Visit(roots, nil, func(p *packages.Package) { all = append(all, p) })
	sort.Slice(all, func(i, j int) bool { return all[i].PkgPath < all[j].PkgPath })

	for _, p := range all {
		if p.TypesInfo == nil {
			continue
		}
		for _, file := range p.Syntax {
			for _, d := range file.Decls {
				decl, ok := d.(*ast.FuncDecl)
				if !ok || decl.Body == nil {
					continue
				}
				obj, ok := p.TypesInfo.Defs[decl.Name].(*types.Func)
				if !ok {
					continue
				}
				f := &fn{obj: obj, pkg: p, decl: decl, generic: isGeneric(obj)}
				scan(f, p.TypesInfo)
				c.fns[obj] = f
				c.order = append(c.order, f)
			}
		}
	}

	for _, f := range c.order {
		f.parks, f.strongs, f.kinds = f.direct != "", f.strong != "", f.dkinds
	}
	for changed := true; changed; {
		changed = false
		for _, f := range c.order {
			for _, callee := range f.callees {
				g := c.fns[callee]
				if g == nil {
					continue
				}
				if !f.parks && g.parks {
					f.parks, f.via, changed = true, callee, true
				}
				if !f.strongs && g.strongs {
					f.strongs, f.viaStrong, changed = true, callee, true
				}
				if g.kinds&^f.kinds != 0 {
					f.kinds |= g.kinds
					changed = true
				}
			}
		}
	}
	return c
}

// scan records a body's direct parks, static callees and dynamic calls.
func scan(f *fn, info *types.Info) {
	note := func(reason string) {
		if f.direct == "" {
			f.direct = reason
		}
		if f.strong == "" && !lockPrimitives[reason] {
			f.strong = reason
		}
		f.dkinds |= kindOf(reason)
	}
	ast.Inspect(f.decl.Body, func(n ast.Node) bool {
		switch n := n.(type) {
		case *ast.SendStmt:
			note("chan send")
		case *ast.UnaryExpr:
			if n.Op == token.ARROW {
				note("chan receive")
			}
		case *ast.SelectStmt:
			note("select")
		case *ast.GoStmt:
			note("go statement")
		case *ast.RangeStmt:
			if t := info.TypeOf(n.X); t != nil {
				if _, ok := t.Underlying().(*types.Chan); ok {
					note("range over chan")
				}
			}
		case *ast.CallExpr:
			callee, dynamic := resolve(n, info)
			switch {
			case dynamic:
				f.dynamic++
			case callee != nil:
				if reason, ok := waitPrimitives[key(callee)]; ok {
					note(reason)
					return true
				}
				if !followRuntime && isRuntime(callee) {
					return true
				}
				f.callees = append(f.callees, callee)
			}
		}
		return true
	})
}

// resolve names a call's static callee (its generic origin), or reports the call dynamic. A builtin, a conversion and
// a call of a function literal written in place are neither: the literal's body is scanned as part of the function.
func resolve(call *ast.CallExpr, info *types.Info) (*types.Func, bool) {
	fun := ast.Unparen(call.Fun)
	switch x := fun.(type) {
	case *ast.IndexExpr:
		fun = x.X
	case *ast.IndexListExpr:
		fun = x.X
	}
	if tv, ok := info.Types[fun]; ok && (tv.IsType() || tv.IsBuiltin()) {
		return nil, false
	}
	switch x := fun.(type) {
	case *ast.FuncLit:
		return nil, false
	case *ast.Ident:
		if f, ok := info.Uses[x].(*types.Func); ok {
			return f.Origin(), false
		}
		return nil, true
	case *ast.SelectorExpr:
		if sel, ok := info.Selections[x]; ok {
			f, ok := sel.Obj().(*types.Func)
			if !ok {
				return nil, true // a field of func type
			}
			recv := sel.Recv()
			if p, ok := recv.(*types.Pointer); ok {
				recv = p.Elem()
			}
			if _, ok := recv.(*types.TypeParam); ok {
				return nil, true
			}
			if types.IsInterface(recv) && sel.Kind() == types.MethodVal {
				return nil, true
			}
			return f.Origin(), false
		}
		if f, ok := info.Uses[x.Sel].(*types.Func); ok { // a package-qualified function
			return f.Origin(), false
		}
		return nil, true
	}
	return nil, true
}

// row is one target package set's counts.
type row struct {
	Module                                           string
	Funcs, Parkers                                   int
	Generic, GenericExported                         int
	GenericDirect, GenericTransitive                 int
	GenericParkersExported                           int
	GenericNoParkDynamic, GenericNoParkStatic        int
	GenericParksNoLocks, GenericParksNoLocksExported int
	// Exported generic FUNCTIONS only: in C# a method of a generic type is an internal glyph body (`Scan⁔<T>`) behind a
	// generated public extension, so the twin population's E1 members are the exported generic functions.
	ExportedFuncs, ExportedFuncParkers, ExportedFuncParkersNoLocks int
}

// parker is one generic that parks, with the first reason found.
type parker struct {
	Name, Reason string
	Exported     bool
	Locks        bool // parks only through a lock: not a park in the generator-aligned reading
}

// summarize counts the declarations of the target packages only.
func (c *census) summarize(module string) (row, []parker) {
	r := row{Module: module}
	var list []parker
	for _, f := range c.order {
		if !c.targets[f.pkg] {
			continue
		}
		r.Funcs++
		if f.parks {
			r.Parkers++
		}
		if !f.generic {
			continue
		}
		r.Generic++
		exp := exported(f.obj)
		if exp {
			r.GenericExported++
		}
		switch {
		case f.parks && f.direct != "":
			r.GenericDirect++
		case f.parks:
			r.GenericTransitive++
		case f.dynamic > 0:
			r.GenericNoParkDynamic++
		default:
			r.GenericNoParkStatic++
		}
		if sig, _ := f.obj.Type().(*types.Signature); exp && sig != nil && sig.Recv() == nil {
			r.ExportedFuncs++
			if f.parks {
				r.ExportedFuncParkers++
			}
			if f.strongs {
				r.ExportedFuncParkersNoLocks++
			}
		}
		if f.strongs {
			r.GenericParksNoLocks++
			if exp {
				r.GenericParksNoLocksExported++
			}
		}
		if f.parks {
			if exp {
				r.GenericParkersExported++
			}
			reason := f.direct
			if reason == "" {
				reason = "calls " + key(f.via)
			}
			list = append(list, parker{Name: key(f.obj), Reason: reason, Exported: exp, Locks: !f.strongs})
		}
	}
	sort.Slice(list, func(i, j int) bool { return list[i].Name < list[j].Name })
	return r, list
}

// counted reports whether the named function (key form) is a generic the census counts as parking.
func (c *census) counted(name string) bool {
	for _, f := range c.order {
		if f.generic && f.parks && key(f.obj) == name {
			return true
		}
	}
	return false
}

// countedNoLocks is counted's generator-aligned twin: it parks without counting locks.
func (c *census) countedNoLocks(name string) bool {
	for _, f := range c.order {
		if f.generic && f.strongs && key(f.obj) == name {
			return true
		}
	}
	return false
}

// chain spells how the named function parks, one hop per element, ending in the direct park; noLocks follows the
// generator-aligned reading. Empty when the function does not park in that reading.
func (c *census) chain(name string, noLocks bool) []string {
	var f *fn
	for _, g := range c.order {
		if key(g.obj) == name {
			f = g
			break
		}
	}
	var out []string
	for seen := map[*fn]bool{}; f != nil && !seen[f]; {
		seen[f] = true
		out = append(out, key(f.obj))
		parks, direct, via := f.parks, f.direct, f.via
		if noLocks {
			parks, direct, via = f.strongs, f.strong, f.viaStrong
		}
		if !parks {
			return nil
		}
		if direct != "" {
			return append(out, direct)
		}
		f = c.fns[via]
	}
	return out
}

// declared reports whether the named function (key form) was seen at all, so a control cannot pass vacuously.
func (c *census) declared(name string) bool {
	for _, f := range c.order {
		if key(f.obj) == name {
			return true
		}
	}
	return false
}
