// layoutprobe prints, for linux/amd64 and linux/arm64, the size of each named syscall type and the
// offset of every field, computed by go/types from GOROOT's own source (the gc sizes for the arch).
// Nothing is executed for the target: the layout comes from the type checker. S3 census input.
package main

import (
	"fmt"
	"go/ast"
	"go/build"
	"go/importer"
	"go/parser"
	"go/token"
	"go/types"
	"os"
	"path/filepath"
	"sort"
	"strings"
)

func load(arch string) (*types.Package, types.Sizes) {
	ctx := build.Default
	ctx.GOOS, ctx.GOARCH, ctx.CgoEnabled = "linux", arch, false
	bp, err := ctx.Import("syscall", "", 0)
	if err != nil {
		panic(err)
	}
	fset := token.NewFileSet()
	var files []*ast.File
	for _, name := range bp.GoFiles {
		f, err := parser.ParseFile(fset, filepath.Join(bp.Dir, name), nil, 0)
		if err != nil {
			panic(err)
		}
		files = append(files, f)
	}
	build.Default = ctx // the source importer reads build.Default for the packages syscall imports
	sizes := types.SizesFor("gc", arch)
	conf := types.Config{Importer: importer.ForCompiler(fset, "source", nil), Sizes: sizes}
	pkg, err := conf.Check("syscall", fset, files, nil)
	if err != nil {
		panic(err)
	}
	return pkg, sizes
}

func main() {
	names := strings.Split(os.Args[1], ",")
	for _, arch := range []string{"amd64", "arm64"} {
		pkg, sizes := load(arch)
		for _, n := range names {
			obj := pkg.Scope().Lookup(n)
			if obj == nil {
				fmt.Printf("%s\t%s\tABSENT\n", arch, n)
				continue
			}
			t := obj.Type()
			line := fmt.Sprintf("%s\t%s\tsize=%d", arch, n, sizes.Sizeof(t))
			if st, ok := t.Underlying().(*types.Struct); ok {
				var fields []*types.Var
				for i := 0; i < st.NumFields(); i++ {
					fields = append(fields, st.Field(i))
				}
				offs := sizes.Offsetsof(fields)
				var parts []string
				for i, f := range fields {
					parts = append(parts, fmt.Sprintf("%s@%d", f.Name(), offs[i]))
				}
				line += "\t" + strings.Join(parts, " ")
			}
			fmt.Println(line)
		}
		var consts []string
		for _, n := range pkg.Scope().Names() {
			if strings.HasPrefix(n, "SYS_") {
				consts = append(consts, n)
			}
		}
		sort.Strings(consts)
		f, _ := os.Create(fmt.Sprintf("../sysnum-linux-%s.tsv", arch))
		for _, n := range consts {
			c := pkg.Scope().Lookup(n).(*types.Const)
			fmt.Fprintf(f, "%s\t%s\n", n, c.Val())
		}
		f.Close()
	}
}
