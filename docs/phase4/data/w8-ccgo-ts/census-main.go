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
// WRITES through a number derived from __ccgo_ts, judged at the DESTINATION. An address is DERIVED when it is
// __ccgo_ts itself, a local in the set, either of those plus or minus an offset, or a conversion of one (uintptr(x),
// unsafe.Pointer(x)); a call result, a load, a field or an index never is. A local is in the MUST set when EVERY source
// it is given in the function is derived (greatest fixpoint; a parameter, a range variable, a declaration without a
// value, a multi-value assignment or an op= other than += and -= is an opaque source). Flagged, as the verdict:
//
//	store     *(*T)(unsafe.Pointer(e)) = / op= / ++ / -- whose destination e is derived under MUST
//	memwrite  a libc write primitive (memcpy, memmove, memset, strcpy, strncpy, strcat, strncat, sprintf, snprintf,
//	          vsnprintf; X-prefixed or not; a first tls argument skipped) whose DESTINATION is derived under MUST
//
// Reported apart and never as the verdict: store-may and memwrite-may, where the destination is derived only when a
// local counts with at least ONE derived source (least fixpoint). A __ccgo_ts number stored as a VALUE, or passed as
// a format or a source, is not a write through it.
// HEADER USES (COORD's D6 class): every reflect.SliceHeader and reflect.StringHeader the files name, as
// hdr-<Slice|String>-<field>-<read|write|addr> for the conversion (*reflect.XHeader)(p).Field, or hdr-<kind>-type for any
// other use of the name. The __ccgo_ts declarations' own StringHeader Data read is counted apart. Files that name
// neither __ccgo_ts nor "Header" are not parsed.
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

	headerClasses map[string]bool
}

func main() {
	flag.Parse()
	if flag.NArg() == 0 {
		fmt.Fprintln(os.Stderr, "usage: census [-tests] <dir>...")
		os.Exit(2)
	}
	st := &stats{counts: map[string]int{}, sites: map[string][]site{}, escapes: map[string]int{}, headerClasses: map[string]bool{}}
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
			names := strings.Contains(string(src), "__ccgo_ts")
			if !names && !strings.Contains(string(src), "Header") {
				return nil
			}
			if names {
				st.refFiles++
			}
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
	headerUses(fset, f, st)
	_ = name
}

// HEADER USES: every reflect.SliceHeader and reflect.StringHeader named in the file, classified as
// hdr-<Slice|String>-<field>-<read|write|addr> when it is the conversion (*reflect.XHeader)(p) followed by .Field, or
// hdr-<Slice|String>-type when the name is used any other way (a declared variable, a composite literal, a parameter).
// Every site but the __ccgo_ts declaration's own StringHeader Data read is printed.
func headerUses(fset *token.FileSet, f *ast.File, st *stats) {
	var stack []ast.Node
	ast.Inspect(f, func(n ast.Node) bool {
		if n == nil {
			stack = stack[:len(stack)-1]
			return true
		}
		stack = append(stack, n)
		sel, ok := n.(*ast.SelectorExpr)
		if !ok || !isIdent(sel.X, "reflect") || (sel.Sel.Name != "SliceHeader" && sel.Sel.Name != "StringHeader") {
			return true
		}
		kind := strings.TrimSuffix(sel.Sel.Name, "Header")
		class, at := "hdr-"+kind+"-type", ast.Node(sel)
		// Up from the name: ( * reflect.XHeader ) as a conversion's Fun, then ( ... ).Field, then its use.
		i := len(stack) - 2
		for i >= 0 {
			switch stack[i].(type) {
			case *ast.ParenExpr, *ast.StarExpr:
				i--
				continue
			}
			break
		}
		if i >= 0 {
			if call, ok := stack[i].(*ast.CallExpr); ok && len(call.Args) == 1 {
				j := i - 1
				for j >= 0 {
					if _, ok := stack[j].(*ast.ParenExpr); ok {
						j--
						continue
					}
					break
				}
				if j >= 0 {
					if fsel, ok := stack[j].(*ast.SelectorExpr); ok && unparen(fsel.X) == ast.Expr(call) {
						use := "read"
						if j >= 1 {
							switch u := stack[j-1].(type) {
							case *ast.AssignStmt:
								for _, l := range u.Lhs {
									if l == ast.Expr(fsel) {
										use = "write"
									}
								}
							case *ast.IncDecStmt:
								use = "write"
							case *ast.UnaryExpr:
								if u.Op == token.AND {
									use = "addr"
								}
							}
						}
						class = "hdr-" + kind + "-" + fsel.Sel.Name + "-" + use
						at = fsel
						if j >= 1 {
							at = stack[j-1]
						}
					}
				}
			}
		}
		if kind == "String" && class == "hdr-String-Data-read" && strings.Contains(render(fset, at), "__ccgo_ts1") {
			st.counts["hdr-String-Data-read-ccgo_ts-decl"]++
			return true
		}
		st.add(class, fset, sel.Pos(), at, "")
		st.headerClasses[class] = true
		return true
	})
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

	// Every source a local name is given in this function. nil means OPAQUE: a parameter, a range variable, a
	// declaration with no value, one value of a multi-value assignment, or an op= other than += and -=.
	sources := map[string][]ast.Expr{}
	opaque := func(name string) { sources[name] = append(sources[name], nil) }
	if fd.Recv != nil {
		for _, f := range fd.Recv.List {
			for _, nm := range f.Names {
				opaque(nm.Name)
			}
		}
	}
	for _, list := range []*ast.FieldList{fd.Type.Params, fd.Type.Results} {
		if list == nil {
			continue
		}
		for _, f := range list.List {
			for _, nm := range f.Names {
				opaque(nm.Name)
			}
		}
	}
	ast.Inspect(fd.Body, func(n ast.Node) bool {
		switch s := n.(type) {
		case *ast.AssignStmt:
			for i, lhs := range s.Lhs {
				id, ok := lhs.(*ast.Ident)
				if !ok || id.Name == "_" {
					continue
				}
				switch {
				case (s.Tok == token.ASSIGN || s.Tok == token.DEFINE) && len(s.Rhs) == len(s.Lhs):
					sources[id.Name] = append(sources[id.Name], s.Rhs[i])
				case s.Tok == token.ADD_ASSIGN && len(s.Rhs) == 1:
					sources[id.Name] = append(sources[id.Name], &ast.BinaryExpr{X: id, Op: token.ADD, Y: s.Rhs[0]})
				case s.Tok == token.SUB_ASSIGN && len(s.Rhs) == 1:
					sources[id.Name] = append(sources[id.Name], &ast.BinaryExpr{X: id, Op: token.SUB, Y: s.Rhs[0]})
				default:
					opaque(id.Name)
				}
			}
		case *ast.ValueSpec:
			for i, nm := range s.Names {
				if i < len(s.Values) && len(s.Values) == len(s.Names) {
					sources[nm.Name] = append(sources[nm.Name], s.Values[i])
				} else {
					opaque(nm.Name)
				}
			}
		case *ast.RangeStmt:
			for _, e := range []ast.Expr{s.Key, s.Value} {
				if id, ok := e.(*ast.Ident); ok {
					opaque(id.Name)
				}
			}
		}
		return true
	})

	// MUST: every source of the name is address arithmetic on __ccgo_ts (greatest fixpoint).
	must := map[string]bool{}
	for name, srcs := range sources {
		ok := true
		for _, e := range srcs {
			if e == nil {
				ok = false
				break
			}
		}
		if ok && len(srcs) > 0 {
			must[name] = true
		}
	}
	for changed := true; changed; {
		changed = false
		for name := range must {
			for _, e := range sources[name] {
				if !derived(e, must) {
					delete(must, name)
					changed = true
					break
				}
			}
		}
	}

	// MAY: at least one source is (least fixpoint). Reported apart, never as the verdict.
	may := map[string]bool{}
	for changed := true; changed; {
		changed = false
		for name, srcs := range sources {
			if may[name] {
				continue
			}
			for _, e := range srcs {
				if e != nil && derived(e, may) {
					may[name] = true
					changed = true
					break
				}
			}
		}
	}

	mark := func(class string, target ast.Expr, n ast.Node) {
		switch {
		case derived(target, must):
			st.add(class, fset, n.Pos(), n, "")
		case derived(target, may):
			st.add(class+"-may", fset, n.Pos(), n, "")
		}
	}

	ast.Inspect(fd.Body, func(n ast.Node) bool {
		switch s := n.(type) {
		case *ast.AssignStmt:
			for _, lhs := range s.Lhs {
				if t := derefTarget(lhs); t != nil {
					mark("store", t, s)
				}
			}
		case *ast.IncDecStmt:
			if t := derefTarget(s.X); t != nil {
				mark("store", t, s)
			}
		case *ast.CallExpr:
			name := calleeName(s.Fun)
			if writeFn.MatchString(name) {
				dst := 0
				if len(s.Args) > 0 && isIdent(s.Args[0], "tls") {
					dst = 1
				}
				if dst < len(s.Args) {
					mark("memwrite", s.Args[dst], s)
				}
				return true
			}
			for _, a := range s.Args {
				if derived(a, may) {
					st.escapes[name]++
					break
				}
			}
		}
		return true
	})
}

// An ADDRESS derived from __ccgo_ts: the identifier itself, a local in the set, that plus or minus an offset, or a
// conversion of one (uintptr(x), unsafe.Pointer(x)). A call result, a load, a field or an index is never derived.
func derived(e ast.Expr, set map[string]bool) bool {
	switch x := unparen(e).(type) {
	case *ast.Ident:
		return x.Name == "__ccgo_ts" || set[x.Name]
	case *ast.BinaryExpr:
		switch x.Op {
		case token.ADD:
			return derived(x.X, set) || derived(x.Y, set)
		case token.SUB:
			return derived(x.X, set)
		}
	case *ast.CallExpr:
		if len(x.Args) == 1 && isConversion(x.Fun) {
			return derived(x.Args[0], set)
		}
	}
	return false
}

func isConversion(fun ast.Expr) bool {
	switch f := unparen(fun).(type) {
	case *ast.Ident:
		switch f.Name {
		case "uintptr", "int", "int64", "uint", "uint64", "int32", "uint32":
			return true
		}
	case *ast.SelectorExpr:
		return isIdent(f.X, "unsafe") && f.Sel.Name == "Pointer"
	}
	return false
}

// *(*T)(unsafe.Pointer(x)) -> x
func derefTarget(e ast.Expr) ast.Expr {
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
	classes := []string{"decl", "decl-ref", "ts1-decl", "plusN", "value", "plusExpr", "compare", "subtract", "ts1use", "other", "store", "memwrite", "store-may", "memwrite-may", "parseerror"}
	for _, c := range classes {
		fmt.Printf("%-10s %d\n", c, st.counts[c])
	}
	for _, c := range []string{"decl", "plusExpr", "compare", "subtract", "ts1use", "other", "store", "memwrite", "store-may", "memwrite-may", "parseerror"} {
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
	fmt.Printf("\n== header uses: reflect.SliceHeader and reflect.StringHeader (the __ccgo_ts declarations' own Data read counted apart: %d)\n", st.counts["hdr-String-Data-read-ccgo_ts-decl"])
	var hc []string
	for c := range st.headerClasses {
		hc = append(hc, c)
	}
	sort.Strings(hc)
	if len(hc) == 0 {
		fmt.Println("   none")
	}
	for _, c := range hc {
		fmt.Printf("   %-28s %d\n", c, st.counts[c])
	}
	for _, c := range hc {
		ss := st.sites[c]
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
