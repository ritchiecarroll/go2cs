// lazyPackageVarOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/ast"
	"go/parser"
	"go/token"
	"go/types"
	"os"
	"path/filepath"
	"strings"
)

// Package-level VARS built on their FIRST READ (docs/DESIGN-lazy-static-data.md, ruled by COORD and the owner on
// 2026-10-22). Go's linker emits a table of constants and function values as static DATA, so it costs a Go program
// nothing at startup; the converted form was a field initializer (or a package_init.cs relocation) that the package's
// static constructor runs, JIT-compiling the whole table the first time ANY member of the package is touched. A lazy
// var is declared as a `ref` property over a holder class of its own, whose field initializer the CLR runs on the first
// read, exactly once; package_init.cs does not call it. A use site reads exactly as it did.
//
// WHICH VARS (collectLazyPackageVars): an ELIGIBLE initializer has a value that cannot depend on when it is computed and
// a computation nothing can observe (lazyInitializerRefusal states the five rules), and of the eligible vars a var is
// lazy when its initializer has at least lazyPackageVarNodeFloor nodes, or it reads a lazy var (closure: an eager reader
// would build the lazy var at class init and undo it), or it is an EXPORTED var a lazy initializer reads (it then needs
// its initial-value snapshot, below). A var whose address is taken stays eager until the holder carries the box.
//
// lazyPackageVars lists vars that MUST be lazy whatever their size (COORD's ruling (A): runtime's malloc dispatch
// tables, read only behind a const-false gate); a listed var the predicate refuses is refused by name at its declaration.
var lazyPackageVars = map[string]map[string]bool{
	"runtime": {
		"mallocScanTable":   true,
		"mallocNoScanTable": true,
	},
}

// lazyPackageVarNodeFloor is the initializer size, in syntax nodes, from which an eligible var is worth its holder class:
// a bool or a short string costs less built eagerly than a class of its own. 20 keeps 97.5% of the eligible
// initializer mass in std at a third of the vars (the design note's census).
const lazyPackageVarNodeFloor = 20

// lazyVarFacts records what a lazy var's declaration needs beyond the plain holder.
type lazyVarFacts struct {
	// snapshot: an EXPORTED var a lazy initializer reads. An importer may assign it before that initializer runs, where
	// Go's init-time build saw its initial value, so its holder keeps the initial value apart (`initial`) and lazy
	// initializers read that.
	snapshot bool
}

// packageLazyVars holds the current package's lazy vars (collectLazyPackageVars); packageLazyVarRefusals the LISTED vars
// the predicate refused, with the reason their declaration panics with.
var (
	packageLazyVars        map[types.Object]*lazyVarFacts
	packageLazyVarRefusals map[types.Object]string
)

// isLazyPackageVar reports whether the package-level var is built on its first read.
func isLazyPackageVar(obj types.Object) bool {
	_, lazy := packageLazyVars[obj]
	return lazy
}

// lazyPackageVarSnapshot reports whether a lazy var keeps its initial value apart for lazy initializers.
func lazyPackageVarSnapshot(obj types.Object) bool {
	facts, lazy := packageLazyVars[obj]
	return lazy && facts.snapshot
}

func isListedLazyPackageVar(obj types.Object) bool {
	return obj != nil && obj.Pkg() != nil && lazyPackageVars[resolveGorootVendoredPath(obj.Pkg().Path())][obj.Name()]
}

// lazyCandidate is one package-level var with a single initializer, as the predicate sees it.
type lazyCandidate struct {
	obj      *types.Var
	nodes    int
	constant bool   // a constant-valued initializer: eligible to READ, never worth a holder (its emission arm is the constant one)
	reason   string // why it is ineligible; "" while eligible
	deps     []*types.Var
	resolved bool
	visiting bool
}

// collectLazyPackageVars decides which of the package's vars are built on their first read. Called at the top of
// collectMovedInitVars, so every driver (-stdlib, the hand-owned sibling pass, -tests) decides alike, and its mutation
// analysis reads the package's internal _test.go files in every mode: a -tests build compiles them into the package, and
// -stdlib must emit the same declarations as -tests.
func collectLazyPackageVars(fset *token.FileSet, pkg *types.Package, info *types.Info, syntax []*ast.File) {
	packageLazyVars = map[types.Object]*lazyVarFacts{}
	packageLazyVarRefusals = map[types.Object]string{}

	if pkg == nil || info == nil {
		return
	}

	scope := pkg.Scope()
	mutated, addressed := packageVarMutations(fset, pkg, info, syntax)
	candidates := map[*types.Var]*lazyCandidate{}

	for _, initializer := range info.InitOrder {
		if len(initializer.Lhs) != 1 {
			for _, lhs := range initializer.Lhs {
				candidates[lhs] = &lazyCandidate{obj: lhs, reason: "it is declared with other names from one value", resolved: true}
			}

			continue
		}

		obj := initializer.Lhs[0]

		// A test file's own vars stay eager: only production declarations are emitted alike in both modes.
		if strings.HasSuffix(fset.Position(obj.Pos()).Filename, "_test.go") {
			continue
		}

		candidate := &lazyCandidate{obj: obj}

		ast.Inspect(initializer.Rhs, func(n ast.Node) bool {
			if n != nil {
				candidate.nodes++
			}

			return true
		})

		if tv, ok := info.Types[initializer.Rhs]; ok && tv.Value != nil {
			candidate.constant = true
		} else {
			candidate.reason = lazyInitializerRefusal(info, scope, initializer.Rhs, &candidate.deps)
		}

		if candidate.reason == "" && addressed[obj] {
			candidate.reason = "its address is taken, and the holder does not carry an addressable box yet"
		}

		candidates[obj] = candidate
	}

	var resolve func(candidate *lazyCandidate) bool

	resolve = func(candidate *lazyCandidate) bool {
		if candidate.resolved {
			return candidate.reason == ""
		}

		if candidate.visiting {
			candidate.reason = "its initializer reads vars in a cycle"
			return false
		}

		candidate.visiting = true

		for _, dep := range candidate.deps {
			if candidate.reason != "" {
				break
			}

			if mutated[dep] {
				candidate.reason = fmt.Sprintf("it reads the var %s, which the package or its tests assign or take the address of", dep.Name())
				break
			}

			depCandidate, ok := candidates[dep]

			if !ok {
				candidate.reason = fmt.Sprintf("it reads the var %s, which has no initializer of its own", dep.Name())
				break
			}

			if !resolve(depCandidate) {
				candidate.reason = fmt.Sprintf("it reads the var %s, which is not eligible (%s)", dep.Name(), depCandidate.reason)
			} else if dep.Exported() && depCandidate.constant {
				// An importer can assign it, and a constant var has no holder to keep its initial value.
				candidate.reason = fmt.Sprintf("it reads the exported var %s, which an importer can assign and which has no holder to keep its initial value", dep.Name())
			}
		}

		candidate.visiting = false
		candidate.resolved = true

		return candidate.reason == ""
	}

	readers := map[*types.Var][]*lazyCandidate{}

	for _, candidate := range candidates {
		if resolve(candidate) {
			for _, dep := range candidate.deps {
				readers[dep] = append(readers[dep], candidate)
			}
		}
	}

	// The lazy set: a fixpoint, since a var can become lazy through a dep or a reader that only just did.
	lazy := map[*types.Var]bool{}

	for changed := true; changed; {
		changed = false

		for obj, candidate := range candidates {
			if lazy[obj] || candidate.reason != "" || candidate.constant {
				continue
			}

			include := candidate.nodes >= lazyPackageVarNodeFloor || isListedLazyPackageVar(obj)

			for _, dep := range candidate.deps {
				include = include || lazy[dep]
			}

			if obj.Exported() {
				for _, reader := range readers[obj] {
					include = include || lazy[reader.obj]
				}
			}

			if include {
				lazy[obj] = true
				changed = true
			}
		}
	}

	for obj := range lazy {
		packageLazyVars[obj] = &lazyVarFacts{}
	}

	for obj := range lazy {
		for _, dep := range candidates[obj].deps {
			if dep.Exported() {
				packageLazyVars[dep].snapshot = true
			}
		}
	}

	for obj, candidate := range candidates {
		if isListedLazyPackageVar(obj) && candidate.reason != "" {
			packageLazyVarRefusals[obj] = candidate.reason
		}
	}

	// Invariant: every exported var a lazy initializer reads is lazy, so its snapshot exists (the fixpoint makes it lazy;
	// resolve refuses a reader of an exported constant var, which never is).
	for obj := range lazy {
		for _, dep := range candidates[obj].deps {
			if dep.Exported() && !isLazyPackageVar(dep) {
				panic(fmt.Sprintf("@collectLazyPackageVars - lazy var %s reads the exported var %s, which is not lazy and has no initial-value snapshot", obj.Name(), dep.Name()))
			}
		}
	}
}

// lazyInitializerRefusal returns why an initializer cannot be built on its first read, or "" when it can, and records the
// same-package vars it reads in deps. ELIGIBLE: (1) constants, types, package functions and func literals (a func value,
// whose body does not run when it is made); (2) composite literals and &T{...} allocations of eligible parts; (3)
// conversions and operators over eligible operands; (4) reads of same-package vars that are themselves eligible and never
// mutated (collectLazyPackageVars checks deps; an exported one is read through its initial-value snapshot); (5) slices
// with constant bounds of such a var (slicing aliases, so it is the same alias whenever it is taken). Everything else
// refuses: a call, an imported package's var, an index or a dereference (each copies a value as it is at that moment), a
// method value (it binds its receiver now), a channel receive, a type assertion.
func lazyInitializerRefusal(info *types.Info, scope *types.Scope, expr ast.Expr, deps *[]*types.Var) string {
	if tv, ok := info.Types[expr]; ok && tv.Value != nil {
		return ""
	}

	switch node := expr.(type) {
	case *ast.BasicLit, *ast.FuncLit:
		return ""
	case *ast.ParenExpr:
		return lazyInitializerRefusal(info, scope, node.X, deps)
	case *ast.Ident:
		switch obj := info.Uses[node].(type) {
		case *types.Const, *types.TypeName, *types.Func, *types.Nil, nil:
			return ""
		case *types.Var:
			if obj.IsField() {
				return "" // a struct literal's key names the field it sets
			}

			if obj.Parent() == scope {
				*deps = append(*deps, obj)
				return ""
			}

			return fmt.Sprintf("its initializer reads %s, which is not a package var", obj.Name())
		default:
			return fmt.Sprintf("its initializer names %s", node.Name)
		}
	case *ast.SelectorExpr:
		if packageIdent, ok := node.X.(*ast.Ident); ok {
			if _, isPackage := info.Uses[packageIdent].(*types.PkgName); isPackage {
				switch info.Uses[node.Sel].(type) {
				case *types.Const, *types.TypeName, *types.Func:
					return ""
				case *types.Var:
					return fmt.Sprintf("its initializer reads the imported var %s.%s, which its package may change before the first read", packageIdent.Name, node.Sel.Name)
				}

				return fmt.Sprintf("its initializer names %s.%s", packageIdent.Name, node.Sel.Name)
			}
		}

		if selection, ok := info.Selections[node]; ok && selection.Kind() != types.FieldVal {
			return "its initializer takes a method value, which binds its receiver when it runs"
		}

		return "its initializer reads a field, which code may write before the first read"
	case *ast.CompositeLit:
		for _, element := range node.Elts {
			if refusal := lazyInitializerRefusal(info, scope, element, deps); refusal != "" {
				return refusal
			}
		}

		return ""
	case *ast.KeyValueExpr:
		if refusal := lazyInitializerRefusal(info, scope, node.Key, deps); refusal != "" {
			return refusal
		}

		return lazyInitializerRefusal(info, scope, node.Value, deps)
	case *ast.UnaryExpr:
		switch node.Op {
		case token.AND:
			if _, isLiteral := ast.Unparen(node.X).(*ast.CompositeLit); isLiteral {
				return lazyInitializerRefusal(info, scope, node.X, deps)
			}

			return "its initializer takes the address of storage"
		case token.ARROW:
			return "its initializer receives from a channel"
		}

		return lazyInitializerRefusal(info, scope, node.X, deps)
	case *ast.BinaryExpr:
		if refusal := lazyInitializerRefusal(info, scope, node.X, deps); refusal != "" {
			return refusal
		}

		return lazyInitializerRefusal(info, scope, node.Y, deps)
	case *ast.CallExpr:
		if tv, ok := info.Types[node.Fun]; ok && tv.IsType() && len(node.Args) == 1 {
			return lazyInitializerRefusal(info, scope, node.Args[0], deps)
		}

		return "its initializer makes a call, which would run on the first read instead of at init"
	case *ast.SliceExpr:
		for _, bound := range []ast.Expr{node.Low, node.High, node.Max} {
			if bound != nil {
				if tv, ok := info.Types[bound]; !ok || tv.Value == nil {
					return "its initializer slices with a bound that is not a constant"
				}
			}
		}

		if _, isIdent := ast.Unparen(node.X).(*ast.Ident); !isIdent {
			return "its initializer slices something other than a var"
		}

		return lazyInitializerRefusal(info, scope, node.X, deps)
	case *ast.IndexExpr, *ast.IndexListExpr, *ast.StarExpr:
		return "its initializer indexes or dereferences storage, copying a value as it is at that moment"
	case *ast.TypeAssertExpr:
		return "its initializer makes a type assertion"
	}

	return fmt.Sprintf("its initializer is an expression of kind %T", expr)
}

// packageVarMutations finds the package vars the package ASSIGNS (assignment, increment, range assignment) or takes the
// address of, over its typed syntax and over its internal _test.go files, which -stdlib does not load: those are parsed
// here and matched by NAME, which can only refuse more (a local shadowing a package var's name reads as the var).
func packageVarMutations(fset *token.FileSet, pkg *types.Package, info *types.Info, syntax []*ast.File) (mutated, addressed map[*types.Var]bool) {
	mutated = map[*types.Var]bool{}
	addressed = map[*types.Var]bool{}
	scope := pkg.Scope()

	byName := func(ident *ast.Ident) *types.Var {
		v, _ := scope.Lookup(ident.Name).(*types.Var)
		return v
	}

	typed := func(ident *ast.Ident) *types.Var {
		v, _ := info.Uses[ident].(*types.Var)

		if v != nil && v.Parent() != scope {
			return nil
		}

		return v
	}

	visit := func(file *ast.File, resolve func(*ast.Ident) *types.Var) {
		root := func(expr ast.Expr) *types.Var {
			for {
				switch node := expr.(type) {
				case *ast.ParenExpr:
					expr = node.X
				case *ast.SelectorExpr:
					expr = node.X
				case *ast.IndexExpr:
					expr = node.X
				case *ast.IndexListExpr:
					expr = node.X
				case *ast.StarExpr:
					expr = node.X
				case *ast.Ident:
					return resolve(node)
				default:
					return nil
				}
			}
		}

		mark := func(expr ast.Expr, address bool) {
			if v := root(expr); v != nil {
				mutated[v] = true

				if address {
					addressed[v] = true
				}
			}
		}

		ast.Inspect(file, func(n ast.Node) bool {
			switch node := n.(type) {
			case *ast.AssignStmt:
				if node.Tok != token.DEFINE {
					for _, lhs := range node.Lhs {
						mark(lhs, false)
					}
				}
			case *ast.IncDecStmt:
				mark(node.X, false)
			case *ast.RangeStmt:
				if node.Tok == token.ASSIGN {
					for _, e := range []ast.Expr{node.Key, node.Value} {
						if e != nil {
							mark(e, false)
						}
					}
				}
			case *ast.UnaryExpr:
				if node.Op == token.AND {
					mark(node.X, true)
				}
			}

			return true
		})
	}

	seen := map[string]bool{}
	dirs := map[string]bool{}

	for _, file := range syntax {
		name := fset.Position(file.Pos()).Filename
		seen[filepath.Clean(name)] = true
		dirs[filepath.Dir(name)] = true
		visit(file, typed)
	}

	for dir := range dirs {
		names, _ := filepath.Glob(filepath.Join(dir, "*_test.go"))

		for _, name := range names {
			if seen[filepath.Clean(name)] {
				continue
			}

			source, err := os.ReadFile(name)

			if err != nil {
				continue
			}

			file, err := parser.ParseFile(token.NewFileSet(), name, source, parser.SkipObjectResolution)

			// Only the INTERNAL test files share the package's vars; an external _test package reaches only exported
			// ones, which the initial-value snapshot covers.
			if err != nil || file.Name.Name != pkg.Name() {
				continue
			}

			visit(file, byName)
		}
	}

	return mutated, addressed
}

// writeLazyPackageVar writes a lazy var as ONE line: today's exact eager declaration inside its marked holder,
// `[GoLazy] partial class lazyᴛ_Lu { internal static ж<RangeTable> _Lu = Ꮡ(new RangeTable(…)); }`, with writeTrailing
// putting the var's Go comment after it, so the Go name, the initializer and the comment stay together (COORD's ruling
// S1, the owner's steer: initialization noise out of the mainline). go2cs-gen's LazyHolderGenerator writes the `ref`
// accessor under the Go name, and for a snapshot var (`[GoLazy(Initial = true)]`) the holder's `initial` field.
func (v *Visitor) writeLazyPackageVar(access, csTypeName, csIDName, valExpr string, snapshot bool, writeTrailing func()) {
	marker := "GoLazy"

	if snapshot {
		marker = "GoLazy(Initial = true)"
	}

	v.writeOutput("[%s] partial class %s { %s static %s %s = %s; }", marker, lazyPackageVarHolder(csIDName), access, csTypeName, csIDName, valExpr)
	writeTrailing()
}

// lazyPackageVarHolder names a lazy var's holder class: lazyᴛ<name>, beside the initᴛ<name> it replaces.
func lazyPackageVarHolder(csIDName string) string {
	return "lazy" + TempVarMarker + csIDName
}
