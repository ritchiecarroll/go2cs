// b3census: Go-side census for the collection-expression design.
// For every package loaded: range statements by operand kind and iteration-variable shape, and composite literals
// of slice type by form and by whether their syntactic position gives C# a target type.
package main

import (
	"flag"
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"os"
	"sort"
	"strings"

	"golang.org/x/tools/go/packages"
)

type counter map[string]int

func (c counter) add(k string) { c[k]++ }

var (
	counts   = counter{}
	examples = map[string][]string{}
	seenFile = map[string]bool{}
)

func ex(class, where string) {
	counts.add(class)
	if len(examples[class]) < 8 {
		examples[class] = append(examples[class], where)
	}
}

func main() {
	tests := flag.Bool("tests", false, "include _test.go files")
	dir := flag.String("dir", "", "working directory for the load")
	flag.Parse()
	cfg := &packages.Config{Mode: packages.NeedName | packages.NeedFiles | packages.NeedCompiledGoFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo,
		Tests: *tests, Dir: *dir}
	pkgs, err := packages.Load(cfg, flag.Args()...)
	if err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(2)
	}
	for _, p := range pkgs {
		for _, e := range p.Errors {
			counts.add("load-errors")
			if counts["load-errors"] <= 5 {
				fmt.Fprintln(os.Stderr, e)
			}
		}
		for i, f := range p.Syntax {
			name := p.CompiledGoFiles[i]
			if seenFile[name] {
				continue
			}
			seenFile[name] = true
			counts.add("files")
			visit(p, f)
		}
	}
	keys := make([]string, 0, len(counts))
	for k := range counts {
		keys = append(keys, k)
	}
	sort.Strings(keys)
	for _, k := range keys {
		fmt.Printf("%-60s %7d\n", k, counts[k])
	}
	fmt.Println("--- examples")
	for _, k := range keys {
		for _, e := range examples[k] {
			fmt.Printf("%s\t%s\n", k, e)
		}
	}
}

func seqKind(t types.Type) string {
	switch u := t.Underlying().(type) {
	case *types.Slice:
		return "slice"
	case *types.Array:
		return "array"
	case *types.Pointer:
		if _, ok := u.Elem().Underlying().(*types.Array); ok {
			return "ptr-array"
		}
	case *types.Map:
		return "map"
	case *types.Basic:
		if u.Info()&types.IsString != 0 {
			return "string"
		}
		if u.Info()&types.IsInteger != 0 {
			return "int"
		}
	case *types.Chan:
		return "chan"
	case *types.Signature:
		return "func"
	case *types.TypeParam:
		return "typeparam"
	case *types.Interface:
		return "typeparam"
	}
	return "other"
}

func blank(e ast.Expr) bool {
	id, ok := e.(*ast.Ident)
	return e == nil || (ok && id.Name == "_")
}

func visit(p *packages.Package, f *ast.File) {
	info := p.TypesInfo
	fset := p.Fset
	pos := func(n ast.Node) string {
		ps := fset.Position(n.Pos())
		s := ps.Filename
		if i := strings.Index(s, "/src/"); i >= 0 {
			s = s[i+5:]
		}
		return fmt.Sprintf("%s:%d", s, ps.Line)
	}
	var stack []ast.Node
	ast.Inspect(f, func(n ast.Node) bool {
		if n == nil {
			stack = stack[:len(stack)-1]
			return true
		}
		switch x := n.(type) {
		case *ast.RangeStmt:
			t := info.TypeOf(x.X)
			if t == nil {
				break
			}
			k := seqKind(t)
			shape := "pair"
			if blank(x.Value) {
				shape = "index-only"
			}
			if blank(x.Key) && blank(x.Value) {
				shape = "none"
			}
			ex("range:"+k+":"+shape, pos(x))
			if (k == "slice" || k == "array" || k == "ptr-array") && shape == "pair" {
				if blank(x.Key) {
					ex("range-pair-seq:key-blank", pos(x))
				} else {
					ex("range-pair-seq:key-named", pos(x))
				}
				if x.Tok == token.ASSIGN {
					ex("range-pair-seq:assigns-existing-vars", pos(x))
				}
			}
		case *ast.CompositeLit:
			t := info.TypeOf(x)
			if t == nil {
				break
			}
			u := t.Underlying()
			var kind string
			switch u.(type) {
			case *types.Slice:
				kind = "slice"
			case *types.Array:
				kind = "array"
			case *types.Map:
				kind = "map"
			default:
				break
			}
			if kind == "" {
				break
			}
			keyed := false
			for _, el := range x.Elts {
				if _, ok := el.(*ast.KeyValueExpr); ok {
					keyed = true
				}
			}
			_, named := types.Unalias(t).(*types.Named)
			form := kind
			if named {
				form += "-named"
			}
			if keyed {
				form += "-keyed"
			}
			if len(x.Elts) == 0 {
				form += "-empty"
			}
			counts.add("lit:" + form)
			if kind == "slice" && !named && !keyed {
				var parent ast.Node
				if len(stack) > 0 {
					parent = stack[len(stack)-1]
				}
				ctx := litContext(info, x, parent, stack)
				ex("lit-slice-unnamed:"+ctx, pos(x))
				if _, isSl := u.(*types.Slice).Elem().Underlying().(*types.Array); isSl {
					counts.add("lit-slice-unnamed:elem-is-array")
				}
			}
		}
		stack = append(stack, n)
		return true
	})
}

// litContext says whether C# would know the literal's target type at its position.
func litContext(info *types.Info, lit *ast.CompositeLit, parent ast.Node, stack []ast.Node) string {
	switch pr := parent.(type) {
	case *ast.ParenExpr:
		if len(stack) >= 2 {
			return litContext(info, lit, stack[len(stack)-2], stack[:len(stack)-1])
		}
	case *ast.AssignStmt:
		if pr.Tok == token.DEFINE {
			return "untargeted:define"
		}
		return "targeted:assign"
	case *ast.ValueSpec:
		if pr.Type != nil {
			return "targeted:typed-var"
		}
		for _, an := range stack {
			switch an.(type) {
			case *ast.FuncDecl, *ast.FuncLit:
				return "untargeted:untyped-local-var"
			}
		}
		return "targeted:package-var-field"
	case *ast.ReturnStmt:
		return "targeted:return"
	case *ast.CompositeLit:
		if pr.Type == nil && len(stack) >= 2 { // elided outer type: inner of a nested literal
			return "targeted:element"
		}
		return "targeted:element"
	case *ast.KeyValueExpr:
		return "targeted:element-or-field"
	case *ast.SendStmt:
		return "targeted:send"
	case *ast.CallExpr:
		if pr.Fun == lit {
			return "untargeted:callee"
		}
		ft := info.TypeOf(pr.Fun)
		if tv, ok := info.Types[pr.Fun]; ok && tv.IsType() {
			return "untargeted:conversion"
		}
		if id, ok := pr.Fun.(*ast.Ident); ok {
			if _, isB := info.Uses[id].(*types.Builtin); isB {
				return "untargeted:builtin-" + id.Name
			}
		}
		sig, ok := ft.(*types.Signature)
		if !ok {
			return "untargeted:call-other"
		}
		idx := -1
		for i, a := range pr.Args {
			if a == lit {
				idx = i
			}
		}
		params := sig.Params()
		var pt types.Type
		switch {
		case sig.Variadic() && idx >= params.Len()-1:
			if pr.Ellipsis.IsValid() {
				pt = params.At(params.Len() - 1).Type()
			} else {
				pt = params.At(params.Len() - 1).Type().(*types.Slice).Elem()
			}
		case idx >= 0 && idx < params.Len():
			pt = params.At(idx).Type()
		}
		if pt == nil {
			return "untargeted:call-other"
		}
		// the generic declaration's parameter type decides whether C# infers it
		if gen := genericParam(info, pr, idx); gen != nil {
			pt = gen
		}
		switch pt.Underlying().(type) {
		case *types.Interface:
			if _, isTP := pt.(*types.TypeParam); isTP {
				return "untargeted:arg-typeparam"
			}
			return "untargeted:arg-interface"
		case *types.Slice:
			if hasTypeParam(pt) {
				return "targeted:arg-generic-slice"
			}
			return "targeted:arg-slice"
		}
		return "untargeted:call-other"
	case *ast.SelectorExpr, *ast.IndexExpr, *ast.SliceExpr, *ast.StarExpr:
		return "untargeted:operand"
	case *ast.RangeStmt:
		return "untargeted:range-operand"
	case *ast.BinaryExpr:
		return "untargeted:binary-operand"
	case *ast.UnaryExpr:
		return "untargeted:unary-operand"
	case *ast.TypeAssertExpr:
		return "untargeted:operand"
	}
	return fmt.Sprintf("other:%T", parent)
}

func genericParam(info *types.Info, call *ast.CallExpr, idx int) types.Type {
	fun := call.Fun
	if ix, ok := fun.(*ast.IndexExpr); ok {
		fun = ix.X
	}
	if ix, ok := fun.(*ast.IndexListExpr); ok {
		fun = ix.X
	}
	var obj types.Object
	switch f := fun.(type) {
	case *ast.Ident:
		obj = info.Uses[f]
	case *ast.SelectorExpr:
		obj = info.Uses[f.Sel]
	}
	fn, ok := obj.(*types.Func)
	if !ok {
		return nil
	}
	sig := fn.Origin().Type().(*types.Signature)
	if sig.TypeParams().Len() == 0 && (sig.Recv() == nil || !hasTypeParam(sig.Recv().Type())) {
		return nil
	}
	params := sig.Params()
	switch {
	case sig.Variadic() && idx >= params.Len()-1:
		if call.Ellipsis.IsValid() {
			return params.At(params.Len() - 1).Type()
		}
		return params.At(params.Len() - 1).Type().(*types.Slice).Elem()
	case idx >= 0 && idx < params.Len():
		return params.At(idx).Type()
	}
	return nil
}

func hasTypeParam(t types.Type) bool {
	switch x := t.(type) {
	case *types.TypeParam:
		return true
	case *types.Slice:
		return hasTypeParam(x.Elem())
	case *types.Pointer:
		return hasTypeParam(x.Elem())
	case *types.Array:
		return hasTypeParam(x.Elem())
	case *types.Map:
		return hasTypeParam(x.Key()) || hasTypeParam(x.Elem())
	case *types.Named:
		ta := x.TypeArgs()
		for i := 0; i < ta.Len(); i++ {
			if hasTypeParam(ta.At(i)) {
				return true
			}
		}
	}
	return false
}
