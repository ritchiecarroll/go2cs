// gocensus: the Go side of the init-statement scope census. For each Go file named on stdin (one
// "key<TAB>path" per line) it prints, in source pre-order, every statement the converter wraps in
// a scope block: an if, switch or type switch carrying an Init clause, and a bare Go block (a
// BlockStmt sitting directly in a statement list), which the converter also emits as a bare C#
// block. One JSON object per item.
package main

import (
	"bufio"
	"encoding/json"
	"fmt"
	"go/ast"
	"go/parser"
	"go/printer"
	"go/token"
	"os"
	"strings"
)

type item struct {
	Key       string   `json:"key"`
	Line      int      `json:"line"`
	Item      string   `json:"item"`      // if, elseif, switch, typeswitch, block
	Chain     string   `json:"chain"`     // if: none, else, elseif (the chain this if heads)
	Init      string   `json:"init"`      // assert, maplookup, chanrecv, multicall, define1, assign1, parallel, incdec, expr, send, other
	Define    bool     `json:"define"`    // the init declares (:=)
	Names     []string `json:"names"`     // the init's LHS identifiers
	Cond      string   `json:"cond"`      // if: the condition, printed
	CondIsOk  bool     `json:"condIsOk"`  // if: cond is exactly the init's last LHS name
	CondNotOk bool     `json:"condNotOk"` // if: cond is exactly !<last LHS>
	// Name uses inside the if, by region: the condition, the body, the else branch(es).
	UsesCond map[string]int `json:"usesCond,omitempty"`
	UsesBody map[string]int `json:"usesBody,omitempty"`
	UsesElse map[string]int `json:"usesElse,omitempty"`
	FuncLit  bool           `json:"funcLit"` // a func literal anywhere in the statement
	Labeled  bool           `json:"labeled"` // the statement is the target of a label
}

var fset = token.NewFileSet()

func printed(n ast.Node) string {
	var b strings.Builder
	printer.Fprint(&b, fset, n)
	return b.String()
}

func classify(s ast.Stmt) (kind string, define bool, names []string) {
	switch s := s.(type) {
	case *ast.AssignStmt:
		define = s.Tok == token.DEFINE
		for _, l := range s.Lhs {
			if id, ok := l.(*ast.Ident); ok {
				names = append(names, id.Name)
			} else {
				names = append(names, printed(l))
			}
		}
		if len(s.Rhs) == 1 && len(s.Lhs) == 2 {
			switch r := s.Rhs[0].(type) {
			case *ast.TypeAssertExpr:
				return "assert", define, names
			case *ast.IndexExpr:
				return "maplookup", define, names
			case *ast.UnaryExpr:
				if r.Op == token.ARROW {
					return "chanrecv", define, names
				}
			case *ast.CallExpr:
				return "multicall", define, names
			}
		}
		if len(s.Rhs) == 1 && len(s.Lhs) > 2 {
			if _, ok := s.Rhs[0].(*ast.CallExpr); ok {
				return "multicall", define, names
			}
		}
		if len(s.Lhs) == 1 && len(s.Rhs) == 1 {
			if define {
				if u, ok := s.Rhs[0].(*ast.UnaryExpr); ok && u.Op == token.ARROW {
					return "chanrecv1", define, names
				}
				return "define1", define, names
			}
			return "assign1", define, names
		}
		return "parallel", define, names
	case *ast.IncDecStmt:
		return "incdec", false, nil
	case *ast.ExprStmt:
		return "expr", false, nil
	case *ast.SendStmt:
		return "send", false, nil
	}
	return "other", false, nil
}

func uses(n ast.Node, names []string) map[string]int {
	if n == nil || len(names) == 0 {
		return nil
	}
	want := map[string]bool{}
	for _, s := range names {
		want[s] = true
	}
	m := map[string]int{}
	ast.Inspect(n, func(x ast.Node) bool {
		switch x := x.(type) {
		case *ast.SelectorExpr:
			ast.Inspect(x.X, func(y ast.Node) bool {
				if id, ok := y.(*ast.Ident); ok && want[id.Name] {
					m[id.Name]++
				}
				return true
			})
			return false // the selector's Sel is a field or method name, never the local
		case *ast.KeyValueExpr:
			// a composite-literal key may be a field name; count only the value side, and the key
			// when it is not a bare identifier
			if _, ok := x.Key.(*ast.Ident); !ok {
				ast.Inspect(x.Key, func(y ast.Node) bool {
					if id, ok := y.(*ast.Ident); ok && want[id.Name] {
						m[id.Name]++
					}
					return true
				})
			}
			ast.Inspect(x.Value, func(y ast.Node) bool {
				if id, ok := y.(*ast.Ident); ok && want[id.Name] {
					m[id.Name]++
				}
				return true
			})
			return false
		case *ast.Ident:
			if want[x.Name] {
				m[x.Name]++
			}
		}
		return true
	})
	if len(m) == 0 {
		return nil
	}
	return m
}

func hasFuncLit(n ast.Node) bool {
	found := false
	ast.Inspect(n, func(x ast.Node) bool {
		if _, ok := x.(*ast.FuncLit); ok {
			found = true
		}
		return !found
	})
	return found
}

func chainOf(s *ast.IfStmt) string {
	switch s.Else.(type) {
	case nil:
		return "none"
	case *ast.IfStmt:
		return "elseif"
	}
	return "else"
}

func main() {
	out := bufio.NewWriter(os.Stdout)
	defer out.Flush()
	enc := json.NewEncoder(out)
	in := bufio.NewScanner(os.Stdin)
	in.Buffer(make([]byte, 1<<20), 1<<20)
	for in.Scan() {
		parts := strings.SplitN(in.Text(), "\t", 2)
		if len(parts) != 2 {
			continue
		}
		key, path := parts[0], parts[1]
		f, err := parser.ParseFile(fset, path, nil, parser.SkipObjectResolution)
		if err != nil {
			fmt.Fprintf(os.Stderr, "PARSE-FAIL %s: %v\n", key, err)
			continue
		}
		labeled := map[ast.Stmt]bool{}
		elseIfs := map[*ast.IfStmt]bool{}
		ast.Inspect(f, func(n ast.Node) bool {
			if l, ok := n.(*ast.LabeledStmt); ok {
				labeled[l.Stmt] = true
			}
			if s, ok := n.(*ast.IfStmt); ok {
				if e, ok := s.Else.(*ast.IfStmt); ok {
					elseIfs[e] = true
				}
			}
			return true
		})
		// bare blocks: a BlockStmt member of a statement list (a labeled one included)
		bare := map[*ast.BlockStmt]bool{}
		ast.Inspect(f, func(n ast.Node) bool {
			var list []ast.Stmt
			switch s := n.(type) {
			case *ast.BlockStmt:
				list = s.List
			case *ast.CaseClause:
				list = s.Body
			case *ast.CommClause:
				list = s.Body
			}
			for _, st := range list {
				inner := st
				if l, ok := st.(*ast.LabeledStmt); ok {
					inner = l.Stmt
				}
				if b, ok := inner.(*ast.BlockStmt); ok {
					bare[b] = inner != st
				}
			}
			return true
		})
		var visit func(n ast.Node) bool
		visit = func(n ast.Node) bool {
			emit := func(it item) {
				it.Key = key
				it.Line = fset.Position(n.Pos()).Line
				_ = enc.Encode(it)
			}
			switch s := n.(type) {
			case *ast.BlockStmt:
				// emitted at the block itself, so the items stay in source pre-order
				if lab, ok := bare[s]; ok {
					emit(item{Item: "block", Labeled: lab})
				}
			case *ast.IfStmt:
				if s.Init != nil {
					kind, def, names := classify(s.Init)
					it := item{Item: "if", Chain: chainOf(s), Init: kind, Define: def, Names: names,
						Cond: printed(s.Cond), FuncLit: hasFuncLit(s), Labeled: labeled[s]}
					if elseIfs[s] {
						it.Item = "elseif"
					}
					if len(names) > 0 {
						last := names[len(names)-1]
						if id, ok := s.Cond.(*ast.Ident); ok && id.Name == last {
							it.CondIsOk = true
						}
						if u, ok := s.Cond.(*ast.UnaryExpr); ok && u.Op == token.NOT {
							if id, ok := u.X.(*ast.Ident); ok && id.Name == last {
								it.CondNotOk = true
							}
						}
					}
					it.UsesCond = uses(s.Cond, names)
					it.UsesBody = uses(s.Body, names)
					if s.Else != nil {
						it.UsesElse = uses(s.Else, names)
					}
					emit(it)
				}
			case *ast.SwitchStmt:
				if s.Init != nil {
					kind, def, names := classify(s.Init)
					emit(item{Item: "switch", Init: kind, Define: def, Names: names, FuncLit: hasFuncLit(s), Labeled: labeled[s]})
				}
			case *ast.TypeSwitchStmt:
				if s.Init != nil {
					kind, def, names := classify(s.Init)
					emit(item{Item: "typeswitch", Init: kind, Define: def, Names: names, FuncLit: hasFuncLit(s), Labeled: labeled[s]})
				}
			}
			return true
		}
		ast.Inspect(f, visit)
	}
}
