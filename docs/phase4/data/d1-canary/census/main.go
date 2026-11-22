// d1census: per banked row, count concurrency features in the row's own GOROOT
// package directory (package + _test.go files; files selected if they build for
// linux OR windows, amd64; testdata and subdirectories excluded).
package main

import (
	"bufio"
	"fmt"
	"go/ast"
	"go/build"
	"go/parser"
	"go/token"
	"os"
	"path/filepath"
	"sort"
	"strconv"
	"strings"
)

var timerFuncs = map[string]bool{"After": true, "AfterFunc": true, "NewTimer": true, "NewTicker": true, "Tick": true, "Sleep": true}
var netFuncs = map[string]bool{"Listen": true, "ListenTCP": true, "ListenUDP": true, "ListenPacket": true, "ListenUnix": true, "Dial": true, "DialTCP": true, "DialUDP": true, "DialTimeout": true, "Pipe": true}
var ctxFuncs = map[string]bool{"WithCancel": true, "WithCancelCause": true, "WithDeadline": true, "WithDeadlineCause": true, "WithTimeout": true, "WithTimeoutCause": true, "AfterFunc": true}

// feature columns
const (
	fGo = iota
	fSelect
	fChan // send stmt, receive expr, chan type
	fSync // sync.* selector
	fAtomic
	fTimer
	fPipeNet // io.Pipe, net.{Listen,Dial,Pipe}*, httptest.*
	fCtx
	nF
)

var names = []string{"go", "select", "chan", "sync", "atomic", "timer", "pipe/net", "ctx"}

type counts [2][nF]int // [0]=package code, [1]=test code

func main() {
	goroot := os.Args[1]
	roster := os.Args[2]
	f, _ := os.Open(roster)
	sc := bufio.NewScanner(f)
	fmt.Println("package\t" + hdr("pkg") + "\t" + hdr("test"))
	for sc.Scan() {
		pkg := strings.TrimSpace(sc.Text())
		if pkg == "" {
			continue
		}
		c := census(goroot, pkg)
		var cols []string
		for s := 0; s < 2; s++ {
			for i := 0; i < nF; i++ {
				cols = append(cols, strconv.Itoa(c[s][i]))
			}
		}
		fmt.Println(pkg + "\t" + strings.Join(cols, "\t"))
	}
}

func hdr(p string) string {
	var h []string
	for _, n := range names {
		h = append(h, p+":"+n)
	}
	return strings.Join(h, "\t")
}

func matches(dir, name string) bool {
	for _, goos := range []string{"linux", "windows"} {
		ctx := build.Default
		ctx.GOOS, ctx.GOARCH, ctx.CgoEnabled = goos, "amd64", false
		if ok, err := ctx.MatchFile(dir, name); err == nil && ok {
			return true
		}
	}
	return false
}

func census(goroot, pkg string) counts {
	var c counts
	dir := filepath.Join(goroot, "src", pkg)
	ents, _ := os.ReadDir(dir)
	sort.Slice(ents, func(i, j int) bool { return ents[i].Name() < ents[j].Name() })
	self := pkg // the package's own path: its unqualified calls count for its own features
	for _, e := range ents {
		n := e.Name()
		if e.IsDir() || !strings.HasSuffix(n, ".go") || !matches(dir, n) {
			continue
		}
		side := 0
		if strings.HasSuffix(n, "_test.go") {
			side = 1
		}
		fset := token.NewFileSet()
		file, err := parser.ParseFile(fset, filepath.Join(dir, n), nil, parser.SkipObjectResolution)
		if err != nil {
			continue
		}
		imp := map[string]string{} // local name -> import path
		for _, is := range file.Imports {
			p, _ := strconv.Unquote(is.Path.Value)
			local := p[strings.LastIndex(p, "/")+1:]
			if is.Name != nil {
				local = is.Name.Name
			}
			imp[local] = p
		}
		// an internal test package (package x, not x_test) sees its own idents unqualified
		internal := !strings.HasSuffix(file.Name.Name, "_test")
		ast.Inspect(file, func(nd ast.Node) bool {
			switch x := nd.(type) {
			case *ast.GoStmt:
				c[side][fGo]++
			case *ast.SelectStmt:
				c[side][fSelect]++
			case *ast.SendStmt:
				c[side][fChan]++
			case *ast.UnaryExpr:
				if x.Op == token.ARROW {
					c[side][fChan]++
				}
			case *ast.ChanType:
				c[side][fChan]++
			case *ast.SelectorExpr:
				id, ok := x.X.(*ast.Ident)
				if !ok {
					break
				}
				classify(&c, side, imp[id.Name], x.Sel.Name)
			case *ast.CallExpr:
				if id, ok := x.Fun.(*ast.Ident); ok && internal {
					classify(&c, side, self, id.Name)
				}
			}
			return true
		})
	}
	return c
}

func classify(c *counts, side int, path, sel string) {
	switch path {
	case "sync":
		c[side][fSync]++
	case "sync/atomic":
		c[side][fAtomic]++
	case "time":
		if timerFuncs[sel] {
			c[side][fTimer]++
		}
	case "io":
		if sel == "Pipe" {
			c[side][fPipeNet]++
		}
	case "net":
		if netFuncs[sel] {
			c[side][fPipeNet]++
		}
	case "net/http/httptest":
		c[side][fPipeNet]++
	case "context":
		if ctxFuncs[sel] {
			c[side][fCtx]++
		}
	}
}
