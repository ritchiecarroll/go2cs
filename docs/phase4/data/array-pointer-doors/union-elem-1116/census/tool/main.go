// W19 census: every conversion `(*[N]E)(x)`, x an unsafe.Pointer (the doors' operand), whose element E is a struct the
// converter's plain reading calls MANAGED only because it holds arrays: under the inline-array reading (csManagedFieldInline:
// a [0]T is a primitive alignment carrier, a nonzero array is managed only if its element is) it is unmanaged. Door B
// declines such an element today. Split by whether every array in it is a [0]T carrier (the ccgo union shape) or a
// nonzero array is held too. The control counts every conversion of an unsafe.Pointer to a pointer to an array.
package main

import (
	"bytes"
	"fmt"
	"go/ast"
	"go/printer"
	"go/token"
	"go/types"
	"os"
	"sort"

	"golang.org/x/tools/go/packages"
)

func envOr(name, fallback string) string {
	if v := os.Getenv(name); v != "" {
		return v
	}
	return fallback
}

// managed mirrors csManagedField (src/go2cs/zeroSizeFieldLayout.go).
func managed(t types.Type, depth int) bool {
	if depth > 12 {
		return true
	}
	switch u := t.Underlying().(type) {
	case *types.Basic:
		return u.Kind() == types.String || u.Kind() == types.UnsafePointer
	case *types.Pointer, *types.Interface, *types.Slice, *types.Map, *types.Chan, *types.Signature:
		return true
	case *types.Array:
		return true
	case *types.Struct:
		for i := range u.NumFields() {
			if managed(u.Field(i).Type(), depth+1) {
				return true
			}
		}
		return false
	}
	return true
}

// managedInline mirrors csManagedFieldInline.
func managedInline(t types.Type, depth int) bool {
	if depth > 12 {
		return true
	}
	switch u := t.Underlying().(type) {
	case *types.Array:
		if u.Len() == 0 {
			return false
		}
		return managedInline(u.Elem(), depth+1)
	case *types.Struct:
		for i := range u.NumFields() {
			if managedInline(u.Field(i).Type(), depth+1) {
				return true
			}
		}
		return false
	}
	return managed(t, depth)
}

// nonzeroArray reports whether t holds a nonzero array at any depth.
func nonzeroArray(t types.Type, depth int) bool {
	if depth > 12 {
		return false
	}
	switch u := t.Underlying().(type) {
	case *types.Array:
		return u.Len() > 0 || nonzeroArray(u.Elem(), depth+1)
	case *types.Struct:
		for i := range u.NumFields() {
			if nonzeroArray(u.Field(i).Type(), depth+1) {
				return true
			}
		}
	}
	return false
}

func main() {
	dir := os.Args[1]
	cfg := &packages.Config{Mode: packages.LoadAllSyntax | packages.NeedModule, Dir: dir,
		Env: append(os.Environ(), "GOOS="+envOr("CENSUS_GOOS", "linux"), "GOARCH="+envOr("CENSUS_GOARCH", "amd64"))}
	pkgs, err := packages.Load(cfg, os.Args[2:]...)
	if err != nil {
		fmt.Println("load:", err)
		os.Exit(1)
	}
	fmt.Printf("roots: %d, errors: %d\n", len(pkgs), packages.PrintErrors(pkgs))
	counts := map[string]int{}
	byType := map[string]int{}
	seen := map[string]bool{}
	files, loaded := 0, 0
	packages.Visit(pkgs, nil, func(p *packages.Package) {
		if p.TypesInfo == nil || p.Module == nil {
			return
		}
		loaded++
		for _, f := range p.Syntax {
			name := p.Fset.Position(f.Pos()).Filename
			if seen[name] {
				continue
			}
			seen[name] = true
			files++
			ast.Inspect(f, func(n ast.Node) bool {
				call, ok := n.(*ast.CallExpr)
				if !ok || len(call.Args) != 1 {
					return true
				}
				tv, ok := p.TypesInfo.Types[call.Fun]
				if !ok || !tv.IsType() {
					return true
				}
				ptr, ok := types.Unalias(tv.Type).Underlying().(*types.Pointer)
				if !ok {
					return true
				}
				arr, ok := types.Unalias(ptr.Elem()).Underlying().(*types.Array)
				if !ok {
					return true
				}
				basic, isBasic := types.Unalias(p.TypesInfo.TypeOf(call.Args[0])).(*types.Basic)
				if !isBasic || basic.Kind() != types.UnsafePointer {
					return true
				}
				counts["control: (*A)(x), x an unsafe.Pointer"]++
				elem := arr.Elem()
				if _, isStruct := elem.Underlying().(*types.Struct); !isStruct {
					return true
				}
				if !managed(elem, 0) {
					counts["struct element, unmanaged in both readings (the door takes it)"]++
					return true
				}
				if managedInline(elem, 0) {
					counts["struct element, managed in both readings (the door declines, rightly)"]++
					return true
				}
				class := "W19: managed only by its arrays, every one a [0]T carrier"
				if nonzeroArray(elem, 0) {
					class = "W19: managed only by its arrays, a nonzero array among them"
				}
				counts[class]++
				var buf bytes.Buffer
				var spelled ast.Node = call.Fun
				if star, ok := ast.Unparen(call.Fun).(*ast.StarExpr); ok {
					if at, ok := star.X.(*ast.ArrayType); ok {
						spelled = at.Elt
					}
				}
				_ = printer.Fprint(&buf, token.NewFileSet(), spelled)
				byType[class+"\t"+buf.String()]++
				return true
			})
		}
	})
	fmt.Printf("packages with a module: %d, files: %d\n", loaded, files)
	var keys []string
	for k := range counts {
		keys = append(keys, k)
	}
	sort.Strings(keys)
	for _, k := range keys {
		fmt.Printf("%6d  %s\n", counts[k], k)
	}
	keys = keys[:0]
	for k := range byType {
		keys = append(keys, k)
	}
	sort.Strings(keys)
	for _, k := range keys {
		fmt.Printf("%6d  %s\n", byType[k], k)
	}
}
