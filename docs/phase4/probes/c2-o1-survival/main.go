// c2-o1-survival: the O1 SURVIVAL CENSUS for docs/phase4/DESIGN-string-literal-allocation.md (§8.2.3's
// "first census to run"; COORD's assignment after the owner's ruling on c555d91c55). A record-producing
// probe, not a gate.
//
// O1 retypes a string PARAMETER as golib's `sstring` (a ref struct view), so a literal argument binds
// with no copy. Go's escape analysis says which parameters Go keeps off the heap (`does not escape`);
// this program asks which of those a C# ref struct can actually take, by reading every use of the
// parameter in the callee's Go body against the C# filters, and then closing the set under onward
// passing (a parameter survives only if every parameter it is passed on to survives too).
//
//	GOOS=<os> go build -a -gcflags=-m std 2> m-<os>.txt                 (the same input as c2-escape-join)
//	GOOS=<os> go run . -m m-<os>.txt -core <repo>/src/core -params params-<os>.tsv > census-<os>.txt
//
// Every predicate is stated where it is computed, and main_test.go controls each class both ways on
// testdata/fixture.
package main

import (
	"bufio"
	"flag"
	"fmt"
	"go/ast"
	"go/constant"
	"go/token"
	"go/types"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"sort"
	"strings"

	"golang.org/x/tools/go/packages"
)

// ---- classes ----
//
// A class is DISQUALIFYING (the parameter cannot be an sstring without a copy or a compile error), a
// GAP (disqualifying only until golib's sstring gains the member: an enumerator, copy, the spread), or
// SAFE (recorded, not disqualifying). The prefix carries the kind.
const (
	// signature-level: the parameter's TYPE is fixed by something other than its own body
	cHandOwned  = "sig: hand-owned go2cs file (a converter change does not reach it)"
	cNoBody     = "sig: no Go body (assembly or linkname)"
	cFuncValue  = "sig: the function is used as a value in production (delegate signature)"
	cFuncValueT = "sig: the function is used as a value in test files only (e.g. export_test.go)"
	cIfaceMatch = "sig: method matching an interface method (binder: exact parameter types)"
	cReflect    = "sig: exported method (reflection reach)"

	// use-level, disqualifying
	cCapture     = "use: captured by a closure"
	cDeferGo     = "use: argument of defer/go (captured by the lowering)"
	cBoxIface    = "use: converted to an interface (box)"
	cGeneric     = "use: argument of a generic function (type argument)"
	cStoreField  = "use: stored in a struct field"
	cStoreGlobal = "use: stored in a package-level variable"
	cStoreElem   = "use: stored in a slice, array or map element, or appended as an element"
	cStoreComp   = "use: stored in a composite literal"
	cStoreChan   = "use: sent on a channel"
	cStorePtr    = "use: stored through a pointer"
	cMapKey      = "use: map key (no sstring map indexer)"
	cReturn      = "use: returned"
	cAddr        = "use: address taken"
	cVariadic    = "use: passed in a ...string tail"
	cFixedSig    = "use: passed to a fixed signature (func value, interface method)"
	cNamedConv   = "use: converted to a named string type"
	cOther       = "use: other"

	// use-level, golib gaps
	gRange  = "gap: range over the string (sstring has no enumerator)"
	gCopy   = "gap: copy(dst, s) (sstring has no copy)"
	gSpread = "gap: append(b, s...) (sstring has no spread)"

	// use-level, safe
	sLen       = "safe: len"
	sIndex     = "safe: index s[i]"
	sCompare   = "safe: comparison or switch"
	sConcat    = "safe: concatenation (sstring's + is uncounted until V-fix 11)"
	sBytes     = "safe: []byte(s) or []rune(s) (copies, as Go does)"
	sAlias     = "safe: bound to a local (tracked as an alias)"
	sReassign  = "safe: the parameter is reassigned"
	sDiscard   = "safe: assigned to _"
	sOnward    = "onward: passed to a string parameter (resolved by the fixed point)"
	cCascade   = "cascade: passed to a parameter that does not survive"
	cVerdict   = "go: the parameter's -m verdict is not noescape"
	cNoVerdict = "go: -m printed no verdict for the parameter"
)

// A mode is one fixed point: S0 today; S1 with sstring's golib gaps closed; S2 as S1, plus an adapter
// lambda the converter would emit at every site that takes the function as a value (so the value keeps
// the @string signature and forwards to the sstring one: a view, no copy).
type mode struct{ gapsClosed, adapters bool }

var modes = []mode{{false, false}, {true, false}, {true, true}}

func disqualifying(c string, m mode) bool {
	switch {
	case c == cFuncValue || c == cFuncValueT:
		return !m.adapters
	case strings.HasPrefix(c, "sig:"), strings.HasPrefix(c, "use:"), strings.HasPrefix(c, "go:"), c == cCascade:
		return true
	case strings.HasPrefix(c, "gap:"):
		return !m.gapsClosed
	}
	return false
}

// precedence for the "first reason" column: signature, then Go's verdict, then direct uses, then gaps,
// then the cascade.
var precedence = []string{
	cHandOwned, cNoBody, cFuncValue, cFuncValueT, cIfaceMatch, cReflect, cVerdict, cNoVerdict,
	cBoxIface, cStoreField, cStoreGlobal, cStoreElem, cStoreComp, cStoreChan, cStorePtr, cReturn,
	cCapture, cDeferGo, cAddr, cGeneric, cMapKey, cVariadic, cFixedSig, cNamedConv, cOther,
	gRange, gCopy, gSpread, cCascade,
}

// ---- data ----

type param struct {
	key      string // declaration position of the parameter (the -m key), GOROOT-relative in output
	fn       string // pkgpath.Func or pkgpath.(T).M
	pkg      string
	idx      int
	verdict  string
	classes  map[string]int
	edges    map[string]bool // onward string parameters
	exported bool            // the function is exported (public API; not a filter)
	unexpFmt bool            // an unexported function of package fmt
	lit      int             // production literal argument sites (noescape join), non-format
	litFmt   int             // ... format position
	litLeak  int             // production literal argument sites whose verdict is not noescape
	sb       int             // production string(b) argument sites
	survive  [3]bool         // per mode: S0, S1, S2
	culprit  [3]string       // the first onward parameter that does not survive (its function)
	culpritK [3]string       // ... its key
}

type census struct {
	fset      *token.FileSet
	goroot    string
	core      string
	goos      string
	param     map[string]string // -m parameter verdicts by position
	params    map[string]*param
	funcValue map[string]string // function declaration position -> "prod" or "test" (where it is used as a value)
	ifaceSigs map[string]bool   // name|signature of every interface method
	handOwned map[string]bool   // Go file -> its go2cs file is a whole-file hand-own
	counts    map[string]int
	// function declaration position -> the value sites, GOROOT-relative (for the parameter TSV)
	funcValueAt map[string][]string
	fnKey       map[string]string // parameter key -> its function's declaration position
}

func (c *census) pos(x token.Pos) string {
	p := c.fset.Position(x)
	return fmt.Sprintf("%s:%d:%d", p.Filename, p.Line, p.Column)
}

// rel makes a position GOROOT-relative; a file outside GOROOT/src (a cgo-generated file in the build
// cache) keeps only its base name, so no machine path reaches a committed output.
func (c *census) rel(s string) string {
	if c.goroot != "" {
		if r, ok := strings.CutPrefix(s, filepath.ToSlash(c.goroot)+"/src/"); ok {
			return r
		}
	}
	if filepath.IsAbs(s) {
		return "<outside GOROOT>/" + filepath.Base(s)
	}
	return s
}

func isString(t types.Type) bool {
	if t == nil {
		return false
	}
	b, ok := t.Underlying().(*types.Basic)
	return ok && b.Info()&types.IsString != 0
}

// the predeclared string (or an alias of it), as opposed to a named string type
func isPredeclString(t types.Type) bool {
	b, ok := types.Unalias(t).(*types.Basic)
	return ok && b.Info()&types.IsString != 0
}

func sigKey(name string, sig *types.Signature) string {
	var b strings.Builder
	b.WriteString(name + "(")
	for i := 0; i < sig.Params().Len(); i++ {
		b.WriteString(types.TypeString(sig.Params().At(i).Type(), nil) + ",")
	}
	b.WriteString(")(")
	for i := 0; i < sig.Results().Len(); i++ {
		b.WriteString(types.TypeString(sig.Results().At(i).Type(), nil) + ",")
	}
	fmt.Fprintf(&b, ")%v", sig.Variadic())
	return b.String()
}

// Copies of the converter's own predicate (src/go2cs/hoistedLiteralOperations.go), as in c2-escape-join,
// so "format position" means exactly what the join and arm B mean.
func callFuncNameEndsWithF(fun ast.Expr) bool {
	switch f := fun.(type) {
	case *ast.Ident:
		return strings.HasSuffix(f.Name, "f")
	case *ast.SelectorExpr:
		return strings.HasSuffix(f.Sel.Name, "f")
	case *ast.IndexExpr:
		return callFuncNameEndsWithF(f.X)
	case *ast.IndexListExpr:
		return callFuncNameEndsWithF(f.X)
	case *ast.ParenExpr:
		return callFuncNameEndsWithF(f.X)
	}
	return false
}

// staticCallee is the declared function a call binds to, or nil for a func value, a conversion or a
// builtin. A generic instantiation returns the instance (callee.Origin() != callee).
func staticCallee(info *types.Info, fun ast.Expr) *types.Func {
	switch f := ast.Unparen(fun).(type) {
	case *ast.Ident:
		fn, _ := info.Uses[f].(*types.Func)
		return fn
	case *ast.SelectorExpr:
		fn, _ := info.Uses[f.Sel].(*types.Func)
		return fn
	case *ast.IndexExpr:
		return staticCallee(info, f.X)
	case *ast.IndexListExpr:
		return staticCallee(info, f.X)
	}
	return nil
}

func isIfaceMethod(fn *types.Func) bool {
	sig := fn.Type().(*types.Signature)
	return sig.Recv() != nil && types.IsInterface(sig.Recv().Type())
}

func isGeneric(fn *types.Func) bool {
	sig := fn.Type().(*types.Signature)
	if fn.Origin() != fn || sig.TypeParams().Len() > 0 {
		return true
	}
	if r := sig.Recv(); r != nil {
		t := r.Type()
		if p, ok := t.(*types.Pointer); ok {
			t = p.Elem()
		}
		if n, ok := types.Unalias(t).(*types.Named); ok && n.TypeParams().Len() > 0 {
			return true
		}
	}
	return false
}

func main() {
	mFile := flag.String("m", "", "the -gcflags=-m output for the same GOOS")
	core := flag.String("core", "", "the repository's src/core, for the hand-owned check (optional)")
	paramsOut := flag.String("params", "", "write one TSV row per reached parameter here (optional)")
	flag.Parse()
	patterns := flag.Args()
	if len(patterns) == 0 {
		patterns = []string{"std"}
	}
	c, err := run(*mFile, *core, patterns)
	if err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(2)
	}
	c.report(os.Stdout)
	if *paramsOut != "" {
		if err := c.writeParams(*paramsOut); err != nil {
			fmt.Fprintln(os.Stderr, err)
			os.Exit(2)
		}
	}
}

func run(mFile, core string, patterns []string) (*census, error) {
	c := &census{
		core: core, goos: os.Getenv("GOOS"),
		param: map[string]string{}, params: map[string]*param{}, funcValue: map[string]string{},
		ifaceSigs: map[string]bool{}, handOwned: map[string]bool{}, counts: map[string]int{},
		funcValueAt: map[string][]string{}, fnKey: map[string]string{},
	}
	if c.goos == "" {
		c.goos = "linux"
	}
	if err := c.readVerdicts(mFile); err != nil {
		return nil, err
	}
	cfg := &packages.Config{Mode: packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo, Tests: true}
	pkgs, err := packages.Load(cfg, patterns...)
	if err != nil {
		return nil, err
	}
	if len(pkgs) > 0 {
		c.fset = pkgs[0].Fset
	}
	if out, err := goEnvGOROOT(); err == nil {
		c.goroot = out
	}

	type file struct {
		f    *ast.File
		info *types.Info
		pkg  *packages.Package
		name string
		test bool
	}
	var files []file
	seen := map[string]bool{}
	for _, p := range pkgs {
		if p.TypesInfo == nil {
			continue
		}
		for _, f := range p.Syntax {
			name := c.fset.Position(f.Pos()).Filename
			if seen[name] {
				continue
			}
			seen[name] = true
			files = append(files, file{f, p.TypesInfo, p, name, strings.HasSuffix(name, "_test.go")})
		}
	}

	// pass 1: every interface method's name and signature (the binder matches on exact types), and every
	// function used as a value anywhere, tests included (a test's func value fixes the signature too)
	for _, fl := range files {
		for _, tv := range fl.info.Types {
			if it, ok := tv.Type.Underlying().(*types.Interface); ok {
				for i := 0; i < it.NumMethods(); i++ {
					m := it.Method(i)
					c.ifaceSigs[sigKey(m.Name(), m.Type().(*types.Signature))] = true
				}
			}
		}
		for _, obj := range fl.info.Defs {
			if tn, ok := obj.(*types.TypeName); ok {
				if it, ok := tn.Type().Underlying().(*types.Interface); ok {
					for i := 0; i < it.NumMethods(); i++ {
						m := it.Method(i)
						c.ifaceSigs[sigKey(m.Name(), m.Type().(*types.Signature))] = true
					}
				}
			}
		}
		var stack []ast.Node
		ast.Inspect(fl.f, func(n ast.Node) bool {
			if n == nil {
				stack = stack[:len(stack)-1]
				return true
			}
			defer func() { stack = append(stack, n) }()
			id, ok := n.(*ast.Ident)
			if !ok {
				return true
			}
			fn, ok := fl.info.Uses[id].(*types.Func)
			if !ok || isIfaceMethod(fn) {
				return true
			}
			// climb from the use to the expression a call would have as its Fun
			var e ast.Expr = id
			i := len(stack) - 1
			for i >= 0 {
				switch p := stack[i].(type) {
				case *ast.SelectorExpr:
					if p.Sel == e {
						e = p
						i--
						continue
					}
				case *ast.IndexExpr:
					if p.X == e {
						e = p
						i--
						continue
					}
				case *ast.IndexListExpr:
					if p.X == e {
						e = p
						i--
						continue
					}
				case *ast.ParenExpr:
					e = p
					i--
					continue
				}
				break
			}
			if i >= 0 {
				if call, ok := stack[i].(*ast.CallExpr); ok && call.Fun == e {
					return true
				}
			}
			k := c.pos(fn.Origin().Pos())
			if !fl.test || c.funcValue[k] == "" {
				c.funcValue[k] = map[bool]string{false: "prod", true: "test"}[fl.test]
			}
			c.funcValueAt[k] = append(c.funcValueAt[k], c.rel(c.pos(id.Pos())))
			return true
		})
	}

	// pass 2: every production function's string parameters, classified by their uses
	for _, fl := range files {
		if fl.test {
			continue
		}
		for _, d := range fl.f.Decls {
			fd, ok := d.(*ast.FuncDecl)
			if !ok {
				continue
			}
			fn, ok := fl.info.Defs[fd.Name].(*types.Func)
			if !ok {
				continue
			}
			c.analyzeFunc(fl.info, fl.pkg, fl.name, fd, fn)
		}
	}

	// pass 3: the call sites (production only): literal arguments and string(b) arguments; and every
	// other production string literal, by what consumes it (R2)
	for _, fl := range files {
		if fl.test {
			continue
		}
		var stack []ast.Node
		ast.Inspect(fl.f, func(n ast.Node) bool {
			if n == nil {
				stack = stack[:len(stack)-1]
				return true
			}
			defer func() { stack = append(stack, n) }()
			switch x := n.(type) {
			case *ast.CallExpr:
				c.callSite(fl.info, x)
			case *ast.BasicLit:
				if x.Kind == token.STRING {
					c.counts["R2 "+literalContext(fl.info, x, stack)]++
				}
			}
			return true
		})
	}

	for slot, m := range modes {
		c.fixedPoint(slot, m)
	}
	return c, nil
}

func goEnvGOROOT() (string, error) {
	// the GOROOT go/packages used is the one on PATH; ask it rather than guess
	out, err := execOutput("go", "env", "GOROOT")
	return strings.TrimSpace(out), err
}

func (c *census) readVerdicts(mFile string) error {
	if mFile == "" {
		return nil
	}
	fh, err := os.Open(mFile)
	if err != nil {
		return err
	}
	defer fh.Close()
	// the same parameter predicate as c2-escape-join: `leaking param: s` (to heap, or `to result`) or
	// `s does not escape`, keyed by the parameter's declaration position
	re := regexp.MustCompile(`^(.*\.go):(\d+):(\d+): (?:leaking param: (\w+)(.*)|(\w+) does not escape)$`)
	sc := bufio.NewScanner(fh)
	sc.Buffer(make([]byte, 1<<20), 1<<24)
	for sc.Scan() {
		m := re.FindStringSubmatch(sc.Text())
		if m == nil {
			continue
		}
		file := m[1]
		if !filepath.IsAbs(file) { // a package in the current module prints relative paths
			if abs, err := filepath.Abs(file); err == nil {
				file = abs
			}
		}
		k := file + ":" + m[2] + ":" + m[3]
		switch {
		case m[6] != "":
			if _, ok := c.param[k]; !ok {
				c.param[k] = "noescape"
			}
		case strings.Contains(m[5], "to result"):
			if c.param[k] != "leak-heap" {
				c.param[k] = "leak-result"
			}
		default:
			c.param[k] = "leak-heap"
		}
	}
	return sc.Err()
}

func (c *census) isHandOwned(goFile, pkgPath string) bool {
	if pkgPath == "unsafe" || pkgPath == "testing" {
		return true
	}
	if c.core == "" {
		return false
	}
	if v, ok := c.handOwned[goFile]; ok {
		return v
	}
	base := strings.TrimSuffix(filepath.Base(goFile), ".go") + ".cs"
	dir := filepath.Join(c.core, filepath.FromSlash(pkgPath))
	v := false
	for _, cand := range []string{filepath.Join(dir, base), filepath.Join(dir, c.goos, base)} {
		if b, err := os.ReadFile(cand); err == nil {
			v = strings.Contains(string(b), "GoManualConversion")
			break
		}
	}
	c.handOwned[goFile] = v
	return v
}

func (c *census) analyzeFunc(info *types.Info, pkg *packages.Package, file string, fd *ast.FuncDecl, fn *types.Func) {
	sig := fn.Type().(*types.Signature)
	name := pkg.PkgPath + "." + fn.Name()
	if r := sig.Recv(); r != nil {
		t := r.Type()
		if p, ok := t.(*types.Pointer); ok {
			t = p.Elem()
		}
		if n, ok := types.Unalias(t).(*types.Named); ok {
			name = pkg.PkgPath + ".(" + n.Obj().Name() + ")." + fn.Name()
		}
	}
	results := map[types.Object]bool{}
	for i := 0; i < sig.Results().Len(); i++ {
		results[sig.Results().At(i)] = true
	}
	var sigClasses []string
	if c.isHandOwned(file, pkg.PkgPath) {
		sigClasses = append(sigClasses, cHandOwned)
	}
	if fd.Body == nil {
		sigClasses = append(sigClasses, cNoBody)
	}
	switch c.funcValue[c.pos(fn.Pos())] {
	case "prod":
		sigClasses = append(sigClasses, cFuncValue)
	case "test":
		sigClasses = append(sigClasses, cFuncValueT)
	}
	if sig.Recv() != nil {
		if c.ifaceSigs[sigKey(fn.Name(), sig)] {
			sigClasses = append(sigClasses, cIfaceMatch)
		}
		if fn.Exported() {
			sigClasses = append(sigClasses, cReflect)
		}
	}
	for i := 0; i < sig.Params().Len(); i++ {
		v := sig.Params().At(i)
		if !isString(v.Type()) || (sig.Variadic() && i == sig.Params().Len()-1) {
			continue
		}
		key := c.pos(v.Pos())
		p := &param{key: key, fn: name, pkg: pkg.PkgPath, idx: i, classes: map[string]int{}, edges: map[string]bool{},
			exported: fn.Exported(), unexpFmt: pkg.PkgPath == "fmt" && !fn.Exported()}
		p.verdict = c.param[key]
		if p.verdict == "" {
			p.verdict = "no-verdict"
		}
		for _, sc := range sigClasses {
			p.classes[sc]++
		}
		if !isPredeclString(v.Type()) {
			p.classes[cNamedConv]++ // a named string parameter is a struct over @string in go2cs, not O1's shape
		}
		if fd.Body != nil {
			c.uses(info, fd.Body, v, results, p)
		}
		c.params[key] = p
		c.fnKey[key] = c.pos(fn.Pos())
	}
}

// uses classifies every use of v, and of every local it is bound to, in body.
func (c *census) uses(info *types.Info, body *ast.BlockStmt, v *types.Var, results map[types.Object]bool, p *param) {
	aliases := map[types.Object]bool{v: true}
	work := []types.Object{v}
	for len(work) > 0 {
		a := work[0]
		work = work[1:]
		var stack []ast.Node
		ast.Inspect(body, func(n ast.Node) bool {
			if n == nil {
				stack = stack[:len(stack)-1]
				return true
			}
			defer func() { stack = append(stack, n) }()
			id, ok := n.(*ast.Ident)
			if !ok || info.Uses[id] != a {
				return true
			}
			for _, anc := range stack {
				switch anc.(type) {
				case *ast.FuncLit:
					p.classes[cCapture]++
				case *ast.DeferStmt, *ast.GoStmt:
					p.classes[cDeferGo]++
				}
			}
			for _, cl := range c.context(info, id, stack, results, func(o types.Object) {
				if !aliases[o] {
					aliases[o] = true
					work = append(work, o)
				}
			}, p) {
				p.classes[cl]++
			}
			return true
		})
	}
}

// context classifies one use (an identifier whose ancestors are stack) by what consumes it.
func (c *census) context(info *types.Info, id *ast.Ident, stack []ast.Node, results map[types.Object]bool, alias func(types.Object), p *param) []string {
	var cur ast.Expr = id
	for i := len(stack) - 1; i >= 0; i-- {
		switch par := stack[i].(type) {
		case *ast.ParenExpr:
			cur = par
			continue
		case *ast.SliceExpr:
			if par.X == cur { // s[i:j] is still a view of s
				cur = par
				continue
			}
			return []string{cOther}
		case *ast.IndexExpr:
			if par.X == cur {
				return []string{sIndex}
			}
			if _, ok := info.TypeOf(par.X).Underlying().(*types.Map); ok {
				return []string{cMapKey}
			}
			return []string{cOther}
		case *ast.UnaryExpr:
			if par.Op == token.AND {
				return []string{cAddr}
			}
			return []string{cOther}
		case *ast.BinaryExpr:
			switch par.Op {
			case token.EQL, token.NEQ, token.LSS, token.LEQ, token.GTR, token.GEQ:
				return []string{sCompare}
			case token.ADD:
				return []string{sConcat}
			}
			return []string{cOther}
		case *ast.CallExpr:
			if par.Fun == cur {
				return []string{cOther}
			}
			idx := -1
			for k, a := range par.Args {
				if a == cur {
					idx = k
				}
			}
			cl, conv := c.call(info, par, idx, p)
			if conv { // string(s): the same value
				cur = par
				continue
			}
			return []string{cl}
		case *ast.AssignStmt:
			for _, l := range par.Lhs {
				if l == cur {
					if par.Tok == token.ADD_ASSIGN {
						return []string{sConcat}
					}
					return []string{sReassign}
				}
			}
			if par.Tok == token.ADD_ASSIGN {
				return []string{sConcat}
			}
			for k, r := range par.Rhs {
				if r == cur && len(par.Lhs) == len(par.Rhs) {
					return []string{c.lhs(info, par.Lhs[k], results, alias)}
				}
			}
			return []string{cOther}
		case *ast.ValueSpec:
			for k, val := range par.Values {
				if val == cur && k < len(par.Names) {
					return []string{c.lhs(info, par.Names[k], results, alias)}
				}
			}
			return []string{cOther}
		case *ast.ReturnStmt:
			return []string{cReturn}
		case *ast.CompositeLit, *ast.KeyValueExpr:
			return []string{cStoreComp}
		case *ast.SendStmt:
			return []string{cStoreChan}
		case *ast.RangeStmt:
			if par.X == cur {
				return []string{gRange}
			}
			return []string{cOther}
		case *ast.SwitchStmt, *ast.CaseClause:
			return []string{sCompare} // lowered to a temp and comparisons (e.g. net/http/server.cs:3671)
		case *ast.StarExpr:
			return []string{cOther}
		}
		return []string{cOther + " (" + strings.TrimPrefix(fmt.Sprintf("%T", stack[i]), "*ast.") + ")"}
	}
	return []string{cOther}
}

// call classifies argument idx of call; conv is true for an identity string conversion.
func (c *census) call(info *types.Info, call *ast.CallExpr, idx int, p *param) (string, bool) {
	tv := info.Types[call.Fun]
	if tv.IsType() {
		t := tv.Type
		switch {
		case types.IsInterface(t):
			return cBoxIface, false
		case isPredeclString(t):
			return "", true
		case isString(t):
			return cNamedConv, false
		}
		if s, ok := t.Underlying().(*types.Slice); ok {
			if b, ok := s.Elem().Underlying().(*types.Basic); ok && (b.Kind() == types.Uint8 || b.Kind() == types.Int32) {
				return sBytes, false
			}
		}
		return cOther, false
	}
	if tv.IsBuiltin() {
		name := ""
		if id, ok := ast.Unparen(call.Fun).(*ast.Ident); ok {
			name = id.Name
		}
		switch name {
		case "len", "cap":
			return sLen, false
		case "copy":
			return gCopy, false
		case "append":
			if call.Ellipsis.IsValid() && idx == 1 {
				return gSpread, false
			}
			return cStoreElem, false
		case "delete":
			return cMapKey, false
		case "print", "println", "panic":
			return cBoxIface, false
		case "min", "max":
			return sCompare, false
		}
		return cOther, false
	}
	fn := staticCallee(info, call.Fun)
	if fn == nil {
		return cFixedSig, false
	}
	if isIfaceMethod(fn) {
		return cFixedSig, false
	}
	if isGeneric(fn) {
		return cGeneric, false
	}
	sig, _ := info.TypeOf(call.Fun).Underlying().(*types.Signature)
	if sig == nil {
		return cOther, false
	}
	n := sig.Params().Len()
	if sig.Variadic() && idx >= n-1 {
		if call.Ellipsis.IsValid() {
			return cOther, false
		}
		elem := sig.Params().At(n - 1).Type().(*types.Slice).Elem()
		if types.IsInterface(elem) {
			return cBoxIface, false
		}
		if isString(elem) {
			return cVariadic, false
		}
		return cOther, false
	}
	if idx < 0 || idx >= n {
		return cOther, false
	}
	q := sig.Params().At(idx)
	if types.IsInterface(q.Type()) {
		return cBoxIface, false
	}
	if isString(q.Type()) {
		p.edges[c.pos(fn.Origin().Type().(*types.Signature).Params().At(idx).Pos())] = true
		return sOnward, false
	}
	return cOther, false
}

func (c *census) lhs(info *types.Info, l ast.Expr, results map[types.Object]bool, alias func(types.Object)) string {
	switch x := ast.Unparen(l).(type) {
	case *ast.Ident:
		if x.Name == "_" {
			return sDiscard
		}
		obj := info.Defs[x]
		if obj == nil {
			obj = info.Uses[x]
		}
		v, ok := obj.(*types.Var)
		if !ok {
			return cOther
		}
		switch {
		case v.Parent() != nil && v.Parent() == v.Pkg().Scope():
			return cStoreGlobal
		case results[v]:
			return cReturn // a named result
		case types.IsInterface(v.Type()):
			return cBoxIface
		case isString(v.Type()):
			alias(v)
			return sAlias
		}
		return cOther
	case *ast.SelectorExpr:
		if _, ok := info.Uses[x.Sel].(*types.Var); ok {
			if sel := info.Selections[x]; sel == nil {
				return cStoreGlobal // pkg.Var
			}
		}
		return cStoreField
	case *ast.IndexExpr:
		return cStoreElem
	case *ast.StarExpr:
		return cStorePtr
	}
	return cOther
}

// literalContext names what consumes a production string literal (R2). Constants, import paths and
// struct tags are not string VALUES at run time and are named so they can be set aside.
func literalContext(info *types.Info, lit *ast.BasicLit, stack []ast.Node) string {
	for _, anc := range stack {
		if gd, ok := anc.(*ast.GenDecl); ok && gd.Tok == token.CONST {
			if _, ok := stack[len(stack)-1].(*ast.CallExpr); ok {
				return "set aside: in a const declaration (a call argument, e.g. len(\"…\"))"
			}
			return "set aside: in a const declaration"
		}
	}
	var cur ast.Node = lit
	for i := len(stack) - 1; i >= 0; i-- {
		switch par := stack[i].(type) {
		case *ast.ParenExpr:
			cur = par
			continue
		case *ast.ImportSpec:
			return "set aside: import path"
		case *ast.Field:
			return "set aside: struct tag"
		case *ast.CallExpr:
			if tv, ok := info.Types[par.Fun]; ok && tv.IsType() {
				return "a conversion operand"
			}
			return "a call argument (population P)"
		case *ast.ReturnStmt:
			return "returned"
		case *ast.CompositeLit, *ast.KeyValueExpr:
			return "a composite literal element (stored)"
		case *ast.AssignStmt, *ast.ValueSpec:
			return "assigned to a variable"
		case *ast.BinaryExpr:
			switch par.Op {
			case token.ADD:
				return "a concatenation operand"
			}
			return "a comparison operand"
		case *ast.CaseClause, *ast.SwitchStmt:
			return "a switch case"
		case *ast.IndexExpr:
			if par.Index == cur {
				return "a map key"
			}
			return "indexed"
		case *ast.SliceExpr:
			return "sliced"
		case *ast.SendStmt:
			return "sent on a channel"
		}
		_ = cur
		return "other (" + strings.TrimPrefix(fmt.Sprintf("%T", stack[i]), "*ast.") + ")"
	}
	return "other"
}

// callSite joins one production call's literal and string(b) arguments to the callee's parameter, with
// the same exclusions as c2-escape-join's J rows (no callee declaration, interface method, generic
// instantiation, variadic tail).
func (c *census) callSite(info *types.Info, call *ast.CallExpr) {
	sig, _ := info.TypeOf(call.Fun).(*types.Signature)
	if sig == nil {
		if t := info.TypeOf(call.Fun); t != nil {
			sig, _ = t.Underlying().(*types.Signature)
		}
	}
	if sig == nil {
		for _, arg := range call.Args {
			if lit, ok := ast.Unparen(arg).(*ast.BasicLit); ok && lit.Kind == token.STRING {
				kind := "a conversion"
				if tv, ok := info.Types[call.Fun]; ok && tv.IsBuiltin() {
					kind = "a builtin"
				} else if !ok || !tv.IsType() {
					kind = "no recorded signature"
				}
				c.counts["P lit EXCLUDED: argument of "+kind]++
			}
		}
		return
	}
	fn := staticCallee(info, call.Fun)
	params := sig.Params()
	for idx, arg := range call.Args {
		kind := ""
		if lit, ok := ast.Unparen(arg).(*ast.BasicLit); ok && lit.Kind == token.STRING && info.Types[lit].Value != nil && info.Types[lit].Value.Kind() == constant.String {
			kind = "lit"
		} else if sb, ok := ast.Unparen(arg).(*ast.CallExpr); ok && len(sb.Args) == 1 {
			if tv, ok := info.Types[sb.Fun]; ok && tv.IsType() && isString(tv.Type) {
				if s, ok := info.TypeOf(sb.Args[0]).Underlying().(*types.Slice); ok {
					if b, ok := s.Elem().Underlying().(*types.Basic); ok && b.Kind() == types.Uint8 {
						kind = "sb"
					}
				}
			}
		}
		if kind == "" {
			continue
		}
		isFormat := kind == "lit" && sig.Variadic() && params.Len() >= 2 && idx == params.Len()-2 && callFuncNameEndsWithF(call.Fun) && isString(params.At(idx).Type())
		pre := "P " + kind + " "
		switch {
		case fn == nil:
			c.counts[pre+"EXCLUDED: no callee declaration"]++
			continue
		case isIfaceMethod(fn):
			c.counts[pre+"EXCLUDED: interface method"]++
			continue
		case fn.Origin() != fn:
			c.counts[pre+"EXCLUDED: generic instantiation"]++
			continue
		case sig.Variadic() && idx >= params.Len()-1:
			c.counts[pre+"EXCLUDED: variadic tail"]++
			continue
		case idx >= params.Len() || !isString(params.At(idx).Type()):
			c.counts[pre+"not a string parameter"]++
			continue
		}
		key := c.pos(params.At(idx).Pos())
		v := c.param[key]
		if v == "" {
			v = "no-verdict"
		}
		f := ""
		if isFormat {
			f = " (format position)"
		}
		c.counts[pre+"-> string param: "+v+f]++
		p := c.params[key]
		if p == nil {
			c.counts[pre+"-> string param: callee body not in this load (skipped)"+f]++
			continue
		}
		switch {
		case kind == "sb":
			p.sb++
		case v != "noescape":
			p.litLeak++
		case isFormat:
			p.litFmt++
		default:
			p.lit++
		}
	}
}

// fixedPoint: a parameter survives if Go says noescape, no disqualifying class holds, and every onward
// parameter survives. Removal is iterated to the greatest fixed point.
func (c *census) fixedPoint(slot int, m mode) {
	for _, p := range c.params {
		ok := p.verdict == "noescape"
		for cl := range p.classes {
			if disqualifying(cl, m) {
				ok = false
			}
		}
		p.survive[slot] = ok
		p.culprit[slot] = ""
		p.culpritK[slot] = ""
	}
	for changed := true; changed; {
		changed = false
		for _, p := range c.params {
			if !p.survive[slot] {
				continue
			}
			for e := range p.edges {
				q := c.params[e]
				if q == nil || !q.survive[slot] {
					p.survive[slot] = false
					if q == nil {
						p.culprit[slot] = c.rel(e) + " (not a production parameter in this load)"
					} else {
						p.culprit[slot] = q.fn
						p.culpritK[slot] = e
					}
					changed = true
					break
				}
			}
		}
	}
}

// reasons lists a non-surviving parameter's disqualifying classes, the cascade included.
func (p *param) reasons(slot int, m mode) []string {
	var out []string
	if p.verdict == "no-verdict" {
		out = append(out, cNoVerdict)
	} else if p.verdict != "noescape" {
		out = append(out, cVerdict)
	}
	for cl := range p.classes {
		if disqualifying(cl, m) {
			out = append(out, cl)
		}
	}
	if p.culprit[slot] != "" {
		out = append(out, cCascade)
	}
	return out
}

func first(rs []string) string {
	for _, pr := range precedence {
		for _, r := range rs {
			if r == pr {
				return r
			}
		}
	}
	if len(rs) > 0 {
		sort.Strings(rs)
		return rs[0]
	}
	return ""
}

func (c *census) report(w *os.File) {
	pr := func(format string, a ...any) { fmt.Fprintf(w, format, a...) }
	pr("# O1 survival census, GOOS=%s. Population and verdicts as c2-escape-join's J rows; classes and\n", c.goos)
	pr("# predicates in main.go; controls in main_test.go. Sites are production call sites.\n\n")

	pr("## P: the population (and R2: every production string literal by what consumes it)\n")
	var keys []string
	for k := range c.counts {
		keys = append(keys, k)
	}
	sort.Strings(keys)
	for _, k := range keys {
		pr("%-96s %6d\n", k, c.counts[k])
	}

	var reached []*param
	for _, p := range c.params {
		if p.lit+p.litFmt+p.sb > 0 {
			reached = append(reached, p)
		}
	}
	sort.Slice(reached, func(i, j int) bool { return reached[i].key < reached[j].key })

	titles := []string{
		"TODAY'S golib and converter (range, copy and the spread disqualify; so does a func-value use)",
		"WITH the sstring golib gaps closed (an enumerator, copy, the spread)",
		"WITH the gaps closed AND an adapter lambda at every func-value site",
	}
	for slot, m := range modes {
		title := titles[slot]
		pr("\n## S%d: survival, %s\n", slot, title)
		var sp, sl, sf, ss, fp, fl, ff, fs int
		for _, p := range reached {
			if p.survive[slot] {
				sp++
				sl += p.lit
				sf += p.litFmt
				ss += p.sb
			} else {
				fp++
				fl += p.lit
				ff += p.litFmt
				fs += p.sb
			}
		}
		pr("reached parameters (a noescape literal or any string(b) argument)   survive %5d   fail %5d\n", sp, fp)
		pr("noescape literal sites, non-format                                   survive %5d   fail %5d\n", sl, fl)
		pr("noescape literal sites, format position                              survive %5d   fail %5d\n", sf, ff)
		pr("string(b) argument sites (any verdict)                               survive %5d   fail %5d\n", ss, fs)
		if sl+sf > 0 {
			pr("format-position share of surviving literal sites                     %.1f%% (%d of %d)\n", 100*float64(sf)/float64(sl+sf), sf, sl+sf)
		}

		type agg struct{ params, lit, fmtLit, sb int }
		anyOf := map[string]*agg{}
		firstOf := map[string]*agg{}
		for _, p := range reached {
			if p.survive[slot] || p.lit+p.litFmt+p.sb == 0 {
				continue
			}
			rs := p.reasons(slot, m)
			for _, r := range rs {
				if anyOf[r] == nil {
					anyOf[r] = &agg{}
				}
				a := anyOf[r]
				a.params++
				a.lit += p.lit
				a.fmtLit += p.litFmt
				a.sb += p.sb
			}
			f := first(rs)
			if firstOf[f] == nil {
				firstOf[f] = &agg{}
			}
			a := firstOf[f]
			a.params++
			a.lit += p.lit
			a.fmtLit += p.litFmt
			a.sb += p.sb
		}
		for _, tbl := range []struct {
			name string
			m    map[string]*agg
		}{{"failing reached parameters, EVERY reason (a parameter counts once per reason)", anyOf}, {"failing reached parameters, FIRST reason by precedence (sums to the failures)", firstOf}} {
			pr("\n%s\n%-84s %7s %7s %7s %7s\n", tbl.name, "class", "params", "lit", "fmt-lit", "s(b)")
			var ks []string
			for k := range tbl.m {
				ks = append(ks, k)
			}
			sort.Slice(ks, func(i, j int) bool {
				return tbl.m[ks[i]].lit+tbl.m[ks[i]].fmtLit > tbl.m[ks[j]].lit+tbl.m[ks[j]].fmtLit || (tbl.m[ks[i]].lit+tbl.m[ks[i]].fmtLit == tbl.m[ks[j]].lit+tbl.m[ks[j]].fmtLit && ks[i] < ks[j])
			})
			for _, k := range ks {
				a := tbl.m[k]
				pr("%-84s %7d %7d %7d %7d\n", k, a.params, a.lit, a.fmtLit, a.sb)
			}
		}

		pr("\ncallees by surviving noescape literal sites (top 25; fmt's unexported helpers are listed in F below)\n")
		byFn := map[string][2]int{}
		for _, p := range reached {
			if p.survive[slot] {
				v := byFn[p.fn]
				v[0] += p.lit
				v[1] += p.litFmt
				byFn[p.fn] = v
			}
		}
		var fns []string
		for k := range byFn {
			fns = append(fns, k)
		}
		sort.Slice(fns, func(i, j int) bool {
			a, b := byFn[fns[i]], byFn[fns[j]]
			return a[0]+a[1] > b[0]+b[1] || (a[0]+a[1] == b[0]+b[1] && fns[i] < fns[j])
		})
		for i, k := range fns {
			if i == 25 {
				break
			}
			pr("  %-70s lit %5d  fmt-lit %5d\n", k, byFn[k][0], byFn[k][1])
		}
	}

	pr("\n## C: cascade roots at S2 (each cascading reached parameter's culprit chain, followed to the first\n")
	pr("## parameter that fails for its own reason; noescape literal sites totalled by that root)\n")
	roots := map[string]int{}
	for _, p := range reached {
		if p.survive[2] || p.culpritK[2] == "" {
			continue
		}
		q, seenK := p, map[string]bool{}
		for q.culpritK[2] != "" && !seenK[q.key] {
			seenK[q.key] = true
			q = c.params[q.culpritK[2]]
		}
		own := []string{}
		for _, r := range q.reasons(2, modes[2]) {
			if r != cCascade {
				own = append(own, r)
			}
		}
		if q.culpritK[2] == "" && q.culprit[2] != "" {
			own = append(own, "onward to "+q.culprit[2])
		}
		roots[q.fn+" #"+fmt.Sprint(q.idx)+"  <-  "+strings.Join(own, "; ")] += p.lit + p.litFmt
	}
	var rks []string
	for k := range roots {
		rks = append(rks, k)
	}
	sort.Slice(rks, func(i, j int) bool {
		return roots[rks[i]] > roots[rks[j]] || (roots[rks[i]] == roots[rks[j]] && rks[i] < rks[j])
	})
	for _, k := range rks {
		pr("  %6d  %s\n", roots[k], k)
	}

	pr("\n## F: fmt's unexported helpers (every string parameter, reached or not)\n")
	var fmts []*param
	for _, p := range c.params {
		if p.unexpFmt {
			fmts = append(fmts, p)
		}
	}
	sort.Slice(fmts, func(i, j int) bool { return fmts[i].fn+fmts[i].key < fmts[j].fn+fmts[j].key })
	for _, p := range fmts {
		pr("  %-40s #%d  verdict %-11s S0 %-5v S1 %-5v S2 %-5v lit %3d fmt-lit %3d  S2 reasons: %s\n", p.fn, p.idx, p.verdict, p.survive[0], p.survive[1], p.survive[2], p.lit, p.litFmt, strings.Join(p.reasons(2, modes[2]), "; "))
	}

	pr("\n## R: the residual (production literal arguments sstring cannot reach, at S2)\n")
	res := map[string]int{}
	for _, p := range c.params {
		if !p.survive[2] {
			r := first(p.reasons(2, modes[2]))
			bucket := "other"
			switch {
			case strings.Contains(r, "stored"), strings.Contains(r, "sent on"), r == cMapKey:
				bucket = "stored"
			case r == cBoxIface:
				bucket = "boxed"
			case r == cReturn:
				bucket = "returned"
			case strings.HasPrefix(r, "sig:"):
				bucket = "fixed signature"
			case r == cCascade:
				bucket = "cascade (an onward parameter fails)"
			}
			res["noescape literal sites whose parameter fails: "+bucket] += p.lit + p.litFmt
			res["noescape literal sites whose parameter fails, by first reason: "+r] += p.lit + p.litFmt
		}
		res["literal sites whose parameter Go says leaks (heap or result) or has no verdict"] += p.litLeak
	}
	for _, k := range []string{"EXCLUDED: no callee declaration", "EXCLUDED: interface method", "EXCLUDED: generic instantiation", "EXCLUDED: variadic tail",
		"EXCLUDED: argument of a builtin", "EXCLUDED: argument of a conversion"} {
		res["literal sites with no parameter to retype ("+k+")"] = c.counts["P lit "+k]
	}
	var rk []string
	for k := range res {
		rk = append(rk, k)
	}
	sort.Strings(rk)
	for _, k := range rk {
		if res[k] != 0 {
			pr("%-96s %6d\n", k, res[k])
		}
	}

	pr("\n## PILOT: the parameters the fmt format-position pilot flips: fmt's format-position parameters and its\n")
	pr("## unexported helpers that survive at S2, closed under onward passing (S0/S1/S2: which preconditions each needs)\n")
	pilot := map[string]bool{}
	var visit func(k string)
	visit = func(k string) {
		if pilot[k] {
			return
		}
		pilot[k] = true
		for e := range c.params[k].edges {
			if c.params[e] != nil {
				visit(e)
			}
		}
	}
	for _, p := range fmts {
		if p.survive[2] {
			visit(p.key)
		}
	}
	for _, p := range c.params {
		if p.pkg == "fmt" && p.exported && p.litFmt > 0 && p.survive[2] {
			visit(p.key)
		}
	}
	var pk []string
	for k := range pilot {
		pk = append(pk, k)
	}
	sort.Slice(pk, func(i, j int) bool { return c.params[pk[i]].fn+pk[i] < c.params[pk[j]].fn+pk[j] })
	for _, k := range pk {
		p := c.params[k]
		pr("  %-44s #%d  %-44s S0 %-5v S1 %-5v S2 %-5v\n", p.fn, p.idx, c.rel(k), p.survive[0], p.survive[1], p.survive[2])
	}
}

func (c *census) writeParams(path string) error {
	var rows []string
	for _, p := range c.params {
		if p.lit+p.litFmt+p.sb+p.litLeak == 0 && !p.unexpFmt {
			continue
		}
		fv := c.funcValueAt[c.fnKey[p.key]]
		fvs := ""
		if len(fv) > 0 {
			fvs = fmt.Sprintf("%d, first %s", len(fv), fv[0])
		}
		rows = append(rows, fmt.Sprintf("%s\t%s\t%d\t%s\t%v\t%v\t%v\t%d\t%d\t%d\t%d\t%s\t%s\t%s\t%s", c.rel(p.key), p.fn, p.idx, p.verdict,
			p.survive[0], p.survive[1], p.survive[2], p.lit, p.litFmt, p.litLeak, p.sb,
			strings.Join(p.reasons(0, modes[0]), "; "), strings.Join(p.reasons(2, modes[2]), "; "), p.culprit[2], fvs))
	}
	sort.Strings(rows)
	out := "key\tfunction\tparam\tverdict\tS0\tS1\tS2\tlit\tfmt_lit\tleak_lit\tstring_b\treasons_S0\treasons_S2\tculprit_S2\tfunc_value_sites\n" + strings.Join(rows, "\n") + "\n"
	return os.WriteFile(path, []byte(out), 0o644)
}

func execOutput(name string, args ...string) (string, error) {
	out, err := exec.Command(name, args...).Output()
	return string(out), err
}
