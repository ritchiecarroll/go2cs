// vpcensus: the visual-parity scout's Go-side census (items 2, 3, 4), read-only.
//   item 2: multi-name var/const specs, by form, with the Go-visible blockers to a single C# declaration
//   item 3: tagged switches the converter gives a tag temp (string tag, fallthrough, or a non-constant label), by tag
//           shape, and whether the temp could be dropped (level 2)
//   item 4: array composite literals, by form
//   vpcensus -std <patterns> | -dirs <dirs>
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

var counts = map[string]int{}

func add(k string) { counts[k]++ }

type unit struct {
	fset  *token.FileSet
	info  *types.Info
	files []*ast.File
}

// addrOrCaptured: the local objects whose address is taken, or that a func literal references.
func addrOrCaptured(u unit) (addr, captured map[types.Object]bool) {
	addr, captured = map[types.Object]bool{}, map[types.Object]bool{}
	for _, f := range u.files {
		ast.Inspect(f, func(n ast.Node) bool {
			switch x := n.(type) {
			case *ast.UnaryExpr:
				if x.Op == token.AND {
					if id, ok := ast.Unparen(x.X).(*ast.Ident); ok {
						if o := u.info.Uses[id]; o != nil {
							addr[o] = true
						}
					}
				}
			case *ast.FuncLit:
				ast.Inspect(x.Body, func(m ast.Node) bool {
					if id, ok := m.(*ast.Ident); ok {
						if o, ok := u.info.Uses[id].(*types.Var); ok && o.Pos() < x.Pos() && o.Parent() != nil && o.Parent() != o.Pkg().Scope() {
							captured[o] = true
						}
					}
					return true
				})
			}
			return true
		})
	}
	return
}

func pure(info *types.Info, e ast.Expr) bool {
	ok := true
	ast.Inspect(e, func(n ast.Node) bool {
		switch x := n.(type) {
		case *ast.CallExpr:
			if tv, has := info.Types[x.Fun]; !has || !tv.IsType() {
				ok = false
			}
		case *ast.UnaryExpr:
			if x.Op == token.ARROW {
				ok = false
			}
		case *ast.IndexExpr:
			// a map read can't change the tag, but an index can panic: still side-effect free for the tag's value
		}
		return ok
	})
	return ok
}

func isString(t types.Type) bool {
	b, ok := t.Underlying().(*types.Basic)
	return ok && b.Info()&types.IsString != 0
}

func census(u unit) {
	addr, captured := addrOrCaptured(u)
	for _, f := range u.files {
		fn := u.fset.Position(f.Pos()).Filename
		kind := "nontest"
		if strings.HasSuffix(fn, "_test.go") {
			kind = "test"
		}
		// item 2
		for _, d := range f.Decls {
			gd, ok := d.(*ast.GenDecl)
			if !ok || (gd.Tok != token.VAR && gd.Tok != token.CONST) {
				continue
			}
			for _, sp := range gd.Specs {
				vs := sp.(*ast.ValueSpec)
				spec2(u, vs, gd.Tok, "package", kind, addr, captured, f)
			}
		}
		ast.Inspect(f, func(n ast.Node) bool {
			switch x := n.(type) {
			case *ast.FuncDecl, *ast.FuncLit:
				var body *ast.BlockStmt
				if fd, ok := x.(*ast.FuncDecl); ok {
					body = fd.Body
				} else {
					body = x.(*ast.FuncLit).Body
				}
				if body == nil {
					return true
				}
				ast.Inspect(body, func(m ast.Node) bool {
					if ds, ok := m.(*ast.DeclStmt); ok {
						if gd, ok := ds.Decl.(*ast.GenDecl); ok && (gd.Tok == token.VAR || gd.Tok == token.CONST) {
							for _, sp := range gd.Specs {
								spec2(u, sp.(*ast.ValueSpec), gd.Tok, "local", kind, addr, captured, f)
							}
						}
					}
					if _, ok := m.(*ast.FuncLit); ok {
						return false // counted by its own visit
					}
					return true
				})
				return true
			case *ast.SwitchStmt:
				switch3(u, x, kind, addr, captured)
			case *ast.CompositeLit:
				lit4(u, x, kind)
			}
			return true
		})
	}
}

func spec2(u unit, vs *ast.ValueSpec, tok token.Token, scope, kind string, addr, captured map[types.Object]bool, f *ast.File) {
	if len(vs.Names) < 2 {
		return
	}
	tk := "var"
	if tok == token.CONST {
		tk = "const"
	}
	form := "no values"
	if len(vs.Values) == 1 && len(vs.Names) > 1 {
		form = "tuple (one multi-value)"
	} else if len(vs.Values) > 0 {
		form = "values"
	}
	base := fmt.Sprintf("2|%s|%s %s|%s", kind, scope, tk, form)
	add(base + "|specs")
	// blockers
	var blockers []string
	var first types.Type
	sameType, exportMix, blank, heap := true, false, false, false
	exported := -1
	for i, id := range vs.Names {
		if id.Name == "_" {
			blank = true
			continue
		}
		o := u.info.Defs[id]
		if o == nil {
			continue
		}
		if first == nil {
			first = o.Type()
		} else if !types.Identical(first, o.Type()) {
			sameType = false
		}
		e := 0
		if id.IsExported() {
			e = 1
		}
		if exported >= 0 && exported != e && scope == "package" {
			exportMix = true
		}
		exported = e
		if addr[o] || captured[o] {
			heap = true
		}
		_ = i
	}
	if form == "tuple (one multi-value)" {
		blockers = append(blockers, "tuple initializer (already one deconstruction)")
	}
	if !sameType {
		blockers = append(blockers, "names of different types")
	}
	if exportMix {
		blockers = append(blockers, "exported and unexported names")
	}
	if blank {
		blockers = append(blockers, "a blank name")
	}
	if heap {
		blockers = append(blockers, "address-taken or captured name (heap box)")
	}
	if tok == token.CONST && form == "values" && vs.Type == nil {
		blockers = append(blockers, "untyped const (each name's C# type is decided by its uses)")
	}
	// a comment inside the spec, after its first name (a per-name comment)
	for _, cg := range f.Comments {
		if cg.Pos() > vs.Names[0].End() && cg.End() < vs.End() {
			blockers = append(blockers, "a comment inside the spec")
			break
		}
	}
	if len(blockers) == 0 {
		add(base + "|mergeable")
		add(fmt.Sprintf("2|%s|%s %s|specs merged", kind, scope, tk))
		for range vs.Names {
			add(fmt.Sprintf("2|%s|%s %s|declarations saved", kind, scope, tk))
		}
		counts[fmt.Sprintf("2|%s|%s %s|declarations saved", kind, scope, tk)]--
	} else {
		for _, b := range blockers {
			add(base + "|blocked: " + b)
		}
	}
}

func switch3(u unit, s *ast.SwitchStmt, kind string, addr, captured map[types.Object]bool) {
	if s.Tag == nil {
		return
	}
	tagT := u.info.TypeOf(s.Tag)
	if tagT == nil {
		return
	}
	fall, constLabels, pureLabels := false, true, true
	for _, st := range s.Body.List {
		cc := st.(*ast.CaseClause)
		for _, e := range cc.List {
			if tv := u.info.Types[e]; tv.Value == nil {
				constLabels = false
			}
			if !pure(u.info, e) {
				pureLabels = false
			}
		}
		for _, b := range cc.Body {
			if br, ok := b.(*ast.BranchStmt); ok && br.Tok == token.FALLTHROUGH {
				fall = true
			}
		}
	}
	str := isString(tagT)
	if !(str || fall || !constLabels) {
		return // a constant switch: no tag temp
	}
	shape := "other"
	switch t := ast.Unparen(s.Tag).(type) {
	case *ast.Ident:
		o := u.info.Uses[t]
		if v, ok := o.(*types.Var); ok {
			if v.Parent() == v.Pkg().Scope() {
				shape = "identifier: package var"
			} else {
				shape = "identifier: local or parameter"
				if addr[o] {
					shape = "identifier: local, address-taken"
				} else if captured[o] {
					shape = "identifier: local, captured by a closure"
				}
			}
		} else if _, ok := o.(*types.Const); ok {
			shape = "identifier: constant"
		}
	case *ast.SelectorExpr:
		shape = "selector"
	case *ast.CallExpr:
		shape = "call"
	}
	why := "string tag"
	if fall {
		why = "fallthrough"
	} else if !str {
		why = "non-constant label"
	}
	add(fmt.Sprintf("3|%s|%s|%s", kind, why, shape))
	if shape == "identifier: local or parameter" {
		if pureLabels {
			add(fmt.Sprintf("3|%s|level 2 eligible (local, not addressed or captured, labels side-effect free)", kind))
		} else {
			add(fmt.Sprintf("3|%s|level 2 refused: a label calls or receives", kind))
		}
	}
}

func lit4(u unit, c *ast.CompositeLit, kind string) {
	t := u.info.TypeOf(c)
	if t == nil {
		return
	}
	_, isAlias := t.(*types.Alias)
	named := false
	under := types.Unalias(t)
	if _, ok := under.(*types.Named); ok {
		named = true
	}
	arr, ok := under.Underlying().(*types.Array)
	if !ok {
		return
	}
	form := "unkeyed, full length"
	keyed := false
	for _, e := range c.Elts {
		if _, ok := e.(*ast.KeyValueExpr); ok {
			keyed = true
		}
	}
	switch {
	case keyed:
		form = "keyed or sparse"
	case len(c.Elts) == 0:
		form = "empty (zero value)"
	case int64(len(c.Elts)) < arr.Len():
		form = "unkeyed, shorter than its length"
	}
	elem := "scalar or string element"
	switch arr.Elem().Underlying().(type) {
	case *types.Array:
		elem = "array element"
	case *types.Struct:
		elem = "struct element"
	case *types.Interface, *types.Pointer, *types.Slice, *types.Map, *types.Chan, *types.Signature:
		elem = "reference element"
	}
	typ := "unnamed"
	if named {
		typ = "named array type"
	} else if isAlias {
		typ = "alias"
	}
	if c.Type == nil {
		typ += " (elided, inside an outer literal)"
	}
	add(fmt.Sprintf("4|%s|%s|%s|%s", kind, typ, form, elem))
}

func main() {
	std := flag.Bool("std", false, "go/packages with tests")
	dirs := flag.Bool("dirs", false, "type-check each directory from source")
	flag.Parse()
	if *std {
		cfg := &packages.Config{Mode: packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo, Tests: true}
		pkgs, err := packages.Load(cfg, flag.Args()...)
		if err != nil {
			fmt.Fprintln(os.Stderr, err)
			os.Exit(2)
		}
		seen := map[string]bool{}
		for _, p := range pkgs {
			if p.TypesInfo == nil {
				continue
			}
			var files []*ast.File
			for _, f := range p.Syntax {
				fn := p.Fset.Position(f.Pos()).Filename
				if !seen[fn] {
					seen[fn] = true
					files = append(files, f)
				}
			}
			census(unit{p.Fset, p.TypesInfo, files})
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
					f, err := parser.ParseFile(fset, filepath.Join(path, e.Name()), nil, parser.ParseComments|parser.SkipObjectResolution)
					if err != nil {
						continue
					}
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
					census(unit{fset, info, files})
				}
				return nil
			})
		}
	}
	var ks []string
	for k := range counts {
		ks = append(ks, k)
	}
	sort.Strings(ks)
	for _, k := range ks {
		fmt.Printf("%6d  %s\n", counts[k], k)
	}
}
