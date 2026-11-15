// W8 census (kept as data beside docs/phase4's W8 design): how ccgo-generated Go uses __ccgo_ts, the number that names the base of a
// package's C string literals. Read-only: parses .go files (no type checking) under each directory given and prints
// a summary plus every site that is not plain "+N" arithmetic or a plain copy of the value.
//
// Usage: go run . <dir> [<dir>...]      (test files are skipped; -tests to include them)
//
// What it classifies, per use of the identifier __ccgo_ts (or __ccgo_ts1):
//
//	decl      the declaration itself (__ccgo_ts = (*reflect.StringHeader)(unsafe.Pointer(&__ccgo_ts1)).Data)
//	plusN     __ccgo_ts + <constant expression>
//	value     the bare value assigned to a variable or passed on (offset 0)
//	compare   ==, !=, <, <=, >, >= with __ccgo_ts on either side           (an identity use)
//	subtract  __ccgo_ts on either side of a binary -                         (an identity use)
//	ts1use    __ccgo_ts1 anywhere but the declaration's &__ccgo_ts1          (StringData, len, indexing, ...)
//	other     anything else, printed in full
//
// WRITES through a __ccgo_ts-derived number, flow-insensitively within one function: a variable is TAINTED when it
// is assigned an expression that mentions __ccgo_ts or a tainted variable (iterated to a fixpoint). Flagged:
//
//	store     *(*T)(unsafe.Pointer(e)) = / op= / ++ / -- where e mentions __ccgo_ts or a tainted variable
//	memwrite  a libc write primitive whose DESTINATION mentions one (memcpy, memmove, memset, strcpy, strncpy,
//	          strcat, strncat, sprintf, snprintf, vsnprintf; the X-prefixed libc spellings; tls first argument skipped)
//
// LIMITS: no inter-procedural flow (a tainted number passed into a function that writes through its parameter is
// counted under "escapes" by callee, not followed); no flow through struct fields, globals or memory.
package main

import (
	"flag"
	"fmt"
	"go/ast"
	"go/parser"
	"go/printer"
	"go/token"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
)

var (
	includeTests = flag.Bool("tests", false, "include _test.go files")
	maxPrint     = flag.Int("max", 40, "print at most this many sites per flagged class")
	writeFn      = regexp.MustCompile(`^X?(memcpy|memmove|memset|strcpy|strncpy|strcat|strncat|sprintf|snprintf|vsnprintf)$`)
)

type site struct {
	class string
	pos   string
	text  string
}

type stats struct {
	files    int
	refFiles int
	counts   map[string]int
	sites    map[string][]site
	escapes  map[string]int
}

func main() {
	flag.Parse()
	if flag.NArg() == 0 {
		fmt.Fprintln(os.Stderr, "usage: census [-tests] <dir>...")
		os.Exit(2)
	}
	st := &stats{counts: map[string]int{}, sites: map[string][]site{}, escapes: map[string]int{}}
	fset := token.NewFileSet()
	for _, root := range flag.Args() {
		err := filepath.Walk(root, func(path string, info os.FileInfo, err error) error {
			if err != nil {
				return err
			}
			if info.IsDir() || !strings.HasSuffix(path, ".go") || (!*includeTests && strings.HasSuffix(path, "_test.go")) {
				return nil
			}
			st.files++
			src, err := os.ReadFile(path)
			if err != nil {
				return err
			}
			if !strings.Contains(string(src), "__ccgo_ts") {
				return nil
			}
			st.refFiles++
			f, err := parser.ParseFile(fset, path, src, parser.SkipObjectResolution)
			if err != nil {
				st.add("parseerror", fset, token.NoPos, nil, err.Error())
				return nil
			}
			censusFile(fset, f, st, rel(root, path))
			return nil
		})
		if err != nil {
			fmt.Fprintln(os.Stderr, err)
			os.Exit(1)
		}
	}
	report(st)
}

func rel(root, path string) string {
	r, err := filepath.Rel(root, path)
	if err != nil {
		return path
	}
	return filepath.ToSlash(r)
}

func (st *stats) add(class string, fset *token.FileSet, pos token.Pos, n ast.Node, text string) {
	st.counts[class]++
	p := ""
	if pos.IsValid() {
		p = fset.Position(pos).String()
	}
	if text == "" && n != nil {
		text = render(fset, n)
	}
	st.sites[class] = append(st.sites[class], site{class, p, text})
}

func render(fset *token.FileSet, n ast.Node) string {
	var b strings.Builder
	_ = printer.Fprint(&b, fset, n)
	s := strings.Join(strings.Fields(b.String()), " ")
	if len(s) > 200 {
		s = s[:200] + "…"
	}
	return s
}

func isIdent(e ast.Expr, name string) bool {
	id, ok := e.(*ast.Ident)
	return ok && id.Name == name
}

func censusFile(fset *token.FileSet, f *ast.File, st *stats, name string) {
	// Uses of the identifiers, classified by their parent.
	var stack []ast.Node
	ast.Inspect(f, func(n ast.Node) bool {
		if n == nil {
			stack = stack[:len(stack)-1]
			return true
		}
		stack = append(stack, n)
		id, ok := n.(*ast.Ident)
		if !ok || (id.Name != "__ccgo_ts" && id.Name != "__ccgo_ts1") {
			return true
		}
		parent := ast.Node(nil)
		if len(stack) >= 2 {
			parent = stack[len(stack)-2]
		}
		classify(fset, st, id, parent, stack)
		return true
	})

	// Writes through tainted numbers, per function.
	for _, d := range f.Decls {
		fd, ok := d.(*ast.FuncDecl)
		if !ok || fd.Body == nil {
			continue
		}
		writesIn(fset, st, fd)
	}
	_ = name
}

func classify(fset *token.FileSet, st *stats, id *ast.Ident, parent ast.Node, stack []ast.Node) {
	if id.Name == "__ccgo_ts1" {
		// The declaration's own &__ccgo_ts1, or its var spec name.
		if u, ok := parent.(*ast.UnaryExpr); ok && u.Op == token.AND {
			st.add("decl-ref", fset, id.Pos(), parent, "")
			return
		}
		if _, ok := parent.(*ast.ValueSpec); ok {
			st.add("ts1-decl", fset, id.Pos(), parent, "")
			return
		}
		st.add("ts1use", fset, id.Pos(), parent, "")
		return
	}
	switch p := parent.(type) {
	case *ast.ValueSpec:
		for _, nm := range p.Names {
			if nm == id {
				st.add("decl", fset, id.Pos(), p, "")
				return
			}
		}
		st.add("value", fset, id.Pos(), p, "")
	case *ast.BinaryExpr:
		other := p.Y
		if p.Y == ast.Expr(id) {
			other = p.X
		}
		switch p.Op {
		case token.ADD:
			if isConst(other) {
				st.add("plusN", fset, id.Pos(), p, "")
			} else {
				st.add("plusExpr", fset, id.Pos(), p, "")
			}
		case token.SUB:
			st.add("subtract", fset, id.Pos(), p, "")
		case token.EQL, token.NEQ, token.LSS, token.LEQ, token.GTR, token.GEQ:
			st.add("compare", fset, id.Pos(), p, "")
		default:
			st.add("other", fset, id.Pos(), p, "")
		}
	case *ast.AssignStmt, *ast.ReturnStmt, *ast.CallExpr, *ast.CompositeLit, *ast.KeyValueExpr:
		st.add("value", fset, id.Pos(), p, "")
	default:
		st.add("other", fset, id.Pos(), parent, "")
	}
}

// A constant offset as ccgo writes it: an integer literal, possibly parenthesised or combined with other literals.
func isConst(e ast.Expr) bool {
	switch x := e.(type) {
	case *ast.BasicLit:
		return x.Kind == token.INT
	case *ast.ParenExpr:
		return isConst(x.X)
	case *ast.BinaryExpr:
		return isConst(x.X) && isConst(x.Y)
	case *ast.UnaryExpr:
		return isConst(x.X)
	}
	return false
}

func mentions(e ast.Node, tainted map[string]bool) bool {
	found := false
	ast.Inspect(e, func(n ast.Node) bool {
		if found {
			return false
		}
		if id, ok := n.(*ast.Ident); ok && (id.Name == "__ccgo_ts" || tainted[id.Name]) {
			found = true
		}
		return true
	})
	return found
}

func writesIn(fset *token.FileSet, st *stats, fd *ast.FuncDecl) {
	if !mentions(fd.Body, map[string]bool{}) {
		return
	}
	tainted := map[string]bool{}
	for changed := true; changed; {
		changed = false
		ast.Inspect(fd.Body, func(n ast.Node) bool {
			switch s := n.(type) {
			case *ast.AssignStmt:
				for i, lhs := range s.Lhs {
					id, ok := lhs.(*ast.Ident)
					if !ok || id.Name == "_" || tainted[id.Name] {
						continue
					}
					var rhs ast.Expr
					if len(s.Rhs) == len(s.Lhs) {
						rhs = s.Rhs[i]
					} else if len(s.Rhs) == 1 {
						rhs = s.Rhs[0]
					}
					if rhs != nil && mentions(rhs, tainted) {
						tainted[id.Name] = true
						changed = true
					}
				}
			case *ast.ValueSpec:
				for i, nm := range s.Names {
					if i < len(s.Values) && !tainted[nm.Name] && mentions(s.Values[i], tainted) {
						tainted[nm.Name] = true
						changed = true
					}
				}
			}
			return true
		})
	}

	derefTarget := func(e ast.Expr) ast.Expr {
		// *(*T)(unsafe.Pointer(x)) -> x
		star, ok := e.(*ast.StarExpr)
		if !ok {
			return nil
		}
		call, ok := unparen(star.X).(*ast.CallExpr)
		if !ok || len(call.Args) != 1 {
			return nil
		}
		inner, ok := unparen(call.Args[0]).(*ast.CallExpr)
		if !ok || len(inner.Args) != 1 {
			return nil
		}
		if sel, ok := inner.Fun.(*ast.SelectorExpr); ok && isIdent(sel.X, "unsafe") && sel.Sel.Name == "Pointer" {
			return inner.Args[0]
		}
		return nil
	}

	ast.Inspect(fd.Body, func(n ast.Node) bool {
		switch s := n.(type) {
		case *ast.AssignStmt:
			for _, lhs := range s.Lhs {
				if t := derefTarget(lhs); t != nil && mentions(t, tainted) {
					st.add("store", fset, s.Pos(), s, "")
				}
			}
		case *ast.IncDecStmt:
			if t := derefTarget(s.X); t != nil && mentions(t, tainted) {
				st.add("store", fset, s.Pos(), s, "")
			}
		case *ast.CallExpr:
			name := calleeName(s.Fun)
			if writeFn.MatchString(name) {
				dst := 0
				if len(s.Args) > 0 && isIdent(s.Args[0], "tls") {
					dst = 1
				}
				if dst < len(s.Args) && mentions(s.Args[dst], tainted) {
					st.add("memwrite", fset, s.Pos(), s, "")
				}
				return true
			}
			for _, a := range s.Args {
				if mentions(a, tainted) {
					st.escapes[name]++
					break
				}
			}
		}
		return true
	})
}

func unparen(e ast.Expr) ast.Expr {
	for {
		p, ok := e.(*ast.ParenExpr)
		if !ok {
			return e
		}
		e = p.X
	}
}

func calleeName(e ast.Expr) string {
	switch f := e.(type) {
	case *ast.Ident:
		return f.Name
	case *ast.SelectorExpr:
		return f.Sel.Name
	}
	return "?"
}

func report(st *stats) {
	fmt.Printf("files parsed %d; files naming __ccgo_ts %d\n", st.files, st.refFiles)
	classes := []string{"decl", "decl-ref", "ts1-decl", "plusN", "value", "plusExpr", "compare", "subtract", "ts1use", "other", "store", "memwrite", "parseerror"}
	for _, c := range classes {
		fmt.Printf("%-10s %d\n", c, st.counts[c])
	}
	for _, c := range []string{"decl", "plusExpr", "compare", "subtract", "ts1use", "other", "store", "memwrite", "parseerror"} {
		ss := st.sites[c]
		if len(ss) == 0 {
			continue
		}
		fmt.Printf("\n== %s (%d)\n", c, len(ss))
		for i, s := range ss {
			if i == *maxPrint {
				fmt.Printf("   ... %d more\n", len(ss)-i)
				break
			}
			fmt.Printf("   %s  %s\n", s.pos, s.text)
		}
	}
	type kv struct {
		k string
		v int
	}
	var es []kv
	for k, v := range st.escapes {
		es = append(es, kv{k, v})
	}
	sort.Slice(es, func(i, j int) bool { return es[i].v > es[j].v || (es[i].v == es[j].v && es[i].k < es[j].k) })
	fmt.Printf("\n== escapes: a tainted number passed to a call (not followed), by callee, top 25 of %d callees\n", len(es))
	for i, e := range es {
		if i == 25 {
			break
		}
		fmt.Printf("   %-32s %d\n", e.k, e.v)
	}
}
