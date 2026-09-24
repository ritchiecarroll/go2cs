// c2-escape-join: the standard library's string-literal and no-copy-idiom census for
// docs/phase4/DESIGN-string-literal-allocation.md §8.1R/§8.2, joined to the Go compiler's own escape
// verdicts. A record-producing probe, not a gate.
//
//	GOOS=<os> go build -a -gcflags=-m std 2> m-<os>.txt
//	GOOS=<os> go run . -m m-<os>.txt > census-<os>.txt
//
// Every count is over Go SOURCE (std plus its tests, the GOOS given). Exclusions are counted, not
// dropped: a call through a func value, an interface method or a generic instantiation has no
// callee declaration position to join, and an argument in a variadic tail has no single parameter.
// The verdict describes Go's function; a hand-owned go2cs file (unsafe, testing) is not Go's source.
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
	"regexp"
	"sort"
	"strings"

	"golang.org/x/tools/go/packages"
)

// Copies of the converter's own predicates (src/go2cs/hoistedLiteralOperations.go), so a literal is
// "format-position" and "degenerate" exactly as the converter decides it.
const (
	minHoistSlugLength = 3
	maxHoistSlugLength = 24
)

func isASCIILetter(r rune) bool { return (r >= 'a' && r <= 'z') || (r >= 'A' && r <= 'Z') }

// slugLen is len(literalSlug(v)); the camelCase fold preserves length, so only the word split, the
// leading-digit drop and the total budget matter.
func slugLen(v string) int {
	var words []string
	var cur strings.Builder
	for _, r := range v {
		if isASCIILetter(r) || (r >= '0' && r <= '9') {
			cur.WriteRune(r)
			continue
		}
		if cur.Len() > 0 {
			words = append(words, cur.String())
			cur.Reset()
		}
	}
	if cur.Len() > 0 {
		words = append(words, cur.String())
	}
	for len(words) > 0 && !isASCIILetter(rune(words[0][0])) {
		words = words[1:]
	}
	n := 0
	for _, w := range words {
		if n+len(w) > maxHoistSlugLength {
			break
		}
		n += len(w)
		if n >= maxHoistSlugLength {
			break
		}
	}
	return n
}

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

func bucket(n int) string {
	switch {
	case n <= 8:
		return "a<=8"
	case n <= 16:
		return "b9-16"
	case n <= 32:
		return "c17-32"
	}
	return "d>32"
}

// operandPos is where the compiler positions an expression node: a binary or unary expression at its
// operator, an index or slice at its '[', a call at its '(', a selector at its '.', and a parenthesised
// expression at its inner expression (the syntax package drops parentheses). key "ast" returns Pos().
func operandPos(e ast.Expr, key string) token.Pos {
	if key == "ast" {
		return e.Pos()
	}
	switch x := e.(type) {
	case *ast.ParenExpr:
		return operandPos(x.X, key)
	case *ast.BinaryExpr:
		return x.OpPos
	case *ast.UnaryExpr:
		return x.OpPos
	case *ast.StarExpr:
		return x.Star
	case *ast.IndexExpr:
		return x.Lbrack
	case *ast.IndexListExpr:
		return x.Lbrack
	case *ast.SliceExpr:
		return x.Lbrack
	case *ast.CallExpr:
		return x.Lparen
	case *ast.SelectorExpr:
		return x.Sel.Pos() - 1
	}
	return e.Pos()
}

func main() {
	mFile := flag.String("m", "", "the -gcflags=-m output for the same GOOS")
	// A conversion's verdict is printed at its OPERAND's compiler position, which for a compound operand
	// is its operator, not ast.Node.Pos() (G's oracle block, DESIGN-nonescaping-locals.md finding 4, at
	// claude/g-rec-b-oracle 814603bbbb). "compiler" (the default) keys that way; "ast" reproduces the key
	// revision 3 used, so the two can be diffed.
	posKey := flag.String("poskey", "compiler", "operand key for conversion verdicts: compiler or ast")
	flag.Parse()

	// Parameter verdicts keyed by declaration position. Expression verdicts are keyed where -m reports
	// them: a conversion at its operand's compiler position (operandPos), a concatenation at its outermost `+`.
	param := map[string]string{}
	// Every expression line at a position, with its printed text: an inlined callee's allocation is
	// reported at the caller's call position (G's finding 2), which is also a call operand's compiler
	// position, so a verdict is taken only from a line whose text is the construct being joined.
	type exprLine struct{ text, verdict string }
	exprLines := map[string][]exprLine{}
	textMismatch := map[string]int{}
	verdictAt := func(row, k string, match func(string) bool) string {
		v := ""
		for _, l := range exprLines[k] {
			if !match(l.text) {
				continue
			}
			switch {
			case l.verdict == "zero-copy":
				v = "zero-copy"
			case l.verdict == "heap" && v != "zero-copy":
				v = "heap"
			case v == "":
				v = l.verdict
			}
		}
		if v == "" {
			if len(exprLines[k]) > 0 {
				textMismatch[row]++ // a verdict sits at the key, for a different expression
			}
			return "no-verdict"
		}
		return v
	}
	reParam := regexp.MustCompile(`^(.*\.go):(\d+):(\d+): (?:leaking param: (\w+)(.*)|(\w+) does not escape)$`)
	reExpr := regexp.MustCompile(`^(.*\.go):(\d+):(\d+): (.+) (does not escape|escapes to heap)$`)
	// Go 1.22+'s read-only string->[]byte optimisation prints its own line at the argument's position.
	reZeroCopy := regexp.MustCompile(`^(.*\.go):(\d+):(\d+): zero-copy string->\[\]byte conversion$`)
	fh, err := os.Open(*mFile)
	if err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(2)
	}
	sc := bufio.NewScanner(fh)
	sc.Buffer(make([]byte, 1<<20), 1<<24)
	lines := 0
	for sc.Scan() {
		lines++
		s := sc.Text()
		if m := reParam.FindStringSubmatch(s); m != nil {
			k := m[1] + ":" + m[2] + ":" + m[3]
			switch {
			case m[6] != "":
				if _, ok := param[k]; !ok {
					param[k] = "noescape"
				}
			case strings.Contains(m[5], "to result"):
				if param[k] != "leak-heap" {
					param[k] = "leak-result"
				}
			default:
				param[k] = "leak-heap"
			}
		}
		if m := reZeroCopy.FindStringSubmatch(s); m != nil {
			k := m[1] + ":" + m[2] + ":" + m[3]
			exprLines[k] = append(exprLines[k], exprLine{"zero-copy", "zero-copy"})
			continue
		}
		if m := reExpr.FindStringSubmatch(s); m != nil {
			k := m[1] + ":" + m[2] + ":" + m[3]
			v := "noescape"
			if m[5] == "escapes to heap" {
				v = "heap"
			}
			exprLines[k] = append(exprLines[k], exprLine{m[4], v})
		}
	}
	fh.Close()

	cfg := &packages.Config{Mode: packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo, Tests: true}
	pkgs, err := packages.Load(cfg, "std")
	if err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(2)
	}

	counts := map[string]int{}
	noEscFuncs := map[string]bool{}
	seen := map[string]bool{}

	isString := func(t types.Type) bool {
		b, ok := t.Underlying().(*types.Basic)
		return ok && b.Info()&types.IsString != 0
	}
	isPredeclString := func(t types.Type) bool {
		b, ok := types.Unalias(t).(*types.Basic)
		return ok && b.Info()&types.IsString != 0
	}
	byteSliceElem := func(info *types.Info, e ast.Expr) (types.Type, bool) {
		t := info.TypeOf(e)
		if t == nil {
			return nil, false
		}
		s, ok := t.Underlying().(*types.Slice)
		if !ok {
			return nil, false
		}
		b, ok := s.Elem().Underlying().(*types.Basic)
		return s.Elem(), ok && b.Kind() == types.Uint8
	}
	// string(b) with b a byte slice, or nil
	strOfBytes := func(info *types.Info, e ast.Expr) (*ast.CallExpr, bool) {
		c, ok := ast.Unparen(e).(*ast.CallExpr)
		if !ok || len(c.Args) != 1 {
			return nil, false
		}
		tv, ok := info.Types[c.Fun]
		if !ok || !tv.IsType() || !isString(tv.Type) {
			return nil, false
		}
		_, ok = byteSliceElem(info, c.Args[0])
		return c, ok
	}

	for _, p := range pkgs {
		info := p.TypesInfo
		if info == nil {
			continue
		}
		fset := p.Fset
		pos := func(x token.Pos) string {
			pp := fset.Position(x)
			return fmt.Sprintf("%s:%d:%d", pp.Filename, pp.Line, pp.Column)
		}
		for _, f := range p.Syntax {
			name := fset.Position(f.Pos()).Filename
			if seen[name] {
				continue
			}
			seen[name] = true
			scope := "prod"
			if strings.HasSuffix(name, "_test.go") {
				scope = "test"
			}
			var stack []ast.Node
			ast.Inspect(f, func(n ast.Node) bool {
				if n == nil {
					stack = stack[:len(stack)-1]
					return true
				}
				var parent ast.Node
				if len(stack) > 0 {
					parent = stack[len(stack)-1]
				}
				stack = append(stack, n)

				// I: every string([]byte) conversion, by what consumes it (the §8.2/§8.2R idiom table)
				if call, ok := n.(*ast.CallExpr); ok {
					if _, ok := strOfBytes(info, call); ok {
						ctx := "I7 string(b) elsewhere"
						switch pp := parent.(type) {
						case *ast.CallExpr:
							if pp.Fun != call {
								ctx = "I6 string(b) as a call argument"
							}
						case *ast.BinaryExpr:
							ctx = "I2/I3 string(b) as a binary operand (compare or concatenation)"
						case *ast.SwitchStmt:
							ctx = "I5 switch string(b)"
						case *ast.RangeStmt:
							ctx = "I4 range string(b)"
						case *ast.IndexExpr:
							ctx = "I1 string(b) as an index (map key)"
						case *ast.AssignStmt, *ast.ValueSpec:
							ctx = "I8 string(b) bound to a name"
						case *ast.ReturnStmt:
							ctx = "I9 string(b) returned"
						}
						counts[ctx+" "+scope]++
					}
				}
				if be, ok := n.(*ast.BinaryExpr); ok {
					switch be.Op {
					case token.EQL, token.NEQ, token.LSS, token.LEQ, token.GTR, token.GEQ:
						for _, side := range []ast.Expr{be.X, be.Y} {
							if _, ok := strOfBytes(info, side); ok {
								counts["I2 string(b) compared "+scope]++
							}
							if cb, ok := ast.Unparen(side).(*ast.BinaryExpr); ok && cb.Op == token.ADD && isString(info.TypeOf(cb)) {
								counts["I3 concatenation inside a comparison "+scope]++
							}
						}
					}
				}

				switch e := n.(type) {
				case *ast.FuncDecl:
					if scope == "test" || e.Type.Params == nil {
						return true
					}
					for _, fld := range e.Type.Params.List {
						for _, id := range fld.Names {
							obj := info.Defs[id]
							if obj == nil || !isString(obj.Type()) {
								continue
							}
							v := param[pos(id.Pos())]
							if v == "" {
								v = "no-verdict"
							}
							counts["J string PARAMETERS, prod: "+v]++
							if v == "noescape" {
								noEscFuncs[p.PkgPath+"."+e.Name.Name] = true
							}
						}
					}

				case *ast.CallExpr:
					// (1) type conversions: string(x) of an integer, []byte("lit")
					if tv, ok := info.Types[e.Fun]; ok && tv.IsType() && len(e.Args) == 1 {
						at := info.TypeOf(e.Args[0])
						if at != nil && isString(tv.Type) {
							if b, ok := at.Underlying().(*types.Basic); ok && b.Info()&types.IsInteger != 0 {
								kind := "rune/int"
								if b.Kind() == types.Uint8 {
									kind = "byte"
								}
								row := fmt.Sprintf("R1 string(%s) %s", kind, scope)
								v := verdictAt(row, pos(operandPos(e.Args[0], *posKey)), func(s string) bool { return strings.HasPrefix(s, "string(") })
								counts[row+": "+v]++
							}
						}
						if s, ok := tv.Type.Underlying().(*types.Slice); ok && at != nil && isString(at) {
							if eb, ok := s.Elem().Underlying().(*types.Basic); ok && eb.Kind() == types.Uint8 {
								if cv := info.Types[e.Args[0]].Value; cv != nil && cv.Kind() == constant.String {
									row := fmt.Sprintf("R5 []byte(\"const\") %s", scope)
									v := verdictAt(row, pos(operandPos(e.Args[0], *posKey)), func(s string) bool { return s == "zero-copy" || strings.Contains(s, "[]byte") })
									counts[row+": "+v]++
								}
							}
						}
						return true
					}

					// (2) literal arguments: size buckets, and the join to the callee's parameter
					sig, _ := info.TypeOf(e.Fun).(*types.Signature)
					if sig == nil {
						if t := info.TypeOf(e.Fun); t != nil {
							sig, _ = t.Underlying().(*types.Signature)
						}
					}
					if sig == nil {
						return true
					}
					var callee *types.Func
					switch fn := ast.Unparen(e.Fun).(type) {
					case *ast.Ident:
						callee, _ = info.Uses[fn].(*types.Func)
					case *ast.SelectorExpr:
						callee, _ = info.Uses[fn.Sel].(*types.Func)
					case *ast.IndexExpr, *ast.IndexListExpr:
						callee = nil
					}
					params := sig.Params()
					for idx, arg := range e.Args {
						lit, ok := ast.Unparen(arg).(*ast.BasicLit)
						if !ok || lit.Kind != token.STRING {
							continue
						}
						cv := info.Types[lit].Value
						if cv == nil {
							continue
						}
						val := constant.StringVal(cv)
						isFormat := sig.Variadic() && params.Len() >= 2 && idx == params.Len()-2 && callFuncNameEndsWithF(e.Fun) && isString(params.At(idx).Type())
						if isFormat {
							counts["L format-position literal "+scope+" "+bucket(len(val))]++
						} else if val != "" && slugLen(val) < minHoistSlugLength {
							dest := "other"
							var target types.Type
							if sig.Variadic() && idx >= params.Len()-1 {
								if s, ok := params.At(params.Len() - 1).Type().(*types.Slice); ok && !e.Ellipsis.IsValid() {
									target = s.Elem()
								}
							} else if idx < params.Len() {
								target = params.At(idx).Type()
							}
							if target != nil {
								if isString(target) {
									dest = "@string-param"
								} else if it, ok := target.Underlying().(*types.Interface); ok && it.Empty() {
									dest = "any-param"
								}
							}
							counts["L degenerate literal -> "+dest+" "+scope+" "+bucket(len(val))]++
						}

						// the join
						format := ""
						if isFormat {
							format = " (format position)"
						}
						switch {
						case callee == nil:
							counts["J EXCLUDED literal arg, no callee declaration (func value / generic / conversion) "+scope]++
							continue
						case callee.Type().(*types.Signature).Recv() != nil && types.IsInterface(callee.Type().(*types.Signature).Recv().Type()):
							counts["J EXCLUDED literal arg, interface method "+scope]++
							continue
						case callee.Origin() != callee:
							counts["J EXCLUDED literal arg, generic instantiation "+scope]++
							continue
						case sig.Variadic() && idx >= params.Len()-1:
							counts["J EXCLUDED literal arg, variadic tail "+scope]++
							continue
						case idx >= params.Len() || !isString(params.At(idx).Type()):
							continue
						}
						v := param[pos(params.At(idx).Pos())]
						if v == "" {
							v = "no-verdict"
						}
						counts["J literal -> string param "+scope+": "+v+format]++
					}

				case *ast.IndexExpr:
					mt, ok := info.TypeOf(e.X).Underlying().(*types.Map)
					if !ok {
						return true
					}
					write := false
					if as, ok := parent.(*ast.AssignStmt); ok {
						for _, l := range as.Lhs {
							if l == n {
								write = true
							}
						}
					}
					if write {
						return true
					}
					if c, ok := strOfBytes(info, e.Index); ok {
						elem, _ := byteSliceElem(info, c.Args[0])
						eb, _ := types.Unalias(elem).(*types.Basic)
						switch {
						case !isPredeclString(mt.Key()):
							counts["M1 m[string(b)] read "+scope+": NOT covered (named string KEY type)"]++
						case !isPredeclString(info.TypeOf(c)):
							counts["M1 m[string(b)] read "+scope+": NOT covered (conversion to a NAMED string)"]++
						case eb == nil || eb.Kind() != types.Uint8:
							counts["M1 m[string(b)] read "+scope+": NOT covered (named-byte ELEMENT)"]++
						default:
							counts["M1 m[string(b)] read "+scope+": covered (tmpstring, convIndexExpr.go mapReadTmpStringKey)"]++
						}
					} else if cl, ok := ast.Unparen(e.Index).(*ast.CompositeLit); ok {
						for _, el := range cl.Elts {
							if kv, ok := el.(*ast.KeyValueExpr); ok {
								el = kv.Value
							}
							if _, ok := strOfBytes(info, el); ok {
								counts["M2 m[T{..., string(b), ...}] composite-key read "+scope]++
								break
							}
						}
					}

				case *ast.BinaryExpr:
					if e.Op != token.ADD || !isString(info.TypeOf(e)) {
						return true
					}
					if pb, ok := parent.(*ast.BinaryExpr); ok && pb.Op == token.ADD && isString(info.TypeOf(pb)) {
						return true // not the outermost +
					}
					operands := 0
					var walk func(x ast.Expr)
					walk = func(x ast.Expr) {
						if b, ok := ast.Unparen(x).(*ast.BinaryExpr); ok && b.Op == token.ADD && isString(info.TypeOf(b)) {
							walk(b.X)
							walk(b.Y)
							return
						}
						operands++
					}
					walk(e)
					if info.Types[e].Value != nil {
						return true // folded at compile time
					}
					k := "2 operands"
					if operands >= 3 {
						k = ">=3 operands"
					}
					row := fmt.Sprintf("R4 concatenation %s %s", k, scope)
					v := verdictAt(row, pos(e.OpPos), func(s string) bool { return strings.Contains(s, " + ") })
					counts[row+": "+v]++
				}
				return true
			})
		}
	}

	for row, n := range textMismatch {
		counts[row+": no-verdict, of which a verdict for another expression sits at the key"] = n
	}

	var keys []string
	for k := range counts {
		keys = append(keys, k)
	}
	sort.Strings(keys)
	fmt.Printf("m-lines=%d param-verdicts=%d expr-verdicts=%d files=%d functions-with-a-noescape-string-param=%d\n", lines, len(param), len(exprLines), len(seen), len(noEscFuncs))
	for _, k := range keys {
		fmt.Printf("%-100s %d\n", k, counts[k])
	}
}
