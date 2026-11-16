// W17 census: every conversion `(*A)(x)`, A an array type, whose element type holds a non-empty anonymous struct,
// in the packages loaded from a directory. The class the array pointer doors named with no lift behind it is the
// one whose operand is an unsafe.Pointer (the doors' own operand rule, arrayPointerViewOfBase). The control line
// counts every conversion of an unsafe.Pointer to a pointer to an array, so a zero is read beside a non-zero.
package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"os"
	"sort"

	"golang.org/x/tools/go/packages"
)

func envOr(name string, fallback string) string {
	if value := os.Getenv(name); value != "" {
		return value
	}

	return fallback
}

// holdsAnonStruct reports whether t, through composition and not through a named type, holds a struct type
// with at least one field that is written in place.
func holdsAnonStruct(t types.Type, depth int) bool {
	if depth > 8 {
		return false
	}

	switch typed := types.Unalias(t).(type) {
	case *types.Struct:
		return typed.NumFields() > 0
	case *types.Array:
		return holdsAnonStruct(typed.Elem(), depth+1)
	case *types.Pointer:
		return holdsAnonStruct(typed.Elem(), depth+1)
	case *types.Slice:
		return holdsAnonStruct(typed.Elem(), depth+1)
	case *types.Map:
		return holdsAnonStruct(typed.Key(), depth+1) || holdsAnonStruct(typed.Elem(), depth+1)
	case *types.Chan:
		return holdsAnonStruct(typed.Elem(), depth+1)
	}

	return false
}

func main() {
	dir := os.Args[1]
	cfg := &packages.Config{Mode: packages.LoadAllSyntax | packages.NeedModule, Dir: dir, Tests: os.Getenv("CENSUS_TESTS") == "1",
		Env: append(os.Environ(), "GOOS="+envOr("CENSUS_GOOS", "linux"), "GOARCH="+envOr("CENSUS_GOARCH", "amd64"))}
	pkgs, err := packages.Load(cfg, os.Args[2:]...)

	if err != nil {
		fmt.Println("load:", err)
		os.Exit(1)
	}

	fmt.Printf("roots: %d, errors: %d\n", len(pkgs), packages.PrintErrors(pkgs))
	counts := map[string]int{}
	seen := map[string]bool{}
	var lines []string
	files, loaded := 0, 0

	packages.Visit(pkgs, nil, func(p *packages.Package) {
		if p.TypesInfo == nil {
			return
		}

		if os.Getenv("CENSUS_STD") == "1" && p.Module != nil {
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

				operand := p.TypesInfo.TypeOf(call.Args[0])
				basic, isBasic := types.Unalias(operand).(*types.Basic)
				fromUnsafe := isBasic && basic.Kind() == types.UnsafePointer

				if fromUnsafe {
					counts["control: (*A)(x), x an unsafe.Pointer"]++
				}

				if !holdsAnonStruct(arr.Elem(), 0) {
					return true
				}

				class := "anonymous struct element, other operand"

				if fromUnsafe {
					class = "anonymous struct element, unsafe.Pointer operand (the W17 class)"
				}

				counts[class]++
				pos := p.Fset.Position(call.Pos())
				lines = append(lines, fmt.Sprintf("%s\t%s:%d\t%s", class, pos.Filename, pos.Line, types.TypeString(arr.Elem(), nil)))

				return true
			})
		}
	})

	fmt.Printf("packages: %d, files: %d\n", loaded, files)
	var keys []string

	for k := range counts {
		keys = append(keys, k)
	}

	sort.Strings(keys)

	for _, k := range keys {
		fmt.Printf("%6d  %s\n", counts[k], k)
	}

	sort.Strings(lines)

	for _, l := range lines {
		fmt.Println(l)
	}
}
