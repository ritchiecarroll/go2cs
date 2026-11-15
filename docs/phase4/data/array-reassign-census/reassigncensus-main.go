// reassigncensus: every `=` whose left side is an EXISTING location of array type, or of a struct type that holds
// an array by value (at any depth through struct fields and array elements), in the packages a pattern names. The
// census for the whole-array reassignment parity defect (P5's finding, 2026-11-15): in Go `a = b` copies elements
// INTO a's storage; the converted code swaps a's backing, so an alias taken from a before the assignment keeps the
// old elements.
//
// For each site it prints a TSV row:
//
//	target  package  file:line  kind  dest  alias
//
//	kind   ARRAY (the left side's type is an array) or STRUCT (a struct holding an array by value)
//	dest   LOCAL (a function's own variable, not captured by a closure), CAPTURED, PARAM, PKGVAR, FIELD (x.f),
//	       ELEM (x[i]), DEREF (*p), OTHER
//	alias  for LOCAL and PARAM: HELD-BEFORE / HELD-LOOP / HELD-AFTER (stored earlier in the function; only later
//	       but inside a loop that holds the assignment; only after) when the same function STORES an alias of the variable (x[:], x[i:j], &x,
//	       &x.f..., &x[i] as the value of an assignment, declaration, return, send or composite literal) -- an alias
//	       that may be live across the assignment; TRANSIENT when it only takes one within a statement (a call
//	       argument, an operand, a pointer-receiver call); NONE when it takes none (the clone and an in-place copy
//	       cannot be told apart). For every other dest: UNKNOWN (an alias can be taken anywhere the location is
//	       reachable).
//
// The left side of a `:=`, a var declaration, a range clause, an argument, a return or a map value is new storage in
// Go too, and is not counted -- except (v2, P5's C5) a `:=` operand that REDECLARES an existing variable and a
// `for k, v = range` operand, which assign an existing location.
//
// v2 appends two columns, leaving the first six as v1 printed them:
//
//	form   SINGLE (a = b), RECV (a = <-ch), SELECT (case a = <-ch:), TUPLE-N (a, b = x, y), TUPLE-CALL (a, b = f()),
//	       TUPLE-COMMAOK (a, ok = m[k] / x.(T) / <-ch), DEFINE-REDECL (a, err := f() with a declared), RANGE
//	door   for DEREF (*p): UNSAFE-CONV (p is a (*[N]T)(unsafe.Pointer(…)) result, direct or through a variable it
//	       was stored into), SLICE-CONV (a (*[N]T)(slice) result), OTHER; "-" for every other dest
//
// v1's rows are the rows whose form is not DEFINE-REDECL or RANGE.
//
// usage: GOOS=… GOARCH=… reassigncensus [-tests] [-dir module-dir] <target-label> <pattern...>
package main

import (
	"flag"
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"os"
	"path/filepath"
	"sort"
	"strings"

	"golang.org/x/tools/go/packages"
)

func holdsArray(t types.Type, seen map[types.Type]bool) bool {
	if seen[t] {
		return false
	}
	seen[t] = true

	switch u := t.Underlying().(type) {
	case *types.Array:
		return true
	case *types.Struct:
		for i := 0; i < u.NumFields(); i++ {
			if holdsArray(u.Field(i).Type(), seen) {
				return true
			}
		}
	}

	return false
}

// rootIdent is the variable a location expression is rooted at through selectors, index, parens and stars, and
// whether the path went through a pointer (then the location is not the variable's own storage).
func rootIdent(e ast.Expr, info *types.Info) (*ast.Ident, bool) {
	viaPointer := false

	for {
		switch x := e.(type) {
		case *ast.ParenExpr:
			e = x.X
		case *ast.SelectorExpr:
			if t := info.TypeOf(x.X); t != nil {
				if _, ok := t.Underlying().(*types.Pointer); ok {
					viaPointer = true
				}
			}
			e = x.X
		case *ast.IndexExpr:
			if t := info.TypeOf(x.X); t != nil {
				switch t.Underlying().(type) {
				case *types.Pointer, *types.Slice, *types.Map:
					viaPointer = true
				}
			}
			e = x.X
		case *ast.StarExpr:
			viaPointer = true
			e = x.X
		case *ast.Ident:
			return x, viaPointer
		default:
			return nil, true
		}
	}
}

type site struct {
	pkg, pos, kind, dest, alias, form, door string
}

// arrayPtrConv classifies e as a conversion to a pointer-to-array type: UNSAFE-CONV from an unsafe.Pointer operand,
// SLICE-CONV from a slice operand (Go 1.17's slice-to-array-pointer conversion), "" otherwise.
func arrayPtrConv(e ast.Expr, info *types.Info) string {
	call, ok := ast.Unparen(e).(*ast.CallExpr)
	if !ok || len(call.Args) != 1 {
		return ""
	}

	tv, ok := info.Types[call.Fun]
	if !ok || !tv.IsType() {
		return ""
	}

	ptr, ok := tv.Type.Underlying().(*types.Pointer)
	if !ok {
		return ""
	}

	if _, ok := ptr.Elem().Underlying().(*types.Array); !ok {
		return ""
	}

	switch at := info.TypeOf(call.Args[0]); {
	case at == nil:
	case types.Identical(at, types.Typ[types.UnsafePointer]):
		return "UNSAFE-CONV"
	default:
		if _, ok := at.Underlying().(*types.Slice); ok {
			return "SLICE-CONV"
		}
	}

	return ""
}

func main() {
	tests := flag.Bool("tests", false, "include _test.go files")
	dir := flag.String("dir", "", "module directory to load from")
	flag.Parse()

	if flag.NArg() < 2 {
		fmt.Fprintln(os.Stderr, "usage: reassigncensus [-tests] [-dir d] <target-label> <pattern...>")
		os.Exit(2)
	}

	label := flag.Arg(0)
	cfg := &packages.Config{
		Mode:  packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo,
		Tests: *tests,
		Dir:   *dir,
		Env:   os.Environ(),
	}

	pkgs, err := packages.Load(cfg, flag.Args()[1:]...)

	if err != nil {
		fmt.Fprintln(os.Stderr, "load:", err)
		os.Exit(1)
	}

	loadErrs := 0
	packages.Visit(pkgs, nil, func(p *packages.Package) {
		loadErrs += len(p.Errors)
	})

	var sites []site
	seenPos := map[string]bool{}
	files := 0

	for _, p := range pkgs {
		if p.TypesInfo == nil {
			continue
		}

		info := p.TypesInfo

		for _, f := range p.Syntax {
			files++

			// Per function: the variables an alias is taken of, and those a closure captures.
			ast.Inspect(f, func(n ast.Node) bool {
				var body *ast.BlockStmt
				var ftype *ast.FuncType

				switch fn := n.(type) {
				case *ast.FuncDecl:
					body, ftype = fn.Body, fn.Type
				default:
					return true
				}

				if body == nil {
					return false
				}

				params := map[types.Object]bool{}
				for _, fl := range []*ast.FieldList{ftype.Params} {
					if fl == nil {
						continue
					}
					for _, fld := range fl.List {
						for _, nm := range fld.Names {
							if o := info.Defs[nm]; o != nil {
								params[o] = true
							}
						}
					}
				}
				if fd, ok := n.(*ast.FuncDecl); ok && fd.Recv != nil {
					for _, fld := range fd.Recv.List {
						for _, nm := range fld.Names {
							if o := info.Defs[nm]; o != nil {
								params[o] = true
							}
						}
					}
				}

				aliased := map[types.Object]bool{}
				held := map[types.Object][]token.Pos{}
				var loops [][2]token.Pos
				captured := map[types.Object]bool{}

				// An alias is HELD when it is stored: the value of an assignment, a declaration, a return, a send or a
				// composite literal element. Anywhere else (a call argument, an operand) it is used within its statement.
				markHeld := func(e ast.Expr) {
					e = ast.Unparen(e)
					// see through type conversions: unsafe.Pointer(&a[0]), uintptr(unsafe.Pointer(&b[0])), T(x[:])
					for {
						call, ok := e.(*ast.CallExpr)
						if !ok || len(call.Args) != 1 {
							break
						}
						if tv, ok := info.Types[call.Fun]; !ok || !tv.IsType() {
							break
						}
						e = ast.Unparen(call.Args[0])
					}
					var root ast.Expr
					switch x := e.(type) {
					case *ast.SliceExpr:
						root = x.X
					case *ast.UnaryExpr:
						if x.Op == token.AND {
							root = x.X
						}
					}
					if root == nil {
						return
					}
					if id, viaPtr := rootIdent(root, info); id != nil && !viaPtr {
						if o := info.Uses[id]; o != nil {
							held[o] = append(held[o], e.Pos())
						}
					}
				}

				ast.Inspect(body, func(m ast.Node) bool {
					switch x := m.(type) {
					case *ast.ForStmt:
						loops = append(loops, [2]token.Pos{x.Pos(), x.End()})
					case *ast.RangeStmt:
						loops = append(loops, [2]token.Pos{x.Pos(), x.End()})
					}
					switch x := m.(type) {
					case *ast.AssignStmt:
						for _, r := range x.Rhs {
							markHeld(r)
						}
					case *ast.ValueSpec:
						for _, v := range x.Values {
							markHeld(v)
						}
					case *ast.ReturnStmt:
						for _, r := range x.Results {
							markHeld(r)
						}
					case *ast.SendStmt:
						markHeld(x.Value)
					case *ast.CompositeLit:
						for _, el := range x.Elts {
							if kv, ok := el.(*ast.KeyValueExpr); ok {
								markHeld(kv.Value)
							} else {
								markHeld(el)
							}
						}
					}
					return true
				})

				markAlias := func(e ast.Expr) {
					if id, viaPtr := rootIdent(e, info); id != nil && !viaPtr {
						if o := info.Uses[id]; o != nil {
							aliased[o] = true
						}
					}
				}

				ast.Inspect(body, func(m ast.Node) bool {
					switch x := m.(type) {
					case *ast.SliceExpr:
						markAlias(x.X)
					case *ast.UnaryExpr:
						if x.Op == token.AND {
							markAlias(x.X)
						}
					case *ast.SelectorExpr:
						// a pointer-receiver method value or call on an addressable variable takes its address
						if sel := info.Selections[x]; sel != nil && sel.Kind() != types.FieldVal {
							if sig, ok := sel.Obj().Type().(*types.Signature); ok && sig.Recv() != nil {
								if _, ptr := sig.Recv().Type().(*types.Pointer); ptr {
									markAlias(x.X)
								}
							}
						}
					case *ast.FuncLit:
						ast.Inspect(x.Body, func(k ast.Node) bool {
							if id, ok := k.(*ast.Ident); ok {
								if o, ok := info.Uses[id].(*types.Var); ok && o.Parent() != nil && o.Pkg() == p.Types && o.Parent() != types.Universe {
									captured[o] = true
								}
							}
							return true
						})
					}
					return true
				})

				// heldClass: BEFORE when an alias is stored earlier in the function, LOOP when only later but inside a loop
				// that also holds the assignment (an earlier iteration's alias may be live), AFTER otherwise.
				heldClass := func(o types.Object, at token.Pos) string {
					class := ""
					for _, hp := range held[o] {
						if hp < at {
							return "HELD-BEFORE"
						}
						for _, l := range loops {
							if l[0] <= at && at < l[1] && l[0] <= hp && hp < l[1] {
								class = "HELD-LOOP"
							}
						}
					}
					if class == "" && len(held[o]) > 0 {
						class = "HELD-AFTER"
					}
					return class
				}

				// v2 (C5, C6): the assignments that are a select case's communication, and the variables a
				// `(*[N]T)(unsafe.Pointer(…))` or a `(*[N]T)(slice)` conversion is stored into.
				selectComm := map[ast.Node]bool{}
				convVar := map[types.Object]string{}
				ast.Inspect(body, func(m ast.Node) bool {
					switch x := m.(type) {
					case *ast.CommClause:
						if x.Comm != nil {
							selectComm[x.Comm] = true
						}
					case *ast.AssignStmt:
						if len(x.Lhs) == len(x.Rhs) {
							for i, l := range x.Lhs {
								if id, ok := l.(*ast.Ident); ok {
									if c := arrayPtrConv(x.Rhs[i], info); c != "" {
										if o := info.ObjectOf(id); o != nil {
											convVar[o] = c
										}
									}
								}
							}
						}
					case *ast.ValueSpec:
						if len(x.Names) == len(x.Values) {
							for i, nm := range x.Names {
								if c := arrayPtrConv(x.Values[i], info); c != "" {
									if o := info.Defs[nm]; o != nil {
										convVar[o] = c
									}
								}
							}
						}
					}
					return true
				})

				ast.Inspect(body, func(m ast.Node) bool {
					var lhsList []ast.Expr
					form := ""

					switch x := m.(type) {
					case *ast.AssignStmt:
						if x.Tok != token.ASSIGN && x.Tok != token.DEFINE {
							return true
						}
						lhsList = x.Lhs
						switch {
						case x.Tok == token.DEFINE:
							form = "DEFINE-REDECL"
						case selectComm[x]:
							form = "SELECT"
						case len(x.Lhs) == 1 && len(x.Rhs) == 1:
							form = "SINGLE"
							if u, ok := ast.Unparen(x.Rhs[0]).(*ast.UnaryExpr); ok && u.Op == token.ARROW {
								form = "RECV"
							}
						case len(x.Lhs) == len(x.Rhs):
							form = "TUPLE-N"
						default:
							form = "TUPLE-CALL"
							switch ast.Unparen(x.Rhs[0]).(type) {
							case *ast.IndexExpr, *ast.TypeAssertExpr, *ast.UnaryExpr:
								form = "TUPLE-COMMAOK"
							}
						}
					case *ast.RangeStmt:
						if x.Tok != token.ASSIGN {
							return true
						}
						form = "RANGE"
						for _, e := range []ast.Expr{x.Key, x.Value} {
							if e != nil {
								lhsList = append(lhsList, e)
							}
						}
					default:
						return true
					}

					for _, lhs := range lhsList {
						if id, ok := lhs.(*ast.Ident); ok && id.Name == "_" {
							continue
						}

						// A `:=` operand is counted only when it REDECLARES an existing variable (C5: it assigns it).
						if form == "DEFINE-REDECL" {
							id, ok := lhs.(*ast.Ident)
							if !ok || info.Defs[id] != nil {
								continue
							}
						}

						t := info.TypeOf(lhs)
						if t == nil {
							continue
						}

						kind := ""
						if _, ok := t.Underlying().(*types.Array); ok {
							kind = "ARRAY"
						} else if _, ok := t.Underlying().(*types.Struct); ok && holdsArray(t, map[types.Type]bool{}) {
							kind = "STRUCT"
						}
						if kind == "" {
							continue
						}

						dest, alias := "OTHER", "UNKNOWN"
						switch x := ast.Unparen(lhs).(type) {
						case *ast.Ident:
							o, _ := info.Uses[x].(*types.Var)
							switch {
							case o == nil:
							case o.Parent() == p.Types.Scope():
								dest = "PKGVAR"
							case params[o]:
								dest, alias = "PARAM", "NONE"
								if c := heldClass(o, m.Pos()); c != "" {
									alias = c
								} else if aliased[o] {
									alias = "TRANSIENT"
								}
							case captured[o]:
								dest = "CAPTURED"
							default:
								dest, alias = "LOCAL", "NONE"
								if c := heldClass(o, m.Pos()); c != "" {
									alias = c
								} else if aliased[o] {
									alias = "TRANSIENT"
								}
							}
						case *ast.SelectorExpr:
							dest = "FIELD"
						case *ast.IndexExpr:
							dest = "ELEM"
						case *ast.StarExpr:
							dest = "DEREF"
						}

						// C6: what the dereferenced pointer is. UNSAFE-CONV: a `(*[N]T)(unsafe.Pointer(…))` result, direct
						// or through a variable it was stored into; SLICE-CONV: a `(*[N]T)(slice)` result (door A's managed
						// window); OTHER: anything else.
						door := "-"
						if st, ok := ast.Unparen(lhs).(*ast.StarExpr); ok {
							door = "OTHER"
							if c := arrayPtrConv(st.X, info); c != "" {
								door = c
							} else if id, ok := ast.Unparen(st.X).(*ast.Ident); ok {
								if c := convVar[info.ObjectOf(id)]; c != "" {
									door = c
								}
							}
						}

						pos := p.Fset.Position(lhs.Pos())
						key := fmt.Sprintf("%s:%d:%d", pos.Filename, pos.Line, pos.Column)
						if seenPos[key] {
							continue
						}
						seenPos[key] = true

						rel := pos.Filename
						if i := strings.Index(filepath.ToSlash(rel), "/src/"); i >= 0 {
							rel = filepath.ToSlash(rel)[i+5:]
						}
						sites = append(sites, site{p.PkgPath, fmt.Sprintf("%s:%d", rel, pos.Line), kind, dest, alias, form, door})
					}
					return true
				})

				return false
			})
		}
	}

	sort.Slice(sites, func(i, j int) bool { return sites[i].pos < sites[j].pos })

	for _, s := range sites {
		fmt.Printf("%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\n", label, s.pkg, s.pos, s.kind, s.dest, s.alias, s.form, s.door)
	}

	fmt.Fprintf(os.Stderr, "%s: %d packages, %d files, %d load errors, %d sites\n", label, len(pkgs), files, loadErrs, len(sites))
}
