// spreadcensus: Go's f(g()) multi-value spread -- a call whose ONLY argument is a call returning two or more
// values. Classified by the callee's form and parameter shape. Read-only.
//
//   spreadcensus -std <patterns...>    go/packages (with tests)
//   spreadcensus -dirs <dir...>        each directory type-checked from source on its own (unresolved imports
//                                      tolerated: a site whose callee or argument cannot be typed is counted as
//                                      UNTYPED, never guessed)
package main

import (
	"flag"
	"fmt"
	"go/ast"
	"go/importer"
	"go/parser"
	"go/token"
	"go/types"
	"os"
	"path/filepath"
	"sort"
	"strings"

	"golang.org/x/tools/go/packages"
)

type site struct {
	form, shape, callee, ctx, pos string
	arity                         int
	test, calleeCalls             bool
}

var sites []site
var untyped int
var declCount = map[string]int{}
var declExported int

func isEmptyIface(t types.Type) bool {
	i, ok := t.Underlying().(*types.Interface)
	return ok && i.Empty()
}

func classify(fset *token.FileSet, info *types.Info, call *ast.CallExpr, ctx string, test bool) {
	if len(call.Args) != 1 || call.Ellipsis.IsValid() {
		return
	}
	inner, ok := ast.Unparen(call.Args[0]).(*ast.CallExpr)
	if !ok {
		return
	}
	tt, ok := info.TypeOf(inner).(*types.Tuple)
	if !ok || tt.Len() < 2 {
		return
	}
	// the callee
	fun := ast.Unparen(call.Fun)
	if ix, ok := fun.(*ast.IndexExpr); ok {
		fun = ix.X
	} else if ix, ok := fun.(*ast.IndexListExpr); ok {
		fun = ix.X
	}
	form, name := "func value", types.ExprString(call.Fun)
	switch f := fun.(type) {
	case *ast.Ident:
		switch o := info.Uses[f].(type) {
		case *types.Func:
			form = "package func"
		case *types.Builtin:
			form = "builtin"
		case *types.Var:
			form = "func value"
		case nil:
			untyped++
			return
		default:
			_ = o
		}
	case *ast.SelectorExpr:
		if sel := info.Selections[f]; sel != nil {
			if sel.Kind() == types.FieldVal {
				form = "func value"
			} else if types.IsInterface(sel.Recv()) {
				form = "interface method"
			} else {
				form = "method"
			}
		} else if o, ok := info.Uses[f.Sel].(*types.Func); ok {
			form = "package func"
			if o.Pkg() != nil {
				name = o.Pkg().Path() + "." + o.Name()
			}
		} else if _, ok := info.Uses[f.Sel].(*types.Var); ok {
			form = "func value"
		} else {
			untyped++
			return
		}
	case *ast.FuncLit:
		form = "func literal"
	}
	shape := "?"
	if form == "builtin" {
		shape = "builtin"
	} else if sig, ok := info.TypeOf(call.Fun).Underlying().(*types.Signature); ok {
		n := sig.Params().Len()
		if sig.Variadic() {
			elem := sig.Params().At(n - 1).Type().(*types.Slice).Elem()
			prefix := ""
			if n > 1 {
				prefix = fmt.Sprintf("%d fixed + ", n-1)
			}
			if isEmptyIface(elem) {
				shape = prefix + "...any"
			} else {
				shape = prefix + "...T"
			}
		} else {
			shape = fmt.Sprintf("fixed(%d)", n)
		}
	} else {
		untyped++
		return
	}
	// Whether the callee expression itself calls (pick()(g()), a.m(i()).n(g())): Go orders those calls BEFORE g().
	calleeCalls := false
	ast.Inspect(call.Fun, func(n ast.Node) bool {
		if c, ok := n.(*ast.CallExpr); ok {
			if tv, ok := info.Types[c.Fun]; !ok || !tv.IsType() {
				calleeCalls = true
			}
		}
		return !calleeCalls
	})
	sites = append(sites, site{form: form, shape: shape, callee: name, ctx: ctx, pos: fset.Position(call.Pos()).String(), arity: tt.Len(), test: test, calleeCalls: calleeCalls})
}

func walk(fset *token.FileSet, info *types.Info, f *ast.File, test bool) {
	var stack []ast.Node
	ast.Inspect(f, func(n ast.Node) bool {
		if n == nil {
			stack = stack[:len(stack)-1]
			return true
		}
		ctx := "expression"
		if len(stack) > 0 {
			switch p := stack[len(stack)-1].(type) {
			case *ast.ExprStmt:
				ctx = "statement"
			case *ast.DeferStmt:
				ctx = "defer"
			case *ast.GoStmt:
				ctx = "go"
			case *ast.ReturnStmt:
				ctx = "return"
			case *ast.AssignStmt, *ast.ValueSpec:
				ctx = "assignment"
			default:
				_ = p
			}
		}
		stack = append(stack, n)
		if call, ok := n.(*ast.CallExpr); ok {
			classify(fset, info, call, ctx, test)
		}
		return true
	})
}

func main() {
	std := flag.Bool("std", false, "load the patterns with go/packages")
	dirs := flag.Bool("dirs", false, "type-check each directory from source")
	verbose := flag.Bool("v", false, "list every site")
	decls := flag.Bool("decls", false, "with -std: count the declarations a generated per-arity overload (option B) would extend")
	flag.Parse()
	seen := map[string]bool{}
	if *std {
		cfg := &packages.Config{Mode: packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo, Tests: true}
		pkgs, err := packages.Load(cfg, flag.Args()...)
		if err != nil {
			fmt.Fprintln(os.Stderr, err)
			os.Exit(2)
		}
		for _, p := range pkgs {
			for _, f := range p.Syntax {
				fn := p.Fset.Position(f.Pos()).Filename
				if seen[fn] || p.TypesInfo == nil {
					continue
				}
				seen[fn] = true
				walk(p.Fset, p.TypesInfo, f, strings.HasSuffix(fn, "_test.go"))
				if *decls && !strings.HasSuffix(fn, "_test.go") {
					for _, d := range f.Decls {
						fd, ok := d.(*ast.FuncDecl)
						if !ok {
							continue
						}
						obj, _ := p.TypesInfo.Defs[fd.Name].(*types.Func)
						if obj == nil {
							continue
						}
						sig := obj.Type().(*types.Signature)
						if !sig.Variadic() {
							continue
						}
						n := sig.Params().Len()
						elem := sig.Params().At(n - 1).Type().(*types.Slice).Elem()
						kind := "...T"
						if isEmptyIface(elem) {
							kind = "...any"
						}
						if n > 1 {
							kind = "prefix + " + kind
						}
						declCount[kind]++
						if kind == "...any" && obj.Exported() {
							declExported++
						}
					}
				}
			}
		}
		if *decls {
			fmt.Printf("variadic declarations (non-test): %v; exported with ...any as the ONLY parameter: %d\n", declCount, declExported)
		}
	}
	if *dirs {
		for _, root := range flag.Args() {
			filepath.WalkDir(root, func(path string, d os.DirEntry, err error) error {
				if err != nil || !d.IsDir() {
					return nil
				}
				fset := token.NewFileSet()
				byPkg := map[string][]*ast.File{}
				ents, _ := os.ReadDir(path)
				for _, e := range ents {
					if e.IsDir() || !strings.HasSuffix(e.Name(), ".go") {
						continue
					}
					f, err := parser.ParseFile(fset, filepath.Join(path, e.Name()), nil, parser.SkipObjectResolution)
					if err != nil {
						continue
					}
					// The Tour's lessons are one program per file: type-check each file alone.
					key := f.Name.Name
					if strings.Contains(path, "_content/tour") {
						key = e.Name()
					}
					byPkg[key] = append(byPkg[key], f)
				}
				for _, files := range byPkg {
					info := &types.Info{Types: map[ast.Expr]types.TypeAndValue{}, Uses: map[*ast.Ident]types.Object{}, Defs: map[*ast.Ident]types.Object{}, Selections: map[*ast.SelectorExpr]*types.Selection{}}
					conf := types.Config{Importer: importer.ForCompiler(fset, "source", nil), Error: func(error) {}}
					conf.Check(path, fset, files, info)
					for _, f := range files {
						walk(fset, info, f, strings.HasSuffix(fset.Position(f.Pos()).Filename, "_test.go"))
					}
				}
				return nil
			})
		}
	}
	type key struct{ form, shape string }
	byKind := map[key][2]int{}
	callees := map[string]int{}
	ctxs := map[string]int{}
	maxArity := 0
	for _, s := range sites {
		k := key{s.form, s.shape}
		c := byKind[k]
		if s.test {
			c[1]++
		} else {
			c[0]++
		}
		byKind[k] = c
		if strings.HasSuffix(s.shape, "...any") {
			callees[s.callee]++
		}
		ctxs[s.ctx]++
		if s.arity > maxArity {
			maxArity = s.arity
		}
	}
	fmt.Printf("sites %d (untyped, not counted: %d); max arity %d\n", len(sites), untyped, maxArity)
	var ks []key
	for k := range byKind {
		ks = append(ks, k)
	}
	sort.Slice(ks, func(i, j int) bool { return byKind[ks[i]][0]+byKind[ks[i]][1] > byKind[ks[j]][0]+byKind[ks[j]][1] })
	for _, k := range ks {
		fmt.Printf("  %-16s %-18s non-test %5d  test %5d\n", k.form, k.shape, byKind[k][0], byKind[k][1])
	}
	fmt.Printf("contexts: %v\n", ctxs)
	arities := map[int]int{}
	for _, s := range sites {
		arities[s.arity]++
	}
	fmt.Printf("arities: %v\n", arities)
	cc, ccNonTest := 0, 0
	for _, s := range sites {
		if s.calleeCalls {
			cc++
			if !s.test {
				ccNonTest++
			}
		}
	}
	fmt.Printf("callee expression itself calls (today's temp form runs g() first; Go runs the callee's calls first): %d (%d in non-test files)\n", cc, ccNonTest)
	var cs []string
	for c, n := range callees {
		cs = append(cs, fmt.Sprintf("%5d %s", n, c))
	}
	sort.Sort(sort.Reverse(sort.StringSlice(cs)))
	fmt.Println("...any callees:")
	for _, c := range cs {
		fmt.Println(c)
	}
	if *verbose {
		for _, s := range sites {
			fmt.Printf("SITE %s\t%s\t%s\t%s\t%d\t%s\ttest=%v\tcalleeCalls=%v\n", s.pos, s.form, s.shape, s.callee, s.arity, s.ctx, s.test, s.calleeCalls)
		}
	}
}
